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

using AmplitudeSoundboard;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using System;

namespace Amplitude.Helpers
{
    public class ThemeHandler
    {
        private static ThemeHandler _instance;
        public static ThemeHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ThemeHandler();
                }
                return _instance;
            }
        }

        public static string[] ThemesList { get => new string[] { "Dark", "Light" }; }

        public FontFamily TitleFont => FontFamily.Parse("avares://amplitude_soundboard/Assets/Fonts/JosefinSans/#Josefin Sans");
        public FontFamily BodyFont => FontFamily.Parse("avares://amplitude_soundboard/Assets/Fonts/Roboto/#Roboto");

        public Color BorderColor
        {
            get
            {
                switch (SelectedTheme)
                {
                    case "Light":
                        return Color.Parse("#252A36");
                    case "Dark":
                        return Color.Parse("#F08A5D");
                    default:
                        throw new NotImplementedException("Not yet implemented theme: " + SelectedTheme);
                }
            }
        }

        //public Brush BorderBrush => new Brush();

        public Bitmap ArrowLeft { get => getBitmap(folder + "/ArrowLeft.png"); }
        public Bitmap ArrowRight { get => getBitmap(folder + "/ArrowRight.png"); }
        public Bitmap FileBrowse { get => getBitmap(folder + "/FileBrowse.png"); }
        public Bitmap Keyboard { get => getBitmap(folder + "/Keyboard.png"); }
        public Bitmap Settings { get => getBitmap(folder + "/Settings.png"); }
        public Bitmap Play { get => getBitmap(folder + "/Play.png"); }

        // TODO actually refactor this to enum after all to support string localization
        public string SelectedTheme
        {
            get => App.Options.Theme;
            set
            {
                if (!string.IsNullOrEmpty(value) && Array.Exists<string>(ThemesList, t => t == value) && value != SelectedTheme)
                {
                    App.Options.Theme = value;
                    RefreshTheme();
                }
            }
        }

        private static ExperimentalAcrylicMaterial darkAcrylic = new ExperimentalAcrylicMaterial
        {
            BackgroundSource = AcrylicBackgroundSource.Digger,
            TintColor = Color.Parse("Black"),
            TintOpacity = 1,
            MaterialOpacity = 0.8d
        };
        private static ExperimentalAcrylicMaterial lightAcrylic = new ExperimentalAcrylicMaterial
        {
            BackgroundSource = AcrylicBackgroundSource.Digger,
            TintColor = Color.Parse("White"),
            TintOpacity = 1,
            MaterialOpacity = 0.4d
        };

        private static Styles fluentDark = new Styles
        {
            new StyleInclude(new Uri("avares://ControlCatalog/Styles"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/FluentDark.xaml")
            },
        };
        private static Styles fluentLight = new Styles
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
                    case "Light":
                        return lightAcrylic;
                    case "Dark":
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
                    case "Light":
                        fold = "Light";
                        break;
                    case "Dark":
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
                case "Light":
                    App.Current.Styles[0] = fluentLight;
                    break;
                case "Dark":
                    App.Current.Styles[0] = fluentDark;
                    break;
                default:
                    throw new NotImplementedException("Not yet implemented theme: " + SelectedTheme);
            }
        }
    }
}
