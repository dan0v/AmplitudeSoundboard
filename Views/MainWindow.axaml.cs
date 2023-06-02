/*
    AmplitudeSoundboard
    Copyright (C) 2021-2023 dan0v
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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amplitude.Views
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
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

        public (double height, double width) WindowSize = (0, 0);
        public (int x, int y) WindowPosition = (0, 0);

        public MainWindow()
        {
            InitializeComponent();

            PositionChanged += MainWindow_PositionChanged;
            EffectiveViewportChanged += MainWindow_EffectiveViewportChanged;

            WindowSize = (Height, Width);
            WindowPosition = (Position.X, Position.Y);
        }

        private void MainWindow_EffectiveViewportChanged(object? sender, Avalonia.Layout.EffectiveViewportChangedEventArgs e)
        {
            GridSize = (scrl_GridScroll.Bounds.Width, scrl_GridScroll.Bounds.Height);
            WindowSize = (Height, Width);
            App.WindowManager.WindowSizesOrPositionsChanged();
        }

        private void MainWindow_PositionChanged(object? sender, PixelPointEventArgs e)
        {
            WindowPosition = (e.Point.X, e.Point.Y);
            ((MainWindowViewModel)DataContext).WindowPosition = WindowPosition;
            App.WindowManager.WindowSizesOrPositionsChanged();
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            PositionChanged -= MainWindow_PositionChanged;
            EffectiveViewportChanged -= MainWindow_EffectiveViewportChanged;
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
