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

using Amplitude.ViewModels;
using AmplitudeSoundboard;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;

namespace Amplitude.Views
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        ScrollViewer scrl_GridScroll;

        private Timer timer = new Timer(1500);

        private (double width, double height) _gridSize;
        public (double width, double height) GridSize
        {
            get => _gridSize;
            set
            {
                if (value != _gridSize)
                {
                    _gridSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            scrl_GridScroll = this.Find<ScrollViewer>(nameof(scrl_GridScroll));

            PositionChanged += MainWindow_PositionChanged;
            EffectiveViewportChanged += MainWindow_EffectiveViewportChanged;
            timer.AutoReset = false;
            timer.Elapsed += Timer_Elapsed;
        }

        private void MainWindow_EffectiveViewportChanged(object? sender, Avalonia.Layout.EffectiveViewportChangedEventArgs e)
        {
            GridSize = (scrl_GridScroll.Bounds.Width, scrl_GridScroll.Bounds.Height);

            if (App.OptionsManager.Options.AutoScaleTilesToWindow)
            {
                if (!timer.Enabled)
                {
                    timer.Enabled = true;
                }
                else
                {
                    timer.Interval = 1500;
                }
            }
        }

        /// <summary>
        /// Trigger image rescaling a period of time after the user stops resizing the main window
        /// </summary>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            App.SoundClipManager.RescaleAllBackgroundImages(true);
        }

        private void MainWindow_PositionChanged(object? sender, PixelPointEventArgs e)
        {
            ((MainWindowViewModel)DataContext).WindowPosition = (e.Point.X, e.Point.Y);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            App.SoundEngine.Dispose();
            App.KeyboardHook.Dispose();
            ((MainWindowViewModel)DataContext).Dispose();
            base.OnClosing(e);
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
