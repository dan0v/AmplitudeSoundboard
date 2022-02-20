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

using AmplitudeSoundboard;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amplitude.Models
{
    public class ThemeHandler : INotifyPropertyChanged
    {
        private static ThemeHandler? _instance;
        public static ThemeHandler Instance { get => _instance ??= new ThemeHandler(); }

        private ThemeHandler()
        {
            SelectTheme(App.OptionsManager.Options.ThemeId);
        }

        public void SelectTheme(int selection)
        {
            OnPropertyChanged(nameof(ThemesList));
            if (selection > -1 && selection < ThemesList.Length)
            {
                SelectedTheme = (Theme)selection;
            }
            else
            {
                SelectedTheme = Theme.DARK;
                App.WindowManager.ShowErrorString(string.Format(Localization.Localizer.Instance["InvalidThemeError"], selection));
            }
        }

        public static string[] ThemesList
        {
            get =>
            new string[]
            {
                Localization.Localizer.Instance["DarkTheme"],
                Localization.Localizer.Instance["LightTheme"],
                //Localization.Localizer.Instance["CustomTheme"]
            };
        }

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

        public Color TextBoxHighlightedColor { get => SliderForeground; }

        public Bitmap ArrowLeft { get => getBitmap(folder + "/ArrowLeft.png"); }
        public Bitmap ArrowRight { get => getBitmap(folder + "/ArrowRight.png"); }
        public Bitmap FileBrowse { get => getBitmap(folder + "/FileBrowse.png"); }
        public Bitmap Keyboard { get => getBitmap(folder + "/Keyboard.png"); }
        public Bitmap Settings { get => getBitmap(folder + "/Settings.png"); }
        public Bitmap Play { get => getBitmap(folder + "/Play.png"); }
        public Bitmap Clipboard { get => getBitmap(folder + "/Clipboard.png"); }
        public Bitmap SoundClipList { get => getBitmap(folder + "/SoundClipList.png"); }
        public Bitmap StopSound { get => getBitmap(folder + "/StopSound.png"); }
        public Bitmap Info { get => getBitmap(folder + "/Info.png"); }
        public Bitmap Save { get => getBitmap(folder + "/Save.png"); }
        public Bitmap Delete { get => getBitmap(folder + "/Delete.png"); }
        public Bitmap AddAudio { get => getBitmap(folder + "/AddAudio.png"); }
        public Bitmap Plus { get => getBitmap(folder + "/Plus.png"); }
        public Bitmap Minus { get => getBitmap(folder + "/Minus.png"); }
        public Bitmap ClearCache { get => getBitmap(folder + "/ClearCache.png"); }
        public Bitmap LaunchItem { get => getBitmap(folder + "/LaunchItem.png"); }
        public Bitmap RemoveItem { get => getBitmap(folder + "/Remove.png"); }
        public Bitmap AddToQueue { get => getBitmap(folder + "/AddToQueue.png"); }

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
                }
            }
        }

        private readonly static ExperimentalAcrylicMaterial darkAcrylic = new ExperimentalAcrylicMaterial
        {
            BackgroundSource = AcrylicBackgroundSource.Digger,
            TintColor = Color.Parse("#0b1932"), // Should be #252A36, but material shifts the color, so current value unshifts it 
            TintOpacity = 1,
            MaterialOpacity = 0.8d
        };
        private readonly static ExperimentalAcrylicMaterial lightAcrylic = new ExperimentalAcrylicMaterial
        {
            BackgroundSource = AcrylicBackgroundSource.Digger,
            TintColor = Color.Parse("White"),
            TintOpacity = 1,
            MaterialOpacity = 0.4d
        };

        private static readonly Styles fluentDark = new Styles
        {
            new StyleInclude(new Uri("avares://ControlCatalog/Styles"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/FluentDark.xaml")
            },
        };
        private static readonly Styles fluentLight = new Styles
        {
            new StyleInclude(new Uri("avares://ControlCatalog/Styles"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/FluentLight.xaml")
            },
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

        private IAssetLoader assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();

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

        private Bitmap getBitmap(string uriPath)
        {
            return new Bitmap(assetLoader.Open(new Uri(uriPath)));
        }

        private void RefreshTheme()
        {
            switch (SelectedTheme)
            {
                case Theme.LIGHT:
                    App.Current.Styles[0] = fluentLight;
                    break;
                case Theme.DARK:
                    App.Current.Styles[0] = fluentDark;
                    break;
                default:
                    throw new NotImplementedException("Not yet implemented theme: " + SelectedTheme);
            }
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
