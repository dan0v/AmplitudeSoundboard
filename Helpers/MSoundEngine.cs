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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using ManagedBass;
using ManagedBass.Mix;

namespace Amplitude.Helpers
{
    class MSoundEngine : ISoundEngine
    {
        private static MSoundEngine? _instance;
        public static MSoundEngine Instance { get => _instance ??= new MSoundEngine(); }

        public const int SAMPLE_RATE = 44100;

        private ConcurrentBag<int> streams = new();

        public List<string> OutputDeviceListWithoutGlobal
        {
            get
            {
                var all = OutputDeviceListWithGlobal.ToList();
                all.RemoveAt(1);
                return all;
            }
        }

        public List<string> OutputDeviceListWithGlobal
        {
            get
            {
                List<string> devices = new List<string>();
                devices.Add(ISoundEngine.DEFAULT_DEVICE_NAME);
                devices.Add(ISoundEngine.GLOBAL_DEFAULT_DEVICE_NAME);
                for (int dev = 0; dev < Bass.DeviceCount; dev++)
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

            if (playerDeviceName == ISoundEngine.DEFAULT_DEVICE_NAME)
            {
                return -1;
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
            if (string.IsNullOrEmpty(source.Id) || !BrowseIO.ValidAudioFile(source.AudioFilePath, true, source))
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
            // TODO test BASS dependence on threading for multi-device playback
            new Thread(() =>
           {
               double vol = volume / 100.0;

               int? devId = GetOutputPlayerByName(playerDeviceName);

               if (!devId.HasValue)
               {
                   App.WindowManager.ErrorList.AddErrorString(string.Format(Localization.Localizer.Instance["MissingDeviceString"], playerDeviceName));
                   return;
               }

               // Init device
               if (Bass.Init(devId.Value, SAMPLE_RATE) || Bass.LastError == Errors.Already)
               {
                   int mixer = BassMix.CreateMixerStream(SAMPLE_RATE, 2, BassFlags.Default);

                   int stream = Bass.CreateStream(fileName);

                   // TODO investigate volume levels
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
                       App.WindowManager.ErrorList.AddErrorString($"Stream error: {Bass.LastError}");
                   }
               }
               else
               {
                   App.WindowManager.ErrorList.AddErrorString($"ManagedBass error: {Bass.LastError}");
               }
           })
            {
                IsBackground = true
            }.Start();
            
        }

        public void RemoveFromCache(string id)
        {
            // Caching unused
        }

        public void CacheSoundClipIfNecessary(SoundClip clip)
        {
            // Caching unused
        }

        public void CheckDeviceExistsAndGenerateErrors(SoundClip clip)
        {
            foreach (OutputSettings settings in clip.OutputSettings)
            {
                if (GetOutputPlayerByName(settings.DeviceName) == null)
                {
                    if (clip != null)
                    {
                        App.WindowManager.ErrorList.AddErrorSoundClip(clip, ViewModels.ErrorListViewModel.ErrorType.MISSING_DEVICE, settings.DeviceName);
                    }
                }
            }
        }

        public void Reset(bool retainCache = false)
        {
            while(!streams.IsEmpty)
            {
                if (streams.TryTake(out int stream))
                {
                    Bass.StreamFree(stream);
                }
            }
        }

        public void ResetCache()
        {
            // Caching unused
        }

        public void Dispose()
        {
            Reset();
            Bass.Free();
        }
    }
}