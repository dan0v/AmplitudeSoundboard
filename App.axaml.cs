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
using Amplitude.ViewModels;
using Amplitude.Views;
using Amplitude.Models;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.IO;
using System;
using static System.Environment;
using Avalonia.Themes.Fluent;

namespace AmplitudeSoundboard
{
    public class App : Application
    {

        public static string APP_STORAGE
        {
            get
            {
                string path = Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData, SpecialFolderOption.DoNotVerify), "amplitude-soundboard");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                    
                return path;
			}
		}
		
#if Windows
        public static ISoundEngine SoundEngine => NSoundEngine.Instance;
        public static SoundClipManager SoundClipManager => SoundClipManager.Instance;
		
        public static WinKeyboardHook KeyboardHook => WinKeyboardHook.Instance;
#else
        public static ISoundEngine SoundEngine => TempSoundEngine.Instance;
        //public static WinKeyboardHook KeyboardHook => WinKeyboardHook.Instance;
#endif
        public static HotkeysManager HotkeysManager => HotkeysManager.Instance;

        public static ThemeHandler ThemeHandler => ThemeHandler.Instance;

        public static Options Options = new Options();

        private static ErrorList _errorListWindow;
        public static ErrorList ErrorListWindow {
            get
            {
                if (_errorListWindow == null)
                {
                    _errorListWindow = new ErrorList();
                }
                return _errorListWindow;
            }
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            SoundClipManager.AddClip();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
