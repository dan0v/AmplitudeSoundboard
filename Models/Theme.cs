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
using Amplitude.Localization;
using AmplitudeSoundboard;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Amplitude.Models
{
    public class Theme : INotifyPropertyChanged
    {
        
        private ThemeBase _selectedThemeBase = ThemeBase.DARK;
        public ThemeBase SelectedThemeBase
        {
            get => _selectedThemeBase;
            set
            {
                _selectedThemeBase = value;
                OnPropertyChanged();
            }
        }

        private Color _accentColor = ThemeManager.DefaultDarkAccentColor;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Color AccentColor
        {
            get => _accentColor;
            set
            {
                if (value != _accentColor)
                {
                    _accentColor = value;
                    _accentColorBrush = new(_accentColor);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AccentColorBrush));
                }
            }
        }

        public string AccentColorString
        {
            get => AccentColor.ToString();
            set
            {
                AccentColor = Color.Parse(value);
                OnPropertyChanged();
            }
        }

        private SolidColorBrush _accentColorBrush = new();
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public SolidColorBrush AccentColorBrush => _accentColorBrush;

        //private HighlightBrush _accentColorSelectionBrush = new();
        //[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        //public SolidColorBrush AccentColorSelectionBrush => _accentColorSelectionBrush;


        private Color _secondaryColor = ThemeManager.DefaultDarkSecondaryColor;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Color SecondaryColor
        {
            get => _secondaryColor;
            set
            {
                _secondaryColor = value;
                OnPropertyChanged();
            }
        }

        public string SecondaryColorString
        {
            get => SecondaryColor.ToString();
            set
            {
                SecondaryColor = Color.Parse(value);
                OnPropertyChanged();
            }
        }

        private Color _textBoxNormalColor = ThemeManager.DefaultDarkTextBoxNormalColor;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Color TextBoxNormalColor
        {
            get => _textBoxNormalColor;
            set
            {
                _textBoxNormalColor = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Color TextBoxHighlightedColor => SecondaryColor;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Color SliderForegroundColor => AccentColor;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Color SliderBackgroundColor => SecondaryColor;

        private Color _fadedTextBackgroundColor = ThemeManager.DefaultDarkFadedTextBackgroundColor;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Color FadedTextBackgroundColor
        {
            get => _fadedTextBackgroundColor;
            set
            {
                _fadedTextBackgroundColor = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Color BorderColor => SecondaryColor;

        private Color _windowBackgroundColor = ThemeManager.DefaultDarkWindowBackgroundColor;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Color WindowBackgroundColor
        {
            get => _windowBackgroundColor;
            set
            {
                _windowBackgroundColor = value;
                OnPropertyChanged();
                RefreshAcrylic();
            }
        }

        public string WindowBackgroundColorString
        {
            get => WindowBackgroundColor.ToString();
            set
            {
                WindowBackgroundColor = Color.Parse(value);
                OnPropertyChanged();
            }
        }

        private double _windowBackgroundOpacity = ThemeManager.DefaultDarkWindowBackgroundOpacity;
        public double WindowBackgroundOpacity
        {
            get => _windowBackgroundOpacity;
            set
            {
                _windowBackgroundOpacity = value;
                OnPropertyChanged();
                RefreshAcrylic();
            }
        }

        private ExperimentalAcrylicMaterial _acrylic = ThemeManager.DefaultDarkAcrylic;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public ExperimentalAcrylicMaterial Acrylic => _acrylic;

        public void RefreshAcrylic()
        {
            _acrylic = new ExperimentalAcrylicMaterial
            {
                BackgroundSource = AcrylicBackgroundSource.Digger,
                TintColor = WindowBackgroundColor,
                TintOpacity = 1,
                MaterialOpacity = WindowBackgroundOpacity
            };
            OnPropertyChanged(nameof(Acrylic));
        }

        public void ResetDefaultDarkTheme()
        {
            SelectedThemeBase = ThemeBase.DARK;
            AccentColor = ThemeManager.DefaultDarkAccentColor;
            SecondaryColor = ThemeManager.DefaultDarkSecondaryColor;
            TextBoxNormalColor = ThemeManager.DefaultDarkTextBoxNormalColor;
            FadedTextBackgroundColor = ThemeManager.DefaultDarkFadedTextBackgroundColor;
            WindowBackgroundColor = ThemeManager.DefaultDarkWindowBackgroundColor;
            WindowBackgroundOpacity = ThemeManager.DefaultDarkWindowBackgroundOpacity;
        }

        public void ResetDefaultLightTheme()
        {
            SelectedThemeBase = ThemeBase.LIGHT;
            AccentColor = ThemeManager.DefaultLightAccentColor;
            SecondaryColor = ThemeManager.DefaultLightSecondaryColor;
            TextBoxNormalColor = ThemeManager.DefaultLightTextBoxNormalColor;
            FadedTextBackgroundColor = ThemeManager.DefaultLightFadedTextBackgroundColor;
            WindowBackgroundColor = ThemeManager.DefaultLightWindowBackgroundColor;
            WindowBackgroundOpacity = ThemeManager.DefaultLightWindowBackgroundOpacity;
        }


        public Theme ShallowCopy()
        {
            return (Theme)this.MemberwiseClone();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum ThemeBase
    {
        DARK = 0,
        LIGHT = 1
    }
}
