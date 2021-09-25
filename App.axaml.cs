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
using static System.Environment;
using System.Reflection;
using System;
using System.Diagnostics;
using System.Net.Http;

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
		
        public static SoundClipManager SoundClipManager => SoundClipManager.Instance;
        
        public static HotkeysManager HotkeysManager => HotkeysManager.Instance;

        public static ThemeHandler ThemeHandler => ThemeHandler.Instance;

        public static WindowManager WindowManager => WindowManager.Instance;

        public static OptionsManager OptionsManager => OptionsManager.Instance;

        public static string VERSION
        {
            get
            {
                string version = "";
                try
                {
                    version = System.Reflection.Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                }
                catch (Exception e) { Debug.WriteLine(e); }
                return version;
            }
        }

        public const string VERSION_CHECK_URL = "https://github.com/dan0v/AmplitudeSoundboard/releases/latest/download/version.txt";
        public const string RELEASES_PAGE = "https://github.com/dan0v/AmplitudeSoundboard/releases/latest/";

#if Windows
        public static ISoundEngine SoundEngine => NSoundEngine.Instance;
		
        public static IKeyboardHook KeyboardHook => WinKeyboardHook.Instance;
#else
        public static ISoundEngine SoundEngine => TempSoundEngine.Instance;
        //public static IKeyboardHook KeyboardHook => WinKeyboardHook.Instance;
#endif

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };

                // Initialize managers to make sure they are active
                var se = SoundEngine;
                var k = KeyboardHook;
                var o = OptionsManager;
                var s = SoundClipManager;
                var h = HotkeysManager;
                var t = ThemeHandler;
                var w = WindowManager;

                CheckForUpdates();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async void CheckForUpdates()
        {
            try
            {
                HttpResponseMessage response = await new HttpClient().GetAsync(VERSION_CHECK_URL);
                response.EnsureSuccessStatusCode();
                string newVer = await response.Content.ReadAsStringAsync();
                newVer = newVer.Trim();
                if (!string.IsNullOrEmpty(newVer) && newVer != VERSION.Trim())
                {
                    UpdatePrompt updateDialog = new UpdatePrompt(newVer);
                    updateDialog.Show();
                    updateDialog.Activate();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}
