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

using Amplitude.Helpers;
using Amplitude.Models;
using Amplitude.ViewModels;
using Amplitude.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static System.Environment;

namespace AmplitudeSoundboard
{
    public class App : Application, INotifyPropertyChanged
    {
        public static string APP_STORAGE
        {
            get
            {
                string path = Path.Join(GetFolderPath(SpecialFolder.LocalApplicationData, SpecialFolderOption.DoNotVerify), "amplitude-soundboard");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public static SoundClipManager SoundClipManager => SoundClipManager.Instance;
        public static OutputProfileManager OutputProfileManager => OutputProfileManager.Instance;
        public static HotkeysManager HotkeysManager => HotkeysManager.Instance;
        public static ThemeHandler ThemeHandler => ThemeHandler.Instance;
        public static WindowManager WindowManager => WindowManager.Instance;
        public static OptionsManager OptionsManager => OptionsManager.Instance;
        public static ISoundEngine SoundEngine => MSoundEngine.Instance;

#if Windows
        public static IKeyboardHook KeyboardHook => WinKeyboardHook.Instance;
#else
        public static IKeyboardHook KeyboardHook => BlankHotkeysManager.Instance;
#endif

        public static string VERSION
        {
            get
            {
                string version = "";
                try
                {
                    var ver = Assembly.GetEntryAssembly()?.GetName().Version;
                    if (ver == null)
                    {
                        return "0.0.0";
                    }
                    version = $"{ver.Major}.{ver.Minor}.{ver.Build}";
                }
                catch (Exception e) { Debug.WriteLine(e); }
                return version;
            }
        }

        public const string VERSION_CHECK_URL = "https://github.com/dan0v/AmplitudeSoundboard/releases/latest/download/version.txt";
        public const string DOWNLOAD_WIN_URL = "https://github.com/dan0v/AmplitudeSoundboard/releases/latest/download/Amplitude_Soundboard_win_x86_64.zip";
        public const string DOWNLOAD_MACOS_URL = "https://github.com/dan0v/AmplitudeSoundboard/releases/latest/download/Amplitude_Soundboard_macOS_x86_64.tar.gz";
        public const string DOWNLOAD_LINUX_URL = "https://github.com/dan0v/AmplitudeSoundboard/releases/latest/download/Amplitude_Soundboard_linux_AppImage_x86_64.tar.gz";
        public const string RELEASES_PAGE = "https://github.com/dan0v/AmplitudeSoundboard/releases/latest/";

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
                var e = SoundEngine;
                var k = KeyboardHook;
                var o = OptionsManager;
                var p = OutputProfileManager;
                var s = SoundClipManager;
                var h = HotkeysManager;
                var t = ThemeHandler;
                var w = WindowManager;
                w.ReadWindowSizesAndPositions();
                w.SetMainWindow((MainWindow)desktop.MainWindow);

                // Trigger UI redraw
                OptionsManager.OnPropertyChanged(nameof(OptionsManager.Options));
#if !DEBUG
#if Windows
                Task.Run(CleanupOldFiles);
#endif
                CheckForUpdates();
#endif
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async Task CleanupOldFiles()
        {
            try
            {
                await Task.Delay(5000);
                string? currentFileName = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(currentFileName))
                {
                    return;
                }
                string? currentDirectory = Path.GetDirectoryName(currentFileName);
                if (string.IsNullOrEmpty(currentDirectory))
                {
                    return;
                }
                string oldFilePath = Path.Join(currentDirectory, "amplitude_soundboard.OLD.exe");

                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
