/*
    AmplitudeSoundboard
    Copyright (C) 2021 dan0v
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

#if Windows

using NAudio.Wave;
using System;
using Amplitude.Models;
using NAudio.Wave.SampleProviders;
using System.IO;
using AmplitudeSoundboard;
using System.Collections.Generic;
using System.Linq;

namespace Amplitude.Helpers
{
    class NSoundEngine : ISoundEngine
    {
        private static NSoundEngine? _instance;
        public static NSoundEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NSoundEngine();
                }
                return _instance;
            }
        }

        public List<string> OutputDeviceList
        {
            get
            {
                List<string> devs = new List<string>();
                devs.Add(ISoundEngine.DEFAULT_DEVICE_NAME);
                for (int n = 0; n < WaveOut.DeviceCount; n++)
                {
                    var caps = WaveOut.GetCapabilities(n);
                    devs.Add(caps.ProductName);
                }
                return devs;
            }
        }

        private Dictionary<string, (IWavePlayer player, MixingSampleProvider mixer)> outputs = new Dictionary<string, (IWavePlayer player, MixingSampleProvider mixer)>();

        private const int CHANNEL_COUNT = 2;
        public const int SAMPLE_RATE = 44100;

        private Dictionary<string, CachedSound> soundCache = new Dictionary<string, CachedSound>();

        private NSoundEngine()
        {
            
        }

        private IWavePlayer? GetOutputPlayerByName(string playerDeviceName)
        {

            if (playerDeviceName == ISoundEngine.DEFAULT_DEVICE_NAME)
            {
                return new WaveOutEvent();
            }
            else if (OutputDeviceList.Contains(playerDeviceName))
            {
                for (int n = 0; n < WaveOut.DeviceCount; n++)
                {
                    var caps = WaveOut.GetCapabilities(n);
                    if (playerDeviceName == caps.ProductName)
                    {
                        return new WaveOutEvent() { DeviceNumber = n };
                    }
                }
            }
            return null;
        }

        private (IWavePlayer, MixingSampleProvider)? GetOrInitializePlayer(string playerDeviceName)
        {
            if (outputs.TryGetValue(playerDeviceName, out (IWavePlayer player, MixingSampleProvider mixer) device))
            {
                return device;
            }
            else
            {
                IWavePlayer? outputDevice = GetOutputPlayerByName(playerDeviceName);
                if (outputDevice != null)
                {
                    var mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(SAMPLE_RATE, CHANNEL_COUNT));
                    mixer.ReadFully = true;
                    outputDevice.Init(mixer);
                    outputDevice.Play();
                    outputs.Add(playerDeviceName, (outputDevice, mixer));
                    return (outputDevice, mixer);
                }
                else
                {
                    App.WindowManager.ErrorListWindow.AddErrorString(string.Format(Localization.Localizer.Instance["MissingDeviceString"], playerDeviceName));
                }
            }
            return null;
        }

        public void Dispose()
        {
            foreach ((IWavePlayer player, MixingSampleProvider mixer) in outputs.Values)
            {
                player.Dispose();
            }
        }

        /// <summary>
        /// Resample and cache this soundclip
        /// </summary>
        /// <param name="clip"></param>
        public void PreCacheSoundClip(SoundClip clip)
        {
            if (!soundCache.ContainsKey(clip.Id))
            {
                using (AudioFileReader input = new AudioFileReader(clip.AudioFilePath))
                {
                    CachedSound cachedSound = new CachedSound(input);
                    soundCache.Add(clip.Id, cachedSound);
                }
            }
        }

        /// <summary>
        /// Play a soundclip with caching and sample rate conversion as neccesary
        /// </summary>
        /// <param name="source"></param>
        public void Play(SoundClip source)
        {
            if (!string.IsNullOrEmpty(source.Id) && soundCache.TryGetValue(source.Id, out CachedSound sound))
            {
                Play(sound, source.DeviceName, source.Volume);
            }
            else
            {
                // TODO Do not cache all audio files for now, maybe pass Id if required
                Play(source.AudioFilePath, source.Volume, source.DeviceName, null);
            }
        }

        /// <summary>
        /// Directly play cached sound
        /// </summary>
        /// <param name="sound"></param>
        private void Play(CachedSound sound, string playerDeviceName, int volume)
        {
            sound.Volume = (volume / 100f) *(App.OptionsManager.Options.MasterVolume / 100f);
            AddMixerInput(playerDeviceName, new CachedSoundSampleProvider(sound));
        }

        /// <summary>
        /// Optionally cache sound if SoundClip Id is provided
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="volume"></param>
        /// <param name="id"></param>
        public void Play(string fileName, int volume, string playerDeviceName, string? id = null)
        {
            if (!BrowseIO.ValidAudioFile(fileName))
            {
                return;
            }

            AudioFileReader input = new AudioFileReader(fileName);
            int sampleRate = input.WaveFormat.SampleRate;

            // If sample rates don't match, resample audio clip and optionally keep it cached
            if (sampleRate != SAMPLE_RATE)
            {
                CachedSound cachedSound = new CachedSound(input);

                if (!string.IsNullOrEmpty(id))
                {
                    // Recheck dictionary just in case
                    if (!soundCache.ContainsKey(id))
                    {
                        soundCache.Add(id, cachedSound);
                    }
                }

                Play(cachedSound, playerDeviceName, volume);
            }
            else
            {
                input.Volume = (volume / 100f) * (App.OptionsManager.Options.MasterVolume / 100f);
                AddMixerInput(playerDeviceName, new AutoDisposeFileReader(input));
            }
        }
        /// <summary>
        /// Credit to Mark Heath
        /// mark.heath@gmail.com
        /// https://gist.github.com/markheath/8783999
        /// </summary>
        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input, MixingSampleProvider mixer)
        {
            if (input.WaveFormat.Channels == mixer.WaveFormat.Channels)
            {
                return input;
            }
            if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }
            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }
        /// <summary>
        /// Credit to Mark Heath
        /// mark.heath@gmail.com
        /// https://gist.github.com/markheath/8783999
        /// </summary>
        private void AddMixerInput(string playerDeviceName, ISampleProvider input)
        {
            try
            {
                (IWavePlayer player, MixingSampleProvider mixer)? device = GetOrInitializePlayer(playerDeviceName);

                if (device != null)
                {
                    device?.mixer.AddMixerInput(ConvertToRightChannelCount(input, device?.mixer));
                }
                else
                {
                    App.WindowManager.ErrorListWindow.AddErrorString(string.Format(Localization.Localizer.Instance["MissingDeviceString"], playerDeviceName));
                }
            }
            catch (Exception e)
            {
                App.WindowManager.ErrorListWindow.AddErrorString(e.Message);
            }
        }

        public void Reset(bool retainCache = false)
        {
            if (retainCache)
            {
                _instance = new NSoundEngine();
                _instance.soundCache = this.soundCache;
            }
            this.Dispose();
        }

        public void ClearSoundClipCache(string id)
        {
            if (!string.IsNullOrEmpty(id) && soundCache.ContainsKey(id))
            {
                soundCache.Remove(id);
            }
        }

        public void CheckDeviceExistsAndGenerateErrors(SoundClip clip)
        {
            if (GetOutputPlayerByName(clip.DeviceName) == null)
            {
                if (clip != null)
                {
                    App.WindowManager.ErrorListWindow.AddErrorSoundClip(clip, Views.ErrorList.ErrorType.MISSING_DEVICE);
                }
            }
        }
    }

    /// <summary>
    /// Credit to Mark Heath
    /// mark.heath@gmail.com
    /// https://gist.github.com/markheath/8783999
    /// </summary>
    class AutoDisposeFileReader : ISampleProvider
    {
        private readonly AudioFileReader reader;
        private bool isDisposed;
        public AutoDisposeFileReader(AudioFileReader reader)
        {
            this.reader = reader;
            this.WaveFormat = reader.WaveFormat;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (isDisposed)
                return 0;
            int read = reader.Read(buffer, offset, count);
            if (read == 0)
            {
                reader.Dispose();
                isDisposed = true;
            }
            return read;
        }

        public WaveFormat WaveFormat { get; private set; }
    }

    /// <summary>
    /// Credit to Mark Heath
    /// mark.heath@gmail.com
    /// https://gist.github.com/markheath/8783999
    /// Altered to add volume and resampling
    /// </summary>
    class CachedSound
    {
        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }
        public float Volume = 1f;
        public CachedSound(AudioFileReader audioFileReader)
        {
            var resampler = new WdlResamplingSampleProvider(audioFileReader, NSoundEngine.SAMPLE_RATE);

            WaveFormat = resampler.WaveFormat;
            Volume = audioFileReader.Volume;

            var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
            var readBuffer = new float[WaveFormat.SampleRate * WaveFormat.Channels];
            int samplesRead;
            while ((samplesRead = resampler.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                wholeFile.AddRange(readBuffer.Take(samplesRead));
            }
            AudioData = wholeFile.ToArray();
        }
    }

    /// <summary>
    /// Credit to Mark Heath
    /// mark.heath@gmail.com
    /// https://gist.github.com/markheath/8783999
    /// Altered to add volume
    /// </summary>
    class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound cachedSound;
        private long position;

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            this.cachedSound = cachedSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = cachedSound.AudioData.Length - position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(cachedSound.AudioData, position, buffer, offset, samplesToCopy);

            // Shift sample volume
            for (long index = offset; index < samplesToCopy; index++)
            {
                buffer[index] *= cachedSound.Volume;
            }

            position += samplesToCopy;
            return (int)samplesToCopy;
        }

        public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
    }
}

#endif
