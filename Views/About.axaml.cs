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
using Amplitude.Models;
using AmplitudeSoundboard;
using Avalonia;
using Avalonia.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Amplitude.Views
{
    public partial class About : Window
    {
        private string projectUrl = "https://amplitude-soundboard.dan0v.com";

        protected ThemeManager ThemeHandler => App.ThemeManager;
        public static bool CanUseCustomTitlebar => FeatureManager.IsFeatureEnabled(FeatureManager.Feature.CUSTOM_TITLEBAR);

        public About()
        {
            InitializeComponent();
            this.txt_bx_License.Text = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Doc", "LICENSE.txt"));
            this.txt_bx_Notice.Text = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Doc", "NOTICE.txt"));
            this.txt_blk_Copyright.Text = @"AmplitudeSoundboard
Copyright (C) 2021-2024 dan0v";
            this.txt_blk_Version.Text = "Version " + App.VERSION;
            this.txt_blk_URL.Text = projectUrl;
            this.txt_blk_URL.PointerPressed += Txt_blk_URL_PointerPressed;
        }

        private void Txt_blk_URL_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            try
            {
                ProcessStartInfo url = new ProcessStartInfo
                {
                    FileName = projectUrl,
                    UseShellExecute = true
                };
                Process.Start(url);
            }
            catch (Exception ex)
            {
                App.WindowManager.ShowErrorString(ex.Message);
            }
        }

        protected void Dismiss()
        {
            this.Close();
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            App.WindowManager.AboutWindow = null;
            this.txt_blk_URL.PointerPressed -= Txt_blk_URL_PointerPressed;
            base.OnClosing(e);
        }
    }
}
