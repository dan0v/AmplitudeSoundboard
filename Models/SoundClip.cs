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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace Amplitude.Models
{
    public class SoundClip : INotifyPropertyChanged
    {
        public readonly string id;

        public ConcurrentBag<Thread> ActiveThreads = new ConcurrentBag<Thread>();

        private float _volume = 100;
        public float Volume {
            get => _volume;
            set
            {
                if (value != _volume)
                {
                    _volume = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _hotkey = "";
        public string Hotkey
        {
            get => _hotkey;
            set
            {
                if (value != _hotkey)
                {
                    _hotkey = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _filePath = "";
        public string FilePath
        {
            get => _filePath;
            set
            {
                if (value != _filePath)
                {
                    _filePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public SoundClip()
        {
            id = "temp";
            // Get unique ID from soundclip manager
        }

        public static SoundClip? FromJSON(string json)
        {
            try
            {
                return (SoundClip?)JsonConvert.DeserializeObject(json, typeof(SoundClip));
            } catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            return null;
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
