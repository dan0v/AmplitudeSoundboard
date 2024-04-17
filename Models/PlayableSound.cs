/*
    AmplitudeSoundboard
    Copyright (C) 2021-2024 dan0v
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

using AmplitudeSoundboard;
using Avalonia.Input;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amplitude.Models
{
    public class PlayableSound : SoundFile, INotifyPropertyChanged
    {
        public string Name { get; init; }
        public string SoundClipId { get; init; }
        public string OutputDevice { get; init; }
        public bool LoopClip { get; init; }
        public string ToolTip => $"{Name} - {OutputDevice}";

        private double _currentPos = 0;

        public double CurrentPos
        {
            get => _currentPos;
            set
            {
                if (value != _currentPos)
                {
                    _currentPos = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ProgressPct));
                }
            }
        }

        public float ProgressPct => (float)Math.Clamp(CurrentPos / Duration.TotalMilliseconds, 0, 1);

        public new void Play(bool restart = false)
        {
            if (restart)
            {
                _currentPos = 0f;
            }
            base.Play(restart);
        }

        public void StopPlayback()
        {
            App.SoundEngine.StopPlaying(this);
        }

        public PlayableSound(string filePath, int deviceId, float volume, string soundClipName, string soundClipId, string outputDevice, bool loopClip): base(filePath, deviceId)
        {
            Name = soundClipName;
            SoundClipId = soundClipId;
            OutputDevice = outputDevice;
            LoopClip = loopClip;
            Volume = volume;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
