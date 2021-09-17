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

using Amplitude.Helpers;
using Amplitude.Models;
using AmplitudeSoundboard;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.ComponentModel;

namespace Amplitude.Views
{
    public partial class ErrorList : Window
    {
        private StackPanel sp_Errors;
        private Button btn_Dismiss;

        public static ThemeHandler ThemeHandler { get => App.ThemeHandler; }

        public ErrorList()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.btn_Dismiss = this.FindControl<Button>("btn_Dismiss");
            this.sp_Errors = this.FindControl<StackPanel>("sp_Errors");

            this.btn_Dismiss.Click += Dismiss;

        }

        public void AddErrorString(string error)
        {
            TextBlock txt_error = new TextBlock();
            txt_error.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
            txt_error.Text = error;
            txt_error.Margin = Thickness.Parse("5,5,5,5");
            Border border = new Border();
            border.BorderThickness = Thickness.Parse("1");
            border.BorderBrush = Brush.Parse("Gray");
            border.CornerRadius = CornerRadius.Parse("5");
            border.Child = txt_error;

            this.sp_Errors.Children.Add(border);

            if (!this.IsVisible)
            {
                this.Show();
            }
        }

        // TODO update to display sound clip properties
        public void AddErrorSoundClip(string id, ErrorType errorType)
        {
            TextBlock txt_error = new TextBlock();
            txt_error.TextWrapping = Avalonia.Media.TextWrapping.Wrap;

            //SoundClip clip = find by ID from dictionary

            switch (errorType)
            {
                case ErrorType.BAD_FORMAT:
                    //txt_error.Text = string.Format(Localization.Localizer.Instance["FileBadFormatString"], clip.FilePath);
                    break;
                case ErrorType.MISSING_FILE:
                    //txt_error.Text = string.Format(Localization.Localizer.Instance["FileMissingString"], clip.FilePath);
                    break;
            }

            Border border = new Border();
            border.BorderThickness = Thickness.Parse("1");
            border.Child = txt_error;

            this.sp_Errors.Children.Add(border);

            if (!this.IsVisible)
            {
                this.Show();
            }
        }

        private void Dismiss(object? sender, RoutedEventArgs args)
        {
            this.sp_Errors.Children.Clear();
            this.Hide();
        }

        Window GetWindow() => (Window)this.VisualRoot;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public enum ErrorType
        {
            MISSING_FILE,
            BAD_FORMAT,
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.sp_Errors.Children.Clear();
            this.Hide();
            base.OnClosing(e);
        }
    }
}
