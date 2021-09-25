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
            border.BorderThickness = Thickness.Parse("2,1,2,1");
            // TODO this doesn't currently update when theme is switched during runtime
            border.BorderBrush = new SolidColorBrush(ThemeHandler.BorderColor);
            border.CornerRadius = CornerRadius.Parse("5");
            border.Child = txt_error;

            this.sp_Errors.Children.Add(border);

            if (!this.IsVisible)
            {
                this.Show();
            }
        }

        // TODO update to display sound clip properties
        public void AddErrorSoundClip(SoundClip clip, ErrorType errorType)
        {
            TextBlock txt_error = new TextBlock();
            txt_error.TextWrapping = Avalonia.Media.TextWrapping.Wrap;

            switch (errorType)
            {
                case ErrorType.BAD_IMAGE_FORMAT:
                    txt_error.Text = string.Format(Localization.Localizer.Instance["SoundClipError"], clip.Name, string.Format(Localization.Localizer.Instance["FileBadFormatString"], clip.ImageFilePath));
                    break;
                case ErrorType.BAD_AUDIO_FORMAT:
                    txt_error.Text = string.Format(Localization.Localizer.Instance["SoundClipError"], clip.Name, string.Format(Localization.Localizer.Instance["FileBadFormatString"], clip.AudioFilePath));
                    break;
                case ErrorType.MISSING_IMAGE_FILE:
                    txt_error.Text = string.Format(Localization.Localizer.Instance["SoundClipError"], clip.Name, string.Format(Localization.Localizer.Instance["FileMissingString"], clip.ImageFilePath));
                    break;
                case ErrorType.MISSING_AUDIO_FILE:
                    txt_error.Text = string.Format(Localization.Localizer.Instance["SoundClipError"], clip.Name, string.Format(Localization.Localizer.Instance["FileMissingString"], clip.AudioFilePath));
                    break;
                case ErrorType.MISSING_DEVICE:
                    txt_error.Text = string.Format(Localization.Localizer.Instance["SoundClipError"], clip.Name, string.Format(Localization.Localizer.Instance["MissingDeviceString"], clip.DeviceName));
                    break;
            }

            string[] lines = txt_error.Text.Split('\n');

            if (lines.Length > 1)
            {
                txt_error.Text = "";
                for (int i = 0; i < lines.Length; i++)
                {
                    txt_error.Text += (i < lines.Length - 1) ? lines[i] + Environment.NewLine : lines[i];
                }
            }

            txt_error.Margin = Thickness.Parse("5,5,5,5");
            Border border = new Border();
            border.BorderThickness = Thickness.Parse("2,1,2,1");
            // TODO this doesn't currently update when theme is switched during runtime
            border.BorderBrush = new SolidColorBrush(ThemeHandler.BorderColor);
            border.CornerRadius = CornerRadius.Parse("5");
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
            MISSING_IMAGE_FILE,
            MISSING_AUDIO_FILE,
            BAD_AUDIO_FORMAT,
            BAD_IMAGE_FORMAT,
            MISSING_DEVICE,
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
