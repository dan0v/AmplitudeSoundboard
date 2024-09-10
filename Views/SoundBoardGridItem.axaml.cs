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

using Amplitude.Models;
using Amplitude.ViewModels;
using Avalonia.Controls;

namespace Amplitude.Views
{
    public partial class SoundBoardGridItem : UserControl
    {
        public SoundBoardGridItem()
        {
            InitializeComponent();
            this.grd_Control.PointerPressed += Control_PointerPressed;
        }

        private void Control_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (!e.Handled && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                SoundClip? Model = ((SoundBoardGridItemViewModel?)DataContext)?.Model;
                if (Model != null && !string.IsNullOrEmpty(Model.Id))
                {
                    Model.PlayAudio();
                }
            }
            else if (!e.Handled && e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed)
            {
                SoundClip? Model = ((SoundBoardGridItemViewModel?)DataContext)?.Model;
                if (Model != null && !string.IsNullOrEmpty(Model.Id))
                {
                    Model.AddAudioToQueue();
                }
            }
        }
    }
}
