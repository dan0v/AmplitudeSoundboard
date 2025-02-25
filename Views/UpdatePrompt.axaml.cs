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

using Amplitude.Helpers;
using Amplitude.Localization;
using AmplitudeSoundboard;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Amplitude.Views
{
    public partial class UpdatePrompt : Window, INotifyPropertyChanged
    {
        public static ThemeManager ThemeManager => App.ThemeManager;
        public bool CanUseCustomTitlebar => FeatureManager.IsFeatureEnabled(FeatureManager.Feature.CUSTOM_TITLEBAR);

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
            Localizer.Instance.PropertyChanged += Localizer_PropertyChanged;
        }

        public UpdatePrompt(string newVersion)
        {
            InitializeComponent();
            this.newVersion = newVersion;
            OnPropertyChanged(nameof(UpdatePromptText));
            Localizer.Instance.PropertyChanged += Localizer_PropertyChanged;
        }

        private void Localizer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(UpdatePromptText));
        }

        protected async void Update()
        {
            await Task.Run(RunUpdateAsync);
        }

        private async void RunUpdateAsync()
        {
            try
            {
#if Windows
                Updating = true;
                string? currentFileName = Process.GetCurrentProcess().MainModule?.FileName;
                string? currentDirectory = Path.GetDirectoryName(currentFileName);

                if (!string.IsNullOrEmpty(currentDirectory) && !string.IsNullOrEmpty(currentFileName))
                {
                    string oldFilePath = Path.Join(currentDirectory, "amplitude_soundboard.OLD.exe");
                    string zipPath = Path.Join(currentDirectory, "AmplitudeUpdate.zip");
                    string outputPath = Path.Join(currentDirectory, "AmplitudeUpdate");

                    using (HttpClient httpClient = new HttpClient())
                    {
                        using HttpResponseMessage response = await httpClient.GetAsync(App.DOWNLOAD_WIN_URL, HttpCompletionOption.ResponseHeadersRead);
                        using Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();
                        using Stream streamToWriteTo = File.Open(zipPath, FileMode.Create);
                        await streamToReadFrom.CopyToAsync(streamToWriteTo);
                    }

                    ZipFile.ExtractToDirectory(zipPath, outputPath, true);

                    // Mark current application as outdated
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                    File.Move(currentFileName, oldFilePath);

                    // Overwrite current app
                    File.Move(Path.Join(outputPath, "Amplitude Soundboard", "amplitude_soundboard.exe"), currentFileName);

                    // Delete update files
                    File.Delete(zipPath);
                    Directory.Delete(outputPath, true);

                    Updating = false;

                    // Start new version and quit current
                    Process.Start(currentFileName);

                    ((ClassicDesktopStyleApplicationLifetime?)Application.Current?.ApplicationLifetime)?.Shutdown(0);
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
                App.WindowManager.ShowErrorString(ex.Message);
            }
        }

        protected void Dismiss()
        {
            this.Close();
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            if (Updating)
            {
                e.Cancel = true;
            }
            else
            {
                Localizer.Instance.PropertyChanged -= Localizer_PropertyChanged;
            }
            base.OnClosing(e);
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
