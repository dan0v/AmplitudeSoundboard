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

using Amplitude.Helpers;
using Amplitude.Models;
using Avalonia.Media;
using System.Linq;

namespace Amplitude.ViewModels
{
    public sealed class ThemeSettingsViewModel : ViewModelBase
    {
        private Theme _model;
        public Theme Model => _model;
        public static string[] Languages => Localization.Localizer.Languages.Keys.ToArray();

        public ThemeSettingsViewModel()
        {
            _model = ThemeManager.Theme.ShallowCopy();
            _windowBackgroundOpacity = Model.WindowBackgroundOpacity.ToString();
        }

        private string _windowBackgroundOpacity = "";
        public string WindowBackgroundOpacity
        {
            get => _windowBackgroundOpacity;
            set
            {
                _windowBackgroundOpacity = value;
                if (double.TryParse(value, out double val) && val != Model.WindowBackgroundOpacity)
                {
                    Model.WindowBackgroundOpacity = val;
                    Model.RefreshAcrylic();
                }
            }
        }

        public int SelectedThemeBase
        {
            get => (int)Model.SelectedThemeBase;
            set
            {
                if (value != (int)Model.SelectedThemeBase)
                {
                    Model.SelectedThemeBase = (ThemeBase)value;
                }
            }
        }

        public void SaveConfig()
        {
            ThemeManager.SaveAndOverwriteTheme(Model);
            _model = Model.ShallowCopy();
            ThemeManager.RefreshTheme();
            OnPropertyChanged(nameof(Model));
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}