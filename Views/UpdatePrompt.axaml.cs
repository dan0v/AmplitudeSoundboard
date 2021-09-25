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
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Amplitude.Views
{
    public partial class UpdatePrompt : Window
    {
        public static ThemeHandler ThemeHandler { get => App.ThemeHandler; }
        private TextBlock txt_blk_Prompt;

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
            this.txt_blk_Prompt = this.FindControl<TextBlock>("txt_blk_Prompt");
            txt_blk_Prompt.Text = string.Format(Localizer.Instance["NewVersionCanBeDownloaded"], newVersion);
        }

        private void GoToReleases()
        {
            try
            {
                ProcessStartInfo url = new ProcessStartInfo
                {
                    FileName = App.RELEASES_PAGE,
                    UseShellExecute = true
                };
                Process.Start(url);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
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
            base.OnClosing(e);
        }
    }
}
