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
using Amplitude.ViewModels;
using AmplitudeSoundboard;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Amplitude.Views
{
    public partial class EditSoundClip : Window
    {
        private TextBlock txt_blk_SoundClipId;
        private Button btn_BrowseAudioFilePath;
        private Button btn_BrowseImageFilePath;
        private Button btn_Delete;

        public EditSoundClip()
        {
            InitializeComponent();
            txt_blk_SoundClipId = this.FindControl<TextBlock>("txt_blk_SoundClipId");
            txt_blk_SoundClipId.PropertyChanged += Txt_blk_SoundClipId_PropertyChanged;

            btn_BrowseAudioFilePath = this.FindControl<Button>("btn_BrowseAudioFilePath");
            btn_BrowseAudioFilePath.Click += BrowseSoundClip;

            btn_BrowseImageFilePath = this.FindControl<Button>("btn_BrowseImageFilePath");
            btn_BrowseImageFilePath.Click += BrowseImage;

            btn_Delete = this.FindControl<Button>("btn_Delete");
            btn_Delete.Click += DeleteSoundClip;

            EffectiveViewportChanged += EditSoundClip_EffectiveViewportChanged;

        }

        private void EditSoundClip_EffectiveViewportChanged(object? sender, Avalonia.Layout.EffectiveViewportChangedEventArgs e)
        {
            App.WindowManager.WindowSizesOrPositionsChanged();
        }

        private void Txt_blk_SoundClipId_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == TextBlock.TextProperty)
            {
                if (!string.IsNullOrEmpty((string)e.NewValue) && e.NewValue != e.OldValue)
                {
                    App.WindowManager.OpenedEditSoundClipWindow(((EditSoundClipViewModel)this.DataContext).Model.Id, this);
                }
            }
        }

        public async void BrowseSoundClip(object? sender, RoutedEventArgs args)
        {
            try
            {
                string[]? url = await BrowseIO.OpenFileBrowser(GetWindow(), BrowseIO.FileBrowserType.AUDIO);
                ((EditSoundClipViewModel)DataContext).SetClipAudioFilePath(url);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public async void BrowseImage(object? sender, RoutedEventArgs args)
        {
            try
            {
                string[]? url = await BrowseIO.OpenFileBrowser(GetWindow(), BrowseIO.FileBrowserType.IMAGE);
                ((EditSoundClipViewModel)DataContext).SetClipImageFilePath(url);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public void DeleteSoundClip(object? sender, RoutedEventArgs args)
        {
            ((EditSoundClipViewModel)this.DataContext).DeleteSoundClip();
            this.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            EffectiveViewportChanged -= EditSoundClip_EffectiveViewportChanged;
            App.WindowManager.ClosedEditSoundClipWindow(((EditSoundClipViewModel)this.DataContext).Model.Id);
            ((EditSoundClipViewModel)DataContext).Dispose();
            base.OnClosing(e);
        }

        Window GetWindow() => (Window)this.VisualRoot;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
