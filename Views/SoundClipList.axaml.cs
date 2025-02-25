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
    public partial class SoundClipList : Window
    {
        public const string WindowId = "soundClipList";

        public SoundClipList()
        {
            InitializeComponent();
            App.WindowManager.SoundClipListWindow = this;

            PositionChanged += SoundClipList_PositionChanged;
            EffectiveViewportChanged += SoundClipList_EffectiveViewportChanged;
        }

        private void SoundClipList_EffectiveViewportChanged(object? sender, Avalonia.Layout.EffectiveViewportChangedEventArgs e)
        {
            App.WindowManager.WindowSizesOrPositionsChanged();
        }

        private void SoundClipList_PositionChanged(object? sender, PixelPointEventArgs e)
        {
            App.WindowManager.WindowSizesOrPositionsChanged();
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            App.WindowManager.SoundClipListWindow = null;
            PositionChanged -= SoundClipList_PositionChanged;
            EffectiveViewportChanged -= SoundClipList_EffectiveViewportChanged;
            ((SoundClipListViewModel?)DataContext)?.Dispose();
            base.OnClosing(e);
        }
    }
}
