/*
    AmplitudeSoundboard
    Copyright (C) 2021-2022 dan0v
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Amplitude.Helpers
{
    class MSoundEngine : ISoundEngine
    {
        private static MSoundEngine? _instance;
        public static MSoundEngine Instance { get => _instance ??= new MSoundEngine(); }

        public const int SAMPLE_RATE = 44100;

        private readonly object bass_lock = new object();

        private ConcurrentBag<int> streams = new();

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
                List<string> devices = new List<string>();
                devices.Add(ISoundEngine.GLOBAL_DEFAULT_DEVICE_NAME);
                // Index 0 is "No Sound", so skip
                for (int dev = 1; dev < Bass.DeviceCount; dev++)
                {
                    var info = Bass.GetDeviceInfo(dev);
                    devices.Add(info.Name);
                }
                return devices;
            }
        }

        private int? GetOutputPlayerByName(string playerDeviceName)
        {
            if (playerDeviceName == ISoundEngine.GLOBAL_DEFAULT_DEVICE_NAME)
            {
                playerDeviceName = App.OptionsManager.Options.OutputSettings.DeviceName;
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

        private MSoundEngine() { }

        public void Play(SoundClip source)
        {
            if (!BrowseIO.ValidAudioFile(source.AudioFilePath, true, source))
            {
                return;
            }

            foreach (OutputSettings settings in source.OutputSettings)
            {
                Play(source.AudioFilePath, settings.Volume, settings.DeviceName, source.Id);
            }
        }

        public void Play(string fileName, int volume, string playerDeviceName, string id)
        {
            double vol = volume / 100.0;

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
                        streams.Add(stream);
                        Bass.ChannelPlay(stream, false);
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

        public void CheckDeviceExistsAndGenerateErrors(SoundClip clip)
        {
            foreach (OutputSettings settings in clip.OutputSettings)
            {
                if (GetOutputPlayerByName(settings.DeviceName) == null)
                {
                    if (clip != null)
                    {
                        App.WindowManager.ShowErrorSoundClip(clip, ViewModels.ErrorListViewModel.ErrorType.MISSING_DEVICE, settings.DeviceName);
                    }
                }
            }
        }

        public void Reset()
        {
            while (!streams.IsEmpty)
            {
                if (streams.TryTake(out int stream))
                {
                    Bass.StreamFree(stream);
                }
            }
        }

        public void Dispose()
        {
            Reset();
            Bass.Free();
        }
    }
}