/*
    AmplitudeSoundboard
    Copyright (C) 2021-2024 dan0v
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

using Amplitude.ViewModels;
using AmplitudeSoundboard;
using Avalonia.Controls;

namespace Amplitude.Views
{
    public partial class ThemeSettings : Window
    {
        public const string WindowId = "themeSettings";

        public ThemeSettings()
        {
            InitializeComponent();
            EffectiveViewportChanged += ThemeSettings_EffectiveViewportChanged;
        }

        private void ThemeSettings_EffectiveViewportChanged(object? sender, Avalonia.Layout.EffectiveViewportChangedEventArgs e)
        {
            App.WindowManager.WindowSizesOrPositionsChanged();
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            App.WindowManager.ThemeSettingsWindow = null;
            EffectiveViewportChanged -= ThemeSettings_EffectiveViewportChanged;
            ((ThemeSettingsViewModel)DataContext).Dispose();
            base.OnClosing(e);
        }
    }
}