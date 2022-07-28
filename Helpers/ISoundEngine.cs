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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Amplitude.Helpers
{
    public interface ISoundEngine : IDisposable
    {
        public static ISoundEngine Instance { get; }

        public ObservableCollection<PlayingClip> CurrentlyPlaying { get; }
        public ObservableCollection<SoundClip> Queued { get; }

        public const string DEFAULT_DEVICE_NAME = "Default";
        public const string GLOBAL_DEFAULT_DEVICE_NAME = "Global setting";

        public void AddToQueue(SoundClip source);

        public void Play(SoundClip source);

        public void CheckDeviceExistsAndGenerateErrors(OutputProfile outputProfile);

        public List<string> OutputDeviceListWithoutGlobal { get; }

        public List<string> OutputDeviceListWithGlobal { get; }

        public void StopPlaying(int bassId);

        public void RemoveFromQueue(SoundClip clip);

        public void Reset();
    }
}
