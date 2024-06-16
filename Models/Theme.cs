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

        public string TextBoxNormalColorString
        {
            get => TextBoxNormalColor.ToString();
            set
            {
                TextBoxNormalColor = Color.Parse(value);
                OnPropertyChanged();
            }
        }


        private Color _textBoxHighlightedColor = ThemeManager.DefaultDarkTextBoxHighlightedColor;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Color TextBoxHighlightedColor
        {
            get => _textBoxHighlightedColor;
            set
            {
                _textBoxHighlightedColor = value;
                OnPropertyChanged();
            }
        }

        public string TextBoxHighlightedColorString
        {
            get => TextBoxHighlightedColor.ToString();
            set
            {
                TextBoxHighlightedColor = Color.Parse(value);
                OnPropertyChanged();
            }
        }

        private Color _sliderForegroundColor = ThemeManager.DefaultDarkSliderForegroundColor;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Color SliderForegroundColor
        {
            get => _sliderForegroundColor;
            set
            {
                _sliderForegroundColor = value;
                OnPropertyChanged();
            }
        }

        public string SliderForegroundColorString
        {
            get => SliderForegroundColor.ToString();
            set
            {
                SliderForegroundColor = Color.Parse(value);
                OnPropertyChanged();
            }
        }

        private Color _sliderBackgroundColor = ThemeManager.DefaultDarkSliderBackgroundColor;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Color SliderBackgroundColor
        {
            get => _sliderBackgroundColor;
            set
            {
                _sliderBackgroundColor = value;
                OnPropertyChanged();
            }
        }

        public string SliderBackgroundColorString
        {
            get => SliderBackgroundColor.ToString();
            set
            {
                SliderBackgroundColor = Color.Parse(value);
                OnPropertyChanged();
            }
        }

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

        public string FadedTextBackgroundColorString
        {
            get => FadedTextBackgroundColor.ToString();
            set
            {
                FadedTextBackgroundColor = Color.Parse(value);
                OnPropertyChanged();
            }
        }

        private Color _borderColor = ThemeManager.DefaultDarkBorderColor;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                OnPropertyChanged();
            }
        }

        public string BorderColorString
        {
            get => BorderColor.ToString();
            set
            {
                BorderColor = Color.Parse(value);
                OnPropertyChanged();
            }
        }

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
            TextBoxNormalColor = ThemeManager.DefaultDarkTextBoxNormalColor;
            TextBoxHighlightedColor = ThemeManager.DefaultDarkTextBoxHighlightedColor;
            SliderForegroundColor = ThemeManager.DefaultDarkSliderForegroundColor;
            SliderBackgroundColor = ThemeManager.DefaultDarkSliderBackgroundColor;
            FadedTextBackgroundColor = ThemeManager.DefaultDarkFadedTextBackgroundColor;
            BorderColor = ThemeManager.DefaultDarkBorderColor;
            WindowBackgroundColor = ThemeManager.DefaultDarkWindowBackgroundColor;
            WindowBackgroundOpacity = ThemeManager.DefaultDarkWindowBackgroundOpacity;
        }

        public void ResetDefaultLightTheme()
        {
            SelectedThemeBase = ThemeBase.LIGHT;
            TextBoxNormalColor = ThemeManager.DefaultLightTextBoxNormalColor;
            TextBoxHighlightedColor = ThemeManager.DefaultLightTextBoxHighlightedColor;
            SliderForegroundColor = ThemeManager.DefaultLightSliderForegroundColor;
            SliderBackgroundColor = ThemeManager.DefaultLightSliderBackgroundColor;
            FadedTextBackgroundColor = ThemeManager.DefaultLightFadedTextBackgroundColor;
            BorderColor = ThemeManager.DefaultLightBorderColor;
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
