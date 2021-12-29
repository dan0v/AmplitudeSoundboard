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
using AmplitudeSoundboard;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using Avalonia.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amplitude.Helpers
{
    class NSoundEngine : ISoundEngine
    {
        private static NSoundEngine? _instance;
        public static NSoundEngine Instance { get => _instance ??= new NSoundEngine(); }

        public List<string> OutputDeviceListWithGlobal
        {
            get
            {
                List<string> devs = new List<string>();
                devs.Add(ISoundEngine.GLOBAL_DEFAULT_DEVICE_NAME);
                devs.Add(ISoundEngine.DEFAULT_DEVICE_NAME);
                for (int n = 0; n < WaveOut.DeviceCount; n++)
                {
                    var caps = WaveOut.GetCapabilities(n);
                    devs.Add(caps.ProductName);
                }
                return devs;
            }
        }

        public List<string> OutputDeviceListWithoutGlobal
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

        private ConcurrentDictionary<string, (IWavePlayer player, MixingSampleProvider mixer)> outputs = new ConcurrentDictionary<string, (IWavePlayer player, MixingSampleProvider mixer)>();

        private const int CHANNEL_COUNT = 2;
        public const int SAMPLE_RATE = 44100;

        private static int activeThreads = 0;
        private static int resetBlockingThreads = 0;

        private ConcurrentDictionary<string, ConcurrentQueue<QueuedPlayback>> processingClips = new ConcurrentDictionary<string, ConcurrentQueue<QueuedPlayback>>();

        private ConcurrentDictionary<string, CachedSound> soundCache = new ConcurrentDictionary<string, CachedSound>();

        private NSoundEngine()
        {
            
        }

        private IWavePlayer? GetOutputPlayerByName(string playerDeviceName)
        {
            if (playerDeviceName == ISoundEngine.GLOBAL_DEFAULT_DEVICE_NAME)
            {
                playerDeviceName = App.OptionsManager.Options.OutputSettings.DeviceName;
            }

            if (playerDeviceName == ISoundEngine.DEFAULT_DEVICE_NAME)
            {
                return new WaveOutEvent();
            }

            if (OutputDeviceListWithoutGlobal.Contains(playerDeviceName))
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
                    outputs.TryAdd(playerDeviceName, (outputDevice, mixer));
                    return (outputDevice, mixer);
                }
                else
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        App.WindowManager.ErrorList.AddErrorString(string.Format(Localization.Localizer.Instance["MissingDeviceString"], playerDeviceName));
                    });
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
        public void CacheSoundClipIfNecessary(SoundClip clip)
        {
            if (!BrowseIO.ValidAudioFile(clip.AudioFilePath))
            {
                return;
            }
            AudioFileReader input = new AudioFileReader(clip.AudioFilePath);
            int sampleRate = input.WaveFormat.SampleRate;
            if (sampleRate != SAMPLE_RATE)
            {
                if (!processingClips.ContainsKey(clip.Id))
                {
                    Interlocked.Increment(ref activeThreads);
                    SetHasActiveThreads();
                    processingClips.TryAdd(clip.Id, new ConcurrentQueue<QueuedPlayback>());
                    new Thread(() =>
                    {
                        CachedSound cachedSound = new CachedSound(input);
                        input.Dispose();

                        if (!string.IsNullOrEmpty(clip.Id))
                        {
                            soundCache.TryAdd(clip.Id, cachedSound);
                        }

                        if (processingClips.TryRemove(clip.Id, out var queue))
                        {
                            foreach (var queuedItem in queue)
                            {
                                if (!queuedItem.cancelled)
                                {
                                    Play(cachedSound, queuedItem.volume, queuedItem.playerDeviceName);
                                }
                            }
                        }
                        Interlocked.Decrement(ref activeThreads);
                        SetHasActiveThreads();
                    })
                    { IsBackground = true }.Start();
                }
            }
        }

        /// <summary>
        /// Play a soundclip with caching and sample rate conversion as neccesary
        /// </summary>
        /// <param name="source"></param>
        public void Play(SoundClip source)
        {
            if (string.IsNullOrEmpty(source.Id) || !BrowseIO.ValidAudioFile(source.AudioFilePath, true, source))
            {
                return;
            }

            if (soundCache.TryGetValue(source.Id, out CachedSound sound))
            {
                foreach (OutputSettings settings in source.OutputSettings)
                {
                    Play(sound, settings.Volume, settings.DeviceName);
                }
            }
            else
            {
                foreach (OutputSettings settings in source.OutputSettings)
                {
                    Play(source.AudioFilePath, settings.Volume, settings.DeviceName, source.Id);
                }
            }
        }

        /// <summary>
        /// Directly play cached sound
        /// </summary>
        /// <param name="sound"></param>
        private void Play(CachedSound sound, int volume, string playerDeviceName)
        {
            var copy = sound.ShallowCopy();
            copy.Volume = (volume / 100f) *(App.OptionsManager.Options.OutputSettings.Volume / 100f);
            AddMixerInput(playerDeviceName, new CachedSoundSampleProvider(copy));
        }

        /// <summary>
        /// Play and optionally cache sound if global setting is set
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="volume"></param>
        /// <param name="id"></param>
        public void Play(string fileName, int volume, string playerDeviceName, string id)
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
                if (!processingClips.ContainsKey(id))
                {
                    Interlocked.Increment(ref activeThreads);
                    Interlocked.Increment(ref resetBlockingThreads);
                    SetCanReset();
                    SetHasActiveThreads();
                    var queue = new ConcurrentQueue<QueuedPlayback>();
                    queue.Enqueue(new QueuedPlayback(volume, playerDeviceName));
                    Interlocked.Decrement(ref resetBlockingThreads);
                    SetCanReset();
                    processingClips.TryAdd(id, queue);
                    new Thread(() =>
                    {
                        CachedSound cachedSound = new CachedSound(input);
                        input.Dispose();

                        if (!string.IsNullOrEmpty(id) && App.OptionsManager.Options.CacheAudio)
                        {
                            soundCache.TryAdd(id, cachedSound);
                        }

                        if (processingClips.TryRemove(id, out var queue))
                        {
                            foreach (var queuedItem in queue)
                            {
                                if (!queuedItem.cancelled)
                                {
                                    Play(cachedSound, queuedItem.volume, queuedItem.playerDeviceName);
                                }
                            }
                        }
                        Interlocked.Decrement(ref activeThreads);
                        SetHasActiveThreads();
                    })
                    { IsBackground = true }.Start();
                }
                else
                {
                    if (processingClips.TryGetValue(id, out var queue))
                    {
                        queue.Enqueue(new QueuedPlayback(volume, playerDeviceName));
                    }
                }
            }
            else
            {
                input.Volume = (volume / 100f) * (App.OptionsManager.Options.OutputSettings.Volume / 100f);
                AddMixerInput(playerDeviceName, new AutoDisposeFileReader(input));
            }
        }

        private void SetCanReset()
        {
            bool canReset = false;
            if (resetBlockingThreads < 0)
            {
                Interlocked.Exchange(ref resetBlockingThreads, 0);
            }
            if (resetBlockingThreads == 0)
            {
                canReset = true;
            }

            ((App)App.Current).CanResetSoundManager = canReset;
        }

        private void SetHasActiveThreads()
        {
            bool hasActiveThreads = true;
            if (activeThreads < 0)
            {
                Interlocked.Exchange(ref activeThreads, 0);
            }
            if (activeThreads == 0)
            {
                hasActiveThreads = false;
            }

            ((App)App.Current).HasActiveSoundManagerThreads = hasActiveThreads;
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

                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        App.WindowManager.ErrorList.AddErrorString(string.Format(Localization.Localizer.Instance["MissingDeviceString"], playerDeviceName));
                    });
                }
            }
            catch (Exception e)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    App.WindowManager.ErrorList.AddErrorString(e.Message);
                });
            }
        }

        public void Reset(bool retainCache = false)
        {
            if (retainCache)
            {
                _instance = new NSoundEngine
                {
                    soundCache = this.soundCache
                };
            }
            foreach (var id in processingClips)
            {
                foreach (var clip in id.Value)
                {
                    clip.cancelled = true;
                }
            }
            this.Dispose();
        }

        public void ResetCache()
        {
            var temp = soundCache;
            soundCache = new ConcurrentDictionary<string, CachedSound>();
            foreach (var item in temp)
            {
                item.Value.Dispose();
            }
            temp.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void RemoveFromCache(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                soundCache.TryRemove(id, out CachedSound? sound);
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
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            App.WindowManager.ErrorList.AddErrorSoundClip(clip, ViewModels.ErrorListViewModel.ErrorType.MISSING_DEVICE, settings.DeviceName);
                        });
                    }
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
    class CachedSound : IDisposable
    {
        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }
        public float Volume = 1f;
        public CachedSound(AudioFileReader audioFileReader)
        {
            var outFormat = new WaveFormat(NSoundEngine.SAMPLE_RATE, audioFileReader.WaveFormat.Channels);
            var resampleWave = new MediaFoundationResampler(audioFileReader, outFormat);
            var resampler = resampleWave.ToSampleProvider();
            resampleWave.Dispose();
            WaveFormat = resampler.WaveFormat;
            Volume = audioFileReader.Volume;

            var wholeFile = new List<float>();
            var readBuffer = new float[WaveFormat.SampleRate * WaveFormat.Channels];
            int samplesRead;
            while ((samplesRead = resampler.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                wholeFile.AddRange(readBuffer.Take(samplesRead));
            }
            AudioData = wholeFile.ToArray();
        }

        public void Dispose()
        {
            this.AudioData = new float[0];
            WaveFormat = null;
        }

        public CachedSound ShallowCopy()
        {
            return (CachedSound)this.MemberwiseClone();
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
            if (cachedSound.AudioData.Length == 0)
            {
                return 0;
            }
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

    class QueuedPlayback
    {
        public int volume;
        public string playerDeviceName;
        public bool cancelled = false;

        public QueuedPlayback(int volume, string playerDeviceName)
        {
            this.volume = volume;
            this.playerDeviceName = playerDeviceName;
        }

    }
}

#endif
