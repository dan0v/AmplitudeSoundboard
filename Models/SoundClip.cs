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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using AmplitudeSoundboard;

namespace Amplitude.Models
{
    public class SoundClip : INotifyPropertyChanged
    {
        private string _id = null;
        public string Id
        {
            get => _id;
        }

        public void InitializeId(string newId)
        {
            if (Id == null)
            {
                _id = newId;
                OnPropertyChanged(nameof(Id));
            }
            else
            {
                throw new NotSupportedException("Do not alter Id once it has been set");
            }
        }

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

        private string _audioFilePath = "";
        public string AudioFilePath
        {
            get => _audioFilePath;
            set
            {
                if (value != _audioFilePath)
                {
                    _audioFilePath = value;
                    // Clear possibly cached clip
                    App.SoundEngine.ClearSoundClipCache(Id);
                    OnPropertyChanged();
                }
            }
        }

        private string _imageFilePath = "";
        public string ImageFilePath
        {
            get => _imageFilePath;
            set
            {
                if (value != _imageFilePath)
                {
                    _imageFilePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public SoundClip() { }

        public void PlayAudio()
        {
            App.SoundEngine.Play(this);
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

        public SoundClip ShallowCopy()
        {
            return (SoundClip)this.MemberwiseClone();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
