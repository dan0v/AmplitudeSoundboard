/*
    AmplitudeSoundboard
    Copyright (C) 2021-2026 dan0v
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
using Avalonia.Controls;
using Splat;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Amplitude.Views
{
    public partial class WaylandHotkeysPrompt : Window, INotifyPropertyChanged
    {
        public const string OS_CONSTRAINTS_URL = "https://sharphook.tolik.io/articles/os-constraints.html";

        public static ThemeManager ThemeManager => Locator.Current.GetService<ThemeManager>()!;
        public bool CanUseCustomTitlebar => FeatureManager.IsFeatureEnabled(FeatureManager.Feature.CUSTOM_TITLEBAR);

        private bool _degradedModeEnabled = false;
        public bool DegradedModeEnabled
        {
            get => _degradedModeEnabled;
            set
            {
                if (value != _degradedModeEnabled)
                {
                    _degradedModeEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public WaylandHotkeysPrompt()
        {
            InitializeComponent();
        }

        public void OpenDocumentation()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = OS_CONSTRAINTS_URL,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Locator.Current.GetService<WindowManager>()!.ShowErrorString(ex.Message);
            }
        }

        public void EnableDegradedMode()
        {
            var configManager = Locator.Current.GetService<ConfigManager>()!;
            configManager.Config.DegradedX11InputMode = true;
            configManager.SaveAndOverwriteConfig(configManager.Config);
            DegradedModeEnabled = true;
        }

        public void Dismiss()
        {
            Close();
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
