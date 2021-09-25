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
    public partial class About : Window
    {
        private Button btn_Dismiss;
        private TextBox txt_bx_License;
        private TextBox txt_bx_Notice;
        private TextBlock txt_blk_Copyright;
        private TextBlock txt_blk_Version;
        private TextBlock txt_blk_URL;
        private string projectUrl = "https://git.dan0v.com/AmplitudeSoundboard";

        public static ThemeHandler ThemeHandler { get => App.ThemeHandler; }

        public About()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.btn_Dismiss = this.FindControl<Button>("btn_Dismiss");
            this.btn_Dismiss.Click += Dismiss;

            this.txt_bx_License = this.FindControl<TextBox>("txt_bx_License");
            this.txt_bx_License.Text = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Doc", "LICENSE.txt"));

            this.txt_bx_Notice = this.FindControl<TextBox>("txt_bx_Notice");
            this.txt_bx_Notice.Text = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Doc", "NOTICE.txt"));

            this.txt_blk_Copyright = this.FindControl<TextBlock>("txt_blk_Copyright");
            this.txt_blk_Copyright.Text = @"AmplitudeSoundboard
Copyright � 2021 dan0v";

            this.txt_blk_Version = this.FindControl<TextBlock>("txt_blk_Version");
            this.txt_blk_Version.Text = "Version " + App.VERSION;

            this.txt_blk_URL = this.FindControl<TextBlock>("txt_blk_URL");
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
                App.WindowManager.ErrorListWindow.AddErrorString(ex.Message);
            }
        }

        private void Dismiss(object? sender, RoutedEventArgs args)
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
            App.WindowManager.AboutWindow = null;
            this.txt_blk_URL.PointerPressed -= Txt_blk_URL_PointerPressed;
            base.OnClosing(e);
        }
    }
}
