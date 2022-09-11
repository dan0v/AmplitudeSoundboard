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

using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amplitude.Models
{
    public class OutputProfile : INotifyPropertyChanged
    {
        [JsonIgnore]
        public string Id { get; protected set; }

        public void OverrideId(string newId)
        {
            Id = newId;
            OnPropertyChanged(nameof(Id));
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
            if (settings != null)
            {
                OutputSettings = new ObservableCollection<OutputSettings>(settings);
            }

            Id = DateTimeOffset.Now.ToUnixTimeMilliseconds() + "" + GetHashCode();
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
