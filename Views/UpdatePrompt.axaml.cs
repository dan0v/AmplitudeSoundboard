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

using Amplitude.Localization;
using Amplitude.Models;
using AmplitudeSoundboard;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Amplitude.Views
{
    public partial class UpdatePrompt : Window, INotifyPropertyChanged
    {
        public static ThemeHandler ThemeHandler { get => App.ThemeHandler; }

        private string newVersion = "";

        private bool _updating = false;
        public bool Updating
        {
            get => _updating;
            set
            {
                if (value != _updating)
                {
                    _updating = value;
                    OnPropertyChanged();
                }
            }
        }

#if Windows
        public string UpdatePromptText => string.Format(Localizer.Instance["NewVersionCanBeInstalled"], newVersion);
#else
        public string UpdatePromptText => string.Format(Localizer.Instance["NewVersionCanBeDownloaded"], newVersion);
#endif

        public UpdatePrompt()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public UpdatePrompt(string newVersion)
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.newVersion = newVersion;
            OnPropertyChanged(nameof(UpdatePromptText));
        }

        private void Update()
        {
            new Thread(() =>
            {
                RunUpdate();
            })
            {
                IsBackground = true
            }.Start();
        }

        private void RunUpdate()
        {
            try
            {
#if Windows
                Updating = true;
                string? currentFileName = Process.GetCurrentProcess().MainModule?.FileName;
                string? currentDirectory = Path.GetDirectoryName(currentFileName);

                if (!string.IsNullOrEmpty(currentDirectory))
                {
                    string oldFilePath = Path.Join(currentDirectory, "amplitude_soundboard.OLD.exe");

                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }

                    File.Move(currentFileName, oldFilePath);

                    string zipPath = Path.Join(currentDirectory, "AmplitudeUpdate.zip");
                    string outputPath = Path.Join(currentDirectory, "AmplitudeUpdate");

                    using (WebClient myWebClient = new WebClient())
                    {
                        myWebClient.DownloadFile(App.DOWNLOAD_WIN_URL, zipPath);
                    }

                    ZipFile.ExtractToDirectory(zipPath, outputPath);

                    // Overwrite current app
                    File.Move(Path.Join(outputPath, "Amplitude Soundboard", "amplitude_soundboard.exe"), currentFileName);

                    // Delete update files
                    File.Delete(zipPath);
                    Directory.Delete(outputPath, true);

                    Updating = false;

                    // Start new version and quit current
                    Process.Start(currentFileName);
                    using (var lifetime = (ClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime)
                    {
                        lifetime.Shutdown(0);
                    }
                }
#else
                ProcessStartInfo url = new ProcessStartInfo
                {
                    FileName = App.RELEASES_PAGE,
                    UseShellExecute = true
                };
                Process.Start(url);
#endif
            }
            catch (Exception ex)
            {
                Updating = false;
                Debug.WriteLine(ex);
                App.WindowManager.ErrorList.AddErrorString(ex.Message);
            }
        }

        private void Dismiss()
        {
            this.Close();
        }

        Window GetWindow() => (Window)this.VisualRoot;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Updating)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
