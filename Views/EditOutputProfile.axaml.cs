/*
    AmplitudeSoundboard
    Copyright (C) 2021-2026 dan0v
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
using Avalonia;
using Avalonia.Controls;
using Splat;

namespace Amplitude.Views
{
    public partial class EditOutputProfile : Window
    {
        public const string WindowId = "editOutputProfile";

        private string? modelId => ((EditOutputProfileViewModel?)this.DataContext)?.Model.Id;

        public EditOutputProfile()
        {
            InitializeComponent();
            txt_blk_OutputProfileId.PropertyChanged += Txt_blk_OutputProfileId_PropertyChanged;
            btn_RemoveOutputProfile.Click += Btn_RemoveOutputProfile_Click;
        }

        private void Btn_RemoveOutputProfile_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }

        private void Txt_blk_OutputProfileId_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == TextBlock.TextProperty)
            {
                if (!string.IsNullOrEmpty((string?)e.NewValue) && e.NewValue != e.OldValue && modelId != null)
                {
                    Locator.Current.GetService<WindowManager>()!.OpenedEditOutputProfileWindow(modelId, this);
                }
            }
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            if (modelId != null)
            {
                Locator.Current.GetService<WindowManager>()!.ClosedEditOutputProfileWindow(modelId);
            }
            ((EditOutputProfileViewModel?)DataContext)?.Dispose();
            base.OnClosing(e);
        }
    }
}
