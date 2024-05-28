/*
    AmplitudeSoundboard
    Copyright (C) 2021-2024 dan0v
    https://git.dan0v.com/AmplitudeSoundboard

    This file is part of AmplitudeSoundboard.

    AmplitudeSoundboard is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AmplitudeSoundboard is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with AmplitudeSoundboard.  If not, see <https://www.gnu.org/licenses/>.
*/

using Amplitude.Models;
using AmplitudeSoundboard;
using Avalonia.Controls.Primitives;
using DynamicData;
using PortAudioSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Timers;

namespace Amplitude.Helpers
{
    class PSoundEngine : ISoundEngine
    {
        private static PSoundEngine? _instance;
        public static PSoundEngine Instance => _instance ??= new PSoundEngine();

        private readonly object _currentlyPlayingLock = new();

        private ObservableCollection<PlayableSound> _currentlyPlaying = [];
        public ObservableCollection<PlayableSound> CurrentlyPlaying => _currentlyPlaying;

        private readonly object _queueLock = new();

        private ObservableCollection<SoundClip> _queued = [];
        public ObservableCollection<SoundClip> Queued => _queued;

        private const long TIMER_MS = 200;
        private Timer timer = new(TIMER_MS)
        {
            AutoReset = true,
        };

        private void RefreshPlaybackProgressAndCheckQueue(object? sender, ElapsedEventArgs e)
        {
            lock(_currentlyPlayingLock)
            {
                List<PlayableSound> toRemove = [];
                foreach(var track in CurrentlyPlaying)
                {
                    track.CurrentPos += TIMER_MS;
                    if (track.ProgressPct == 1)
                    {
                        toRemove.Add(track);
                    }
                }
                
                foreach(var track in toRemove)
                {
                    if (track.LoopClip)
                    {
                        if (track.IsPlaying != true) {
                            track.Play(true);
                        }
                    }
                    else
                    {
                        track.Pause();
                        track.Dispose();
                        CurrentlyPlaying.Remove(track);
                    }
                }
            }
            lock (_queueLock)
            {
                if (!CurrentlyPlaying.Any() && Queued.Any())
                {
                    var clip = Queued[0];
                    Play(clip, true);
                    Queued.RemoveAt(0);
                }
            }
        }

        private Dictionary<string, int> OutputDevices
        {
            get
            {
                Dictionary<string, int> devices = [];
                for (int i = 0; i < PortAudio.DeviceCount; i++)
                {
                    try
                    {
                        var device = PortAudio.GetDeviceInfo(i);
                        if (device.maxOutputChannels > 0 && device.maxInputChannels == 0)
                        {
                            devices.Add(device.name, i);
                        }
                    }
                    catch { }
                }
                return devices;
            }
        }

        public List<string> OutputDeviceList => [.. OutputDevices.Keys];

        private int? GetOutputPlayerByName(string playerDeviceName)
        {
            if (playerDeviceName == ISoundEngine.GLOBAL_DEFAULT_DEVICE_NAME)
            {
                playerDeviceName = ISoundEngine.DEFAULT_DEVICE_NAME;
            }

            if (playerDeviceName == ISoundEngine.DEFAULT_DEVICE_NAME || playerDeviceName == "System default" || !OutputDevices.ContainsKey(playerDeviceName))
            {
                return 1;
            }

            var index = OutputDevices[playerDeviceName];
            return index == -1 ? null : index;
        }

        private PSoundEngine()
        {
            PortAudio.Initialize();
            timer.Elapsed += RefreshPlaybackProgressAndCheckQueue;
            timer.Start();
        }

        public void AddToQueue(SoundClip source)
        {
            lock(_queueLock)
            {
                Queued.Add(source.ShallowCopy());
            }
        }

