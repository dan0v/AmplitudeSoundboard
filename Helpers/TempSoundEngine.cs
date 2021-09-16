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

using System;
using Amplitude.Models;

namespace Amplitude.Helpers
{
    class TempSoundEngine : ISoundEngine
    {
        private static TempSoundEngine? _instance;
        public static TempSoundEngine Instance {
            get
            {
                if (_instance == null)
                {
                    _instance = new TempSoundEngine();
                }
                return _instance;
            }
        }

        private TempSoundEngine()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool CheckPlayableFileAndGenerateErrors(string fileName)
        {
            throw new NotImplementedException();
        }

        public void Play(SoundClip source)
        {
            throw new NotImplementedException();
        }

        public void Play(string fileName, float volume)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
