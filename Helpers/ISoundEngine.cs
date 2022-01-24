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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Amplitude.Models;

namespace Amplitude.Helpers
{
    public interface ISoundEngine: IDisposable
    {
        public static ISoundEngine Instance { get; }

        public const string DEFAULT_DEVICE_NAME = "System default";
        public const string GLOBAL_DEFAULT_DEVICE_NAME = "Global setting";

        public void Play(SoundClip source);

        public void Play(string fileName, int volume, string playerDeviceName, string id);

        public void CheckDeviceExistsAndGenerateErrors(SoundClip clip);

        public List<string> OutputDeviceListWithoutGlobal { get; }

        public List<string> OutputDeviceListWithGlobal { get; }

        public void Reset(bool retainCache = false);
    }
}
