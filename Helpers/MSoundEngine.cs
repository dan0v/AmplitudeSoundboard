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
using PortAudioSharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Timers;

namespace Amplitude.Helpers
{
    class MSoundEngine : ISoundEngine
    {
        private static MSoundEngine? _instance;
        public static MSoundEngine Instance => _instance ??= new MSoundEngine();

        object currentlyPlayingLock = new object();

        private ObservableCollection<PlayingClip> _currentlyPlaying = new ObservableCollection<PlayingClip>();
        public ObservableCollection<PlayingClip> CurrentlyPlaying => _currentlyPlaying;

        object queueLock = new object();

        private ObservableCollection<SoundClip> _queued = new ObservableCollection<SoundClip>();
        public ObservableCollection<SoundClip> Queued => _queued;


        private const long TIMER_MS = 200;
        private Timer timer = new Timer(TIMER_MS)
        {
            AutoReset = true,
        };

        private void RefreshPlaybackProgressAndCheckQueue(object? sender, ElapsedEventArgs e)
        {
            lock(currentlyPlayingLock)
            {
                List<PlayingClip> toRemove = [];
                foreach(var track in CurrentlyPlaying)
                {
                    track.CurrentPos += TIMER_MS * 0.001d;
                    if (track.ProgressPct == 1)
                    {
                        toRemove.Add(track);
                    }
                }
                
                foreach(var track in toRemove)
                {
                    if (track.LoopClip)
                    {
                        if (track.SoundFile?.IsPlaying != true) {
                            track.CurrentPos = 0;
                            track.SoundFile?.Play();
                        }
                    }
                    else
                    {
                        track.SoundFile?.Pause();
                        track.SoundFile?.Dispose();
                        CurrentlyPlaying.Remove(track);
                    }
                }
            }
            lock (queueLock)
            {
                if (!CurrentlyPlaying.Any() && Queued.Any())
                {
                    var clip = Queued[0];
                    Play(clip, true);
                    Queued.RemoveAt(0);
                }
            }
        }

        public const int SAMPLE_RATE = 44100;

        private readonly object bass_lock = new object();


        public List<string> OutputDeviceList
        {
            get
            {
               List<string> devices = [];
                for (int i = 0; i < PortAudio.DeviceCount; i++) {
                    try {
                        devices.Add(PortAudio.GetDeviceInfo(i).name);
                    } catch {
                        return devices;
                    }
                }
                return devices;
            }
        }

        private int? GetOutputPlayerByName(string playerDeviceName)
        {
            if (playerDeviceName == ISoundEngine.GLOBAL_DEFAULT_DEVICE_NAME)
            {
                playerDeviceName = ISoundEngine.DEFAULT_DEVICE_NAME;
            }

            if (playerDeviceName == ISoundEngine.DEFAULT_DEVICE_NAME || playerDeviceName == "System default")
            {
                return 1;
            }

            var index = OutputDeviceList.IndexOf(playerDeviceName);
            return index == -1 ? null : index;
        }

        private MSoundEngine()
        {
            PortAudio.Initialize();
            timer.Elapsed += RefreshPlaybackProgressAndCheckQueue;
            timer.Start();
        }

        public void AddToQueue(SoundClip source)
        {
            lock(queueLock)
            {
                Queued.Add(source.ShallowCopy());
            }
        }

        private void StopAndRemoveFromQueue(string id)
        {
            lock (queueLock)
            {
                var toRemove = Queued.Where(clip => clip.Id == id).ToList();
                foreach (var clip in toRemove)
                {
                    Queued.Remove(clip);
                }
            }
            lock (currentlyPlayingLock)
            {
                var toRemove = CurrentlyPlaying.Where(clip => clip.SoundClipId == id).ToList();
                foreach (var clip in toRemove)
                {
                    clip.SoundFile?.Pause();
                    clip.SoundFile?.Dispose();
                    CurrentlyPlaying.Remove(clip);
                }
            }
        }

        private bool ClipPlayingOrQueued(string id)
        {
            lock (queueLock)
            {
                if (Queued.Any(clip => clip.Id == id))
                {
                    return true;
                }
            }
            lock (currentlyPlayingLock)
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

            lock (bass_lock)
            {

                var sound = new SoundFile(fileName, (int)devId)
                {
                    volume = (float)vol
                };

                PlayingClip track = new (string.IsNullOrEmpty(name) ? Path.GetFileNameWithoutExtension(fileName) ?? "" : name, soundClipId, playerDeviceName, loopClip);

                lock(currentlyPlayingLock)
                {
                    CurrentlyPlaying.Add(track);
                }

                sound.Play();
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
            lock(queueLock)
            {
                Queued.Clear();
            }
            lock(currentlyPlayingLock)
            {
                foreach (var track in CurrentlyPlaying)
                {
                    track.SoundFile?.Pause();
                    track.SoundFile?.Dispose();
                }

                CurrentlyPlaying.Clear();
            }
        }

        public void StopPlaying(SoundFile? soundFile)
        {
            if (soundFile == null) {
                return;
            }

            lock (currentlyPlayingLock)
            {
                PlayingClip? track = CurrentlyPlaying.FirstOrDefault(c => c.SoundFile == soundFile);
                if (track != null)
                {
                    CurrentlyPlaying.Remove(track);
                }
                soundFile.Pause();
                soundFile.Dispose();
            }
        }

        public void RemoveFromQueue(SoundClip clip)
        {
            lock(queueLock)
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