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
using AmplitudeSoundboard;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amplitude.Models
{
    public class OutputSettings : INotifyPropertyChanged
    {
        private int _volume = 100;
        public int Volume
        {
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

        private string _deviceName = ISoundEngine.DEFAULT_DEVICE_NAME;
        public string DeviceName
        {
            get => _deviceName;
            set
            {
                if (value != _deviceName)
                {
                    _deviceName = value;
                    OnPropertyChanged();
                }
            }
        }

        public void IncreaseVolumeSmall()
        {
            if (Volume < 100)
            {
                Volume += 1;
            }
        }
        public void DecreaseVolumeSmall()
        {
            if (Volume > 0)
            {
                Volume -= 1;
            }
        }

        [JsonIgnore]
        public List<string> DeviceList => App.SoundEngine.OutputDeviceListWithGlobal;
        [JsonIgnore]
        public List<string> DeviceListForGlobal => App.SoundEngine.OutputDeviceListWithoutGlobal;

        public OutputSettings ShallowCopy()
        {
            return (OutputSettings)this.MemberwiseClone();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
