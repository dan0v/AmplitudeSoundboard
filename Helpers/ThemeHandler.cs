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

using AmplitudeSoundboard;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amplitude.Helpers
{
    public class ThemeHandler : INotifyPropertyChanged
    {
        private static ThemeHandler? _instance;
        public static ThemeHandler Instance => _instance ??= new ThemeHandler();

        private ThemeHandler()
        {
            SelectTheme(App.ConfigManager.Config.ThemeId);
        }

        public void SelectTheme(int selection)
        {
            if (selection > -1 && selection < ThemesList.Length)
            {
                SelectedTheme = (Theme)selection;
            }
            else
            {
                SelectedTheme = Theme.DARK;
                App.WindowManager.ShowErrorString(string.Format(Localization.Localizer.Instance["InvalidThemeError"], selection));
            }
            OnPropertyChanged(nameof(ThemesList));
        }

        public static string[] ThemesList => new string[]
            {
                Localization.Localizer.Instance["DarkTheme"],
                Localization.Localizer.Instance["LightTheme"],
                //Localization.Localizer.Instance["CustomTheme"]
            };

        public FontFamily TitleFont => FontFamily.Parse("avares://amplitude_soundboard/Assets/Fonts/JosefinSans/#Josefin Sans");
        public FontFamily BodyFont => FontFamily.Parse("avares://amplitude_soundboard/Assets/Fonts/NotoSansDisplay/");


        public Color FadedTextBackgroundColor
        {
            get
            {
                switch (SelectedTheme)
                {
                    case Theme.LIGHT:
                        return Color.Parse("#BFBFC1");
                    case Theme.DARK:
                        return Color.Parse("#171A21");
                    default:
                        throw new NotImplementedException("Not yet implemented theme: " + SelectedTheme);
                }
            }
        }

        public Color BorderColor
        {
            get
            {
                switch (SelectedTheme)
                {
                    case Theme.LIGHT:
                        return Color.Parse("#F08A5D");
                    case Theme.DARK:
                        return Color.Parse("#F08A5D");
                    default:
                        throw new NotImplementedException("Not yet implemented theme: " + SelectedTheme);
                }
            }
        }

        public Color SliderForeground
        {
            get
            {
                switch (SelectedTheme)
                {
                    case Theme.LIGHT:
                        return Color.Parse("#252A36");
                    case Theme.DARK:
                        return Color.Parse("#EBAEFF");
                    default:
                        throw new NotImplementedException("Not yet implemented theme: " + SelectedTheme);
                }
            }
        }

        public Color SliderBackground
        {
            get
            {
                switch (SelectedTheme)
                {
                    case Theme.LIGHT:
                        return Color.Parse("#EBAEFF");
                    case Theme.DARK:
                        return Color.Parse("#F08A5D");
                    default:
                        throw new NotImplementedException("Not yet implemented theme: " + SelectedTheme);
                }
            }
        }

        public Color TextBoxNormalColor
        {
            get
            {
                switch (SelectedTheme)
                {
                    case Theme.LIGHT:
                        return Color.Parse("#66ffffff");
                    case Theme.DARK:
                        return Color.Parse("#66000000");
                    default:
                        throw new NotImplementedException("Not yet implemented theme: " + SelectedTheme);
                }
            }
        }

        public Color TextBoxHighlightedColor => SliderForeground;

        public Bitmap ArrowLeft => GetBitmap(folder + "/ArrowLeft.png");
        public Bitmap ArrowRight => GetBitmap(folder + "/ArrowRight.png");
        public Bitmap FileBrowse => GetBitmap(folder + "/FileBrowse.png");
        public Bitmap Keyboard => GetBitmap(folder + "/Keyboard.png");
        public Bitmap Settings => GetBitmap(folder + "/Settings.png");
        public Bitmap Play => GetBitmap(folder + "/Play.png");
        public Bitmap Clipboard => GetBitmap(folder + "/Clipboard.png");
        public Bitmap SoundClipList => GetBitmap(folder + "/SoundClipList.png");
        public Bitmap StopSound => GetBitmap(folder + "/StopSound.png");
        public Bitmap Info => GetBitmap(folder + "/Info.png");
        public Bitmap Save => GetBitmap(folder + "/Save.png");
        public Bitmap Delete => GetBitmap(folder + "/Delete.png");
        public Bitmap AddAudio => GetBitmap(folder + "/AddAudio.png");
        public Bitmap Plus => GetBitmap(folder + "/Plus.png");
        public Bitmap Minus => GetBitmap(folder + "/Minus.png");
        public Bitmap ClearCache => GetBitmap(folder + "/ClearCache.png");
        public Bitmap LaunchItem => GetBitmap(folder + "/LaunchItem.png");
        public Bitmap RemoveItem => GetBitmap(folder + "/Remove.png");
        public Bitmap AddToQueue => GetBitmap(folder + "/AddToQueue.png");
        public Bitmap Language => GetBitmap(folder + "/Language.png");

        private Theme _selectedTheme;
        public Theme SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (value != _selectedTheme)
                {
                    _selectedTheme = value;
                    RefreshTheme();
                    foreach(var p in typeof(ThemeHandler).GetProperties())
                    {
                        OnPropertyChanged(p.Name);
                    }
                }
            }
        }

        private readonly static ExperimentalAcrylicMaterial darkAcrylic = new ExperimentalAcrylicMaterial
        {
            BackgroundSource = AcrylicBackgroundSource.Digger,
            TintColor = Color.Parse("#0b1932"), // Should be #252A36, but material shifts the color, so current value unshifts it 
            TintOpacity = 1,
            MaterialOpacity = 0.7d
        };
        private readonly static ExperimentalAcrylicMaterial lightAcrylic = new ExperimentalAcrylicMaterial
        {
            BackgroundSource = AcrylicBackgroundSource.Digger,
            TintColor = Color.Parse("White"),
            TintOpacity = 1,
            MaterialOpacity = 0.5d
        };

        public ExperimentalAcrylicMaterial Acrylic
        {
            get
            {
                switch (SelectedTheme)
                {
                    case Theme.LIGHT:
                        return lightAcrylic;
                    case Theme.DARK:
                        return darkAcrylic;
                    default:
                        throw new NotImplementedException("Not yet implemented theme: " + SelectedTheme);
                }
            }
        }

        private string folder
        {
            get
            {
                string prefix = "avares://amplitude_soundboard/Assets/img";
                string fold;
                switch (SelectedTheme)
                {
                    case Theme.LIGHT:
                        fold = "Light";
                        break;
                    case Theme.DARK:
                        fold = "Dark";
                        break;
                    default:
                        throw new NotImplementedException("Not yet implemented theme: " + SelectedTheme);
                }

                return prefix + "/" + fold;
            }
        }

        private Bitmap GetBitmap(string uriPath)
        {
            return new Bitmap(AssetLoader.Open(new Uri(uriPath)));
        }

        private void RefreshTheme()
        {
            if (App.Current == null)
            {
                return;
            }
            
            App.Current.RequestedThemeVariant = SelectedTheme switch
            {
                Theme.LIGHT => ThemeVariant.Light,
                Theme.DARK => ThemeVariant.Dark,
                Theme.CUSTOM => ThemeVariant.Dark,
                _ => ThemeVariant.Dark,
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum Theme
    {
        DARK = 0,
        LIGHT = 1,
        CUSTOM = 2
    }
}
