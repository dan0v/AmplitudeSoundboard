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

using Amplitude.Helpers;
using Amplitude.ViewModels;
using AmplitudeSoundboard;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;

namespace Amplitude.Views
{
    public partial class EditSoundClip : Window
    {
        public const string WindowId = "editSoundClip";
        public long lastTouchedTime = 0;

        public EditSoundClip()
        {
            InitializeComponent();
            txt_blk_SoundClipId.PropertyChanged += Txt_blk_SoundClipId_PropertyChanged;
            btn_BrowseAudioFilePath.Click += BrowseSoundClip;
            btn_BrowseImageFilePath.Click += BrowseImage;
            btn_Delete.Click += DeleteSoundClip;
            cb_OutputProfileSelection.SelectionChanged += Cb_OutputProfileSelectionChanged;
            EffectiveViewportChanged += EditSoundClip_EffectiveViewportChanged;

        }

        private void Cb_OutputProfileSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            ((EditSoundClipViewModel?)DataContext)?.OutputProfileSelectionChanged(sender, e);
        }

        private void EditSoundClip_EffectiveViewportChanged(object? sender, Avalonia.Layout.EffectiveViewportChangedEventArgs e)
        {
            lastTouchedTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            App.WindowManager.WindowSizesOrPositionsChanged();
        }

        private void Txt_blk_SoundClipId_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == TextBlock.TextProperty)
            {
                if (!string.IsNullOrEmpty((string?)e.NewValue) && e.NewValue != e.OldValue)
                {
                    App.WindowManager.OpenedEditSoundClipWindow(((EditSoundClipViewModel?)this.DataContext)?.Model.Id, this);
                }
            }
        }

        protected async void BrowseSoundClip(object? sender, RoutedEventArgs args)
        {
            try
            {
                string[]? url = await BrowseIO.OpenFileBrowser(GetWindow(), BrowseIO.FileBrowserType.AUDIO);
                ((EditSoundClipViewModel?)DataContext)?.SetClipAudioFilePath(url);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        protected async void BrowseImage(object? sender, RoutedEventArgs args)
        {
            try
            {
                string[]? url = await BrowseIO.OpenFileBrowser(GetWindow(), BrowseIO.FileBrowserType.IMAGE);
                ((EditSoundClipViewModel?)DataContext)?.SetClipImageFilePath(url);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        protected void DeleteSoundClip(object? sender, RoutedEventArgs args)
        {
            ((EditSoundClipViewModel?)this.DataContext)?.DeleteSoundClip();
            this.Close();
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            EffectiveViewportChanged -= EditSoundClip_EffectiveViewportChanged;
            App.WindowManager.ClosedEditSoundClipWindow(((EditSoundClipViewModel?)this.DataContext)?.Model.Id);
            ((EditSoundClipViewModel?)DataContext)?.Dispose();
            base.OnClosing(e);
        }

        Window GetWindow() => (Window?)this.VisualRoot ?? throw new NullReferenceException("Window should exist");
    }
}
