/*
    AmplitudeSoundboard
    Copyright (C) 2021-2023 dan0v
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

using Amplitude.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amplitude.Models
{
    public class OutputProfile : INotifyPropertyChanged
    {
        private string _id = null;
        [JsonIgnore]
        public string Id => _id;

        public void InitializeId(string? newId)
        {
            if (string.IsNullOrEmpty(newId))
            {
                newId = DateTimeOffset.Now.ToUnixTimeMilliseconds() + "" + GetHashCode();
            }
            if (string.IsNullOrEmpty(Id))
            {
                _id = newId;
                OnPropertyChanged(nameof(Id));
            }
            else
            {
                throw new NotSupportedException("Do not alter Id once it has been set");
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

        private ObservableCollection<OutputSettings> _outputSettings = new ObservableCollection<OutputSettings>();
        public ObservableCollection<OutputSettings> OutputSettings
        {
            get => _outputSettings;
            set
            {
                if (value != _outputSettings)
                {
                    _outputSettings = value;
                    OnPropertyChanged();
                }
            }
        }

        public OutputProfile(Collection<OutputSettings>? settings = null)
        {
            if (settings == null)
            {
                OutputSettings defaultSettings = new OutputSettings();
                OutputSettings = new ObservableCollection<OutputSettings>(new List<OutputSettings>() { defaultSettings });
                Name = ISoundEngine.DEFAULT_DEVICE_NAME;
            }
            else
            {
                OutputSettings = new ObservableCollection<OutputSettings>(settings);
            }
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public OutputProfile ShallowCopy()
        {
            return (OutputProfile)this.MemberwiseClone();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
