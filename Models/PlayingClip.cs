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

using AmplitudeSoundboard;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amplitude.Models
{
    public class PlayingClip : INotifyPropertyChanged
    {
        public string Name { get; init; }
        public string OutputDevice { get; init; }
        public double Length { get; init; }
        public int BassStreamId { get; init; }
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
        public float ProgressPct
        {
            get
            {
                if (CurrentPos > Length)
                {
                    return 1;
                }
                return (float)(CurrentPos / Length);
            }
        }

        public void StopPlayback()
        {
            App.SoundEngine.StopPlaying(BassStreamId);
        }

        public PlayingClip(string name, string outputDevice, int bassStreamId, double length, bool loopClip)
        {
            if (length == 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            Name = name;
            OutputDevice = outputDevice;
            BassStreamId = bassStreamId;
            Length = length;
            LoopClip = loopClip;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
