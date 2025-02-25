/*
    AmplitudeSoundboard
    Copyright (C) 2021-2025 dan0v
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

using Amplitude.Models;
using AmplitudeSoundboard;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Amplitude.Helpers
{
    [JsonSerializable(typeof(Theme))]
    public partial class ThemeManagerContext : JsonSerializerContext { }

    public class ThemeManager : INotifyPropertyChanged
    {
        private static ThemeManager? _instance;
        public static ThemeManager Instance => _instance ??= new ThemeManager();

        private const string THEME_FILE_LOCATION = "theme.json";

        private Theme _theme = new();
        public Theme Theme => _theme;

        private ThemeManager()
        {
            var theme = RetrieveThemeFromFile();
            if (theme != null)
            {
                _theme = theme;
            }

            RefreshTheme();
        }

        // Light Defaults
        public static readonly Color DefaultLightAccentColor = Color.Parse("#F08A5D");
        public static readonly Color DefaultLightSecondaryColor = Color.Parse("#252A36");
        public static readonly Color DefaultLightTextBoxNormalColor = Color.Parse("#66ffffff");
        public static readonly Color DefaultLightTextBoxHighlightedColor = DefaultLightSecondaryColor;
        public static readonly Color DefaultLightSliderForegroundColor = DefaultLightSecondaryColor;
        public static readonly Color DefaultLightSliderBackgroundColor = DefaultLightAccentColor;
        public static readonly Color DefaultLightFadedTextBackgroundColor = Color.Parse("#BFBFC1");
        public static readonly Color DefaultLightBorderColor = DefaultLightAccentColor;
        public static readonly Color DefaultLightWindowBackgroundColor = Color.Parse("White");
        public static double DefaultLightWindowBackgroundOpacity => FeatureManager.IsFeatureEnabled(FeatureManager.Feature.BACKGROUND_OPACTIY) ? 0.5d : 1;

        public static readonly ExperimentalAcrylicMaterial DefaultLightAcrylic = new()
        {
            BackgroundSource = AcrylicBackgroundSource.Digger,
            TintColor = DefaultLightWindowBackgroundColor,
            TintOpacity = 1,
            MaterialOpacity = DefaultLightWindowBackgroundOpacity
        };

        // Dark Defaults
        public static readonly Color DefaultDarkAccentColor = Color.Parse("#F08A5D");
        public static readonly Color DefaultDarkSecondaryColor = Color.Parse("#EBAEFF");
        public static readonly Color DefaultDarkTextBoxNormalColor = Color.Parse("#66000000");
        public static readonly Color DefaultDarkTextBoxHighlightedColor = DefaultDarkSecondaryColor;
        public static readonly Color DefaultDarkSliderForegroundColor = DefaultDarkSecondaryColor;
        public static readonly Color DefaultDarkSliderBackgroundColor = DefaultDarkAccentColor;
        public static readonly Color DefaultDarkFadedTextBackgroundColor = Color.Parse("#171A21");
        public static readonly Color DefaultDarkBorderColor = DefaultDarkAccentColor;
        public static readonly Color DefaultDarkWindowBackgroundColor = Color.Parse("#0b1932");
        public static double DefaultDarkWindowBackgroundOpacity => FeatureManager.IsFeatureEnabled(FeatureManager.Feature.BACKGROUND_OPACTIY) ? 0.7d : 1;
        public static readonly ExperimentalAcrylicMaterial DefaultDarkAcrylic = new()
        {
            BackgroundSource = AcrylicBackgroundSource.Digger,
            TintColor = DefaultDarkWindowBackgroundColor,
            TintOpacity = 1,
            MaterialOpacity = DefaultDarkWindowBackgroundOpacity
        };

        public static string[] ThemesBaseList =>
        [
            Localization.Localizer.Instance["DarkTheme"],
            Localization.Localizer.Instance["LightTheme"],
        ];

        public FontFamily TitleFont => FontFamily.Parse("avares://amplitude_soundboard/Assets/Fonts/JosefinSans/#Josefin Sans");
        public FontFamily BodyFont => FontFamily.Parse("avares://amplitude_soundboard/Assets/Fonts/OpenSans/#Open Sans");

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
        public Bitmap Reset => GetBitmap(folder + "/Reset.png");

        private string folder
        {
            get
            {
                string prefix = "avares://amplitude_soundboard/Assets/img";
                string fold;
                switch (Theme.SelectedThemeBase)
                {
                    case ThemeBase.LIGHT:
                        fold = "Light";
                        break;
                    default:
                        fold = "Dark";
                        break;
                }

                return prefix + "/" + fold;
            }
        }

        private Bitmap GetBitmap(string uriPath)
        {
            return new Bitmap(AssetLoader.Open(new Uri(uriPath)));
        }

        public void RefreshTheme()
        {
            if (App.Current == null)
            {
                return;
            }

            App.Current.RequestedThemeVariant = Theme.SelectedThemeBase switch
            {
                ThemeBase.LIGHT => ThemeVariant.Light,
                ThemeBase.DARK => ThemeVariant.Dark,
                _ => ThemeVariant.Dark,
            };

            Theme.RefreshAcrylic();
            OnPropertyChanged(nameof(Theme));
            OnPropertyChanged(nameof(ArrowLeft));
            OnPropertyChanged(nameof(ArrowRight));
            OnPropertyChanged(nameof(FileBrowse));
            OnPropertyChanged(nameof(Keyboard));
            OnPropertyChanged(nameof(Settings));
            OnPropertyChanged(nameof(Play));
            OnPropertyChanged(nameof(Clipboard));
            OnPropertyChanged(nameof(SoundClipList));
            OnPropertyChanged(nameof(StopSound));
            OnPropertyChanged(nameof(Info));
            OnPropertyChanged(nameof(Save));
            OnPropertyChanged(nameof(Delete));
            OnPropertyChanged(nameof(AddAudio));
            OnPropertyChanged(nameof(Plus));
            OnPropertyChanged(nameof(Minus));
            OnPropertyChanged(nameof(ClearCache));
            OnPropertyChanged(nameof(LaunchItem));
            OnPropertyChanged(nameof(RemoveItem));
            OnPropertyChanged(nameof(AddToQueue));
            OnPropertyChanged(nameof(Language));
            OnPropertyChanged(nameof(Reset));
        }



        public void SaveAndOverwriteTheme(Theme theme)
        {
            try
            {
                var json = App.JsonIoManager.ConvertObjectsToJSON(theme);
                App.JsonIoManager.SaveJSONToFile(THEME_FILE_LOCATION, json);
                _theme = theme;
            }
            catch (Exception e)
            {
                App.WindowManager.ShowErrorString(e.Message);
            }
            OnPropertyChanged(nameof(Theme));
        }

        private Theme? RetrieveThemeFromFile()
        {
            try
            {
                string json = App.JsonIoManager.RetrieveJSONFromFile(THEME_FILE_LOCATION);
                if (!string.IsNullOrEmpty(json))
                {
                    return App.JsonIoManager.ConvertObjectsFromJSON<Theme>(json);
                }
            }
            catch (Exception e)
            {
                App.WindowManager.ShowErrorString(e.Message);
            }
            return null;
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
