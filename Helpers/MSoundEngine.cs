﻿/*
    AmplitudeSoundboard
    Copyright (C) 2021-2025 dan0v
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
using ManagedBass;
using ManagedBass.Mix;
using System;
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
        public static ISoundEngine Instance => _instance ??= new MSoundEngine();

        private readonly object currentlyPlayingLock = new();

        private ObservableCollection<PlayingClip> _currentlyPlaying = [];
        public ObservableCollection<PlayingClip> CurrentlyPlaying => _currentlyPlaying;

        private readonly object queueLock = new();

        private ObservableCollection<SoundClip> _queued = [];
        public ObservableCollection<SoundClip> Queued => _queued;


        private const long TIMER_MS = 200;
        private Timer timer = new(TIMER_MS)
        {
            AutoReset = true,
        };

        private void RefreshPlaybackProgressAndCheckQueue(object? sender, ElapsedEventArgs e)
        {
            lock (currentlyPlayingLock)
            {
                List<PlayingClip> toRemove = [];
                foreach (var track in CurrentlyPlaying)
                {
                    track.CurrentPos += TIMER_MS * 0.001d;
                    if (track.ProgressPct == 1)
                    {
                        toRemove.Add(track);
                    }
                }

                foreach (var track in toRemove)
                {
                    if (track.LoopClip)
                    {
                        track.CurrentPos = 0;
                        Bass.ChannelPlay(track.BassStreamId, true);
                    }
                    else
                    {
                        Bass.StreamFree(track.BassStreamId);
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


        public List<string> OutputDeviceListWithoutGlobal
        {
            get
            {
                var all = OutputDeviceListWithGlobal.ToList();
                all.RemoveAt(0);
                return all;
            }
        }

        public List<string> OutputDeviceListWithGlobal
        {
            get
            {
                List<string> devices = [];
                // Index 0 is "No Sound", so skip
                for (int dev = 1; Bass.GetDeviceInfo(dev, out DeviceInfo info); dev++)
                {
                    devices.Add(info.Name);
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

            if (OutputDeviceListWithoutGlobal.Contains(playerDeviceName))
            {
                for (int n = 0; n < Bass.DeviceCount; n++)
                {
                    var info = Bass.GetDeviceInfo(n);
                    if (playerDeviceName == info.Name)
                    {
                        return n;
                    }
                }
            }
            return null;
        }

        private MSoundEngine()
        {
            timer.Elapsed += RefreshPlaybackProgressAndCheckQueue;
            timer.Start();
        }

        public void AddToQueue(SoundClip source)
        {
            lock (queueLock)
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
                    Bass.StreamFree(clip.BassStreamId);
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

        private void Play(string fileName, int volume, int volumeMultiplier, string playerDeviceName, bool loopClip, string soundClipId, string? name = null)
        {
            double vol = (volume / 100.0) * (volumeMultiplier / 100.0);

            int? devId = GetOutputPlayerByName(playerDeviceName);

            if (!devId.HasValue)
            {
                App.WindowManager.ShowErrorString(string.Format(Localization.Localizer.Instance["MissingDeviceString"], playerDeviceName));
                return;
            }

            bool streamError = false;
            bool bassError = false;

            lock (bass_lock)
            {
                // Init device
                if (Bass.Init(devId.Value, SAMPLE_RATE) || Bass.LastError == Errors.Already)
                {
                    Bass.CurrentDevice = devId.Value;
                    int mixer = BassMix.CreateMixerStream(SAMPLE_RATE, 2, BassFlags.Default);
                    int stream = Bass.CreateStream(fileName);

                    Bass.ChannelSetAttribute(stream, ChannelAttribute.Volume, vol);
                    BassMix.MixerAddChannel(mixer, stream, BassFlags.AutoFree | BassFlags.MixerChanDownMix);
                    Bass.ChannelPlay(mixer);

                    if (stream != 0)
                    {
                        // Track active streams so they can be stopped
                        try
                        {
                            var len = Bass.ChannelGetLength(stream, PositionFlags.Bytes);
                            double length = Bass.ChannelBytes2Seconds(stream, len);
                            PlayingClip track = new(string.IsNullOrEmpty(name) ? Path.GetFileNameWithoutExtension(fileName) ?? "" : name, soundClipId, playerDeviceName, stream, length, loopClip);

                            lock (currentlyPlayingLock)
                            {
                                CurrentlyPlaying.Add(track);
                            }
                            Bass.ChannelPlay(stream, false);
                        }
                        catch (Exception)
                        {
                            App.WindowManager.ShowErrorString(string.Format(Localization.Localizer.Instance["FileBadFormatString"], fileName));
                        }
                    }
                    else
                    {
                        streamError = true;
                    }
                }
                else
                {
                    bassError = true;
                }
            }
            if (streamError)
            {
                App.WindowManager.ShowErrorString($"Stream error: {Bass.LastError}");
            }
            if (bassError)
            {
                App.WindowManager.ShowErrorString($"ManagedBass error: {Bass.LastError}");
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
            lock (queueLock)
            {
                Queued.Clear();
            }
            lock (currentlyPlayingLock)
            {
                foreach (var stream in CurrentlyPlaying)
                {
                    Bass.StreamFree(stream.BassStreamId);
                }

                CurrentlyPlaying.Clear();
            }
        }

        public void StopPlaying(int bassId)
        {
            lock (currentlyPlayingLock)
            {
                PlayingClip? track = CurrentlyPlaying.FirstOrDefault(c => c.BassStreamId == bassId);
                if (track != null)
                {
                    CurrentlyPlaying.Remove(track);
                }
                Bass.StreamFree(bassId);
            }
        }

        public void RemoveFromQueue(SoundClip clip)
        {
            lock (queueLock)
            {
                Queued.Remove(clip);
            }
        }

        public void Dispose()
        {
            timer.Stop();
            timer.Elapsed -= RefreshPlaybackProgressAndCheckQueue;
            Reset();
            Bass.Free();
        }
    }
}