        private void StopAndRemoveFromQueue(string id)
        {
            lock (_queueLock)
            {
                foreach (var clip in Queued.Where(clip => clip.Id == id).ToList())
                {
                    Queued.Remove(clip);
                }
            }
            var toRemove = CurrentlyPlaying.Where(clip => clip.SoundClipId == id).ToList();
            RemoveFromCurrentlyPlaying(toRemove);
        }

        private bool ClipPlayingOrQueued(string id)
        {
            lock (_queueLock)
            {
                if (Queued.Any(clip => clip.Id == id))
                {
                    return true;
                }
            }
            lock (_currentlyPlayingLock)
            {
                if (CurrentlyPlaying.Any(clip => clip.SoundClipId == id))
                {
                    return true;
                }
            }
            return false;
        }

        public void Play(SoundClip source, bool fromQueue = false)
        {
            var tempId = source.Id ?? source.AudioFilePath;
            if (!fromQueue && App.ConfigManager.Config.StopAudioOnRepeatTrigger && ClipPlayingOrQueued(tempId))
            {
                StopAndRemoveFromQueue(tempId);
                return;
            }

            if (!BrowseIO.ValidAudioFile(source.AudioFilePath, true, source))
            {
                return;
            }

            foreach (OutputSettings settings in source.OutputSettingsFromProfile)
            {
                Play(source.AudioFilePath, settings.Volume, source.Volume, settings.DeviceName, source.LoopClip, tempId, source.Name);
            }
        }

        private void Play(string fileName, int volume, int volumeMultiplier, string playerDeviceName, bool loopClip, string soundClipId, string ? name = null)
        {
            double vol = (volume / 100.0) * (volumeMultiplier / 100.0);

            int? devId = GetOutputPlayerByName(playerDeviceName);

            if (!devId.HasValue)
            {
                App.WindowManager.ShowErrorString(string.Format(Localization.Localizer.Instance["MissingDeviceString"], playerDeviceName));
                return;
            }

            try
            {
                PlayableSound track = new(fileName, (int)devId, (float)vol, string.IsNullOrEmpty(name) ? Path.GetFileNameWithoutExtension(fileName) ?? "" : name, soundClipId, playerDeviceName, loopClip);

                lock (_currentlyPlayingLock)
                {
                    CurrentlyPlaying.Add(track);
                }

                track.Play();
            }
            catch (Exception e)
            {
                App.WindowManager.ShowErrorString($"problem: {e.Message}");
            }
        }

        public void CheckDeviceExistsAndGenerateErrors(OutputProfile profile)
        {
            foreach (OutputSettings settings in profile.OutputSettings)
            {
                if (GetOutputPlayerByName(settings.DeviceName) == null)
                {
                    if (profile != null)
                    {
                        App.WindowManager.ShowErrorOutputProfile(profile, ViewModels.ErrorListViewModel.OutputProfileErrorType.MISSING_DEVICE, settings.DeviceName);
                    }
                }
            }
        }

        public void Reset()
        {
            lock(_queueLock)
            {
                Queued.Clear();
            }
            RemoveFromCurrentlyPlaying(CurrentlyPlaying.ToList());
        }

        private void RemoveFromCurrentlyPlaying(IEnumerable<PlayableSound> toRemove)
        {
            lock (_currentlyPlayingLock)
            {
                foreach (var track in toRemove)
                {
                    track.Pause();
                    track.Dispose();
                }

                CurrentlyPlaying.RemoveMany(toRemove);
            }
        }

        public void StopPlaying(PlayableSound? track)
        {
            if (track == null) {
                return;
            }

            lock (_currentlyPlayingLock)
            {
                CurrentlyPlaying.Remove(track);
                track.Pause();
                track.Dispose();
            }
        }

        public void RemoveFromQueue(SoundClip clip)
        {
            lock(_queueLock)
            {
                Queued.Remove(clip);
            }
        }

        public void Dispose()
        {
            timer.Stop();
            timer.Elapsed -= RefreshPlaybackProgressAndCheckQueue;
            Reset();
            PortAudio.Terminate();
        }
    }
}