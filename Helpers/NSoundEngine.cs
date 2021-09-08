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

using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using Amplitude.Models;
using NAudio.Wave.SampleProviders;

namespace Amplitude.Helpers
{
    class NSoundEngine : ISoundEngine
    {
        private static NSoundEngine? _instance;
        public static NSoundEngine Instance {
            get
            {
                if (_instance == null)
                {
                    _instance = new NSoundEngine();
                }
                return _instance;
            }
        }
        private readonly IWavePlayer outputDevice;
        private readonly MixingSampleProvider mixer;

        private NSoundEngine(int sampleRate = 44100, int channelCount = 2)
        {
            outputDevice = new WaveOutEvent();
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            mixer.ReadFully = true;
            outputDevice.Init(mixer);
            outputDevice.Play();
        }

        public void Dispose()
        {
            outputDevice.Dispose();
        }

        public void Play(SoundClip source)
        {
            Play(source.FilePath, source.Volume);
        }



        /// <summary>
        /// Credit to Mark Heath
        /// mark.heath@gmail.com
        /// https://gist.github.com/markheath/8783999
        /// </summary>
        public void Play(string fileName, float volume)
        {
            var input = new AudioFileReader(fileName);
            input.Volume = volume / 100f; // Altered from original
            AddMixerInput(new AutoDisposeFileReader(input));
        }
        /// <summary>
        /// Credit to Mark Heath
        /// mark.heath@gmail.com
        /// https://gist.github.com/markheath/8783999
        /// </summary>
        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
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
        private void AddMixerInput(ISampleProvider input)
        {
            mixer.AddMixerInput(ConvertToRightChannelCount(input));
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
}
