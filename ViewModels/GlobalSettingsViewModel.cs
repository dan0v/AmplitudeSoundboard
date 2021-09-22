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

using Amplitude.Helpers;
using Amplitude.Models;
using AmplitudeSoundboard;

namespace Amplitude.ViewModels
{
    public class GlobalSettingsViewModel : ViewModelBase
    {
        static ThemeHandler ThemeHandler { get => App.ThemeHandler; }

        private Options _model;
        public Options Model { get => _model; }

        public GlobalSettingsViewModel()
        {
            _model = App.Options.ShallowCopy();
        }

        public void CreateHotkey()
        {

        }

        public void IncreaseVolumeSmall()
        {
            if (Model.MasterVolume < 100)
            {
                Model.MasterVolume += 1;
            }
        }
        public void DecreaseVolumeSmall()
        {
            if (Model.MasterVolume > 0)
            {
                Model.MasterVolume -= 1;
            }
        }

        public void SaveOptions()
        {
            Model.SaveOptions();
            App.Options = Model;
            _model = Model.ShallowCopy();
            OnPropertyChanged(nameof(Model));
        }
    }
}