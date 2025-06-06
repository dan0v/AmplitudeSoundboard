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

using Amplitude.ViewModels;
using AmplitudeSoundboard;
using Avalonia.Controls;

namespace Amplitude.Views
{
    public partial class GlobalSettings : Window
    {
        public const string WindowId = "globalSettings";

        public GlobalSettings()
        {
            InitializeComponent();
            EffectiveViewportChanged += GlobalSettings_EffectiveViewportChanged;
        }

        private void GlobalSettings_EffectiveViewportChanged(object? sender, Avalonia.Layout.EffectiveViewportChangedEventArgs e)
        {
            App.WindowManager.WindowSizesOrPositionsChanged();
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            App.WindowManager.GlobalSettingsWindow = null;
            EffectiveViewportChanged -= GlobalSettings_EffectiveViewportChanged;
            ((GlobalSettingsViewModel?)DataContext)?.Dispose();
            base.OnClosing(e);
        }
    }
}
