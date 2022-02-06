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
using System;

namespace Amplitude.Helpers
{
    public class BlankHotkeysManager : IKeyboardHook
    {
        private static BlankHotkeysManager? _instance;
        public static BlankHotkeysManager Instance { get => _instance ??= new BlankHotkeysManager(); }

        public void Dispose()
        {

        }

        public void SetGlobalStopHotkey(Options options, Action<Options, string> callback)
        {

        }

        public void SetSoundClipHotkey(SoundClip clip, Action<SoundClip, string> callback)
        {

        }
    }
}
