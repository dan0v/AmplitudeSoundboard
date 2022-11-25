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

using Amplitude.ViewModels;
using AmplitudeSoundboard;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.ComponentModel;

namespace Amplitude.Views
{
    public partial class EditOutputProfile : Window
    {
        Button btn_RemoveOutputProfile;
        TextBlock txt_blk_OutputProfileId;

        public EditOutputProfile()
        {
            InitializeComponent();

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            txt_blk_OutputProfileId = this.FindControl<TextBlock>("txt_blk_OutputProfileId");
            txt_blk_OutputProfileId.PropertyChanged += Txt_blk_OutputProfileId_PropertyChanged;

            btn_RemoveOutputProfile = this.Find<Button>("btn_RemoveOutputProfile");
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
                if (!string.IsNullOrEmpty((string)e.NewValue) && e.NewValue != e.OldValue)
                {
                    App.WindowManager.OpenedEditOutputProfileWindow(((EditOutputProfileViewModel)this.DataContext).Model.Id, this);
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            App.WindowManager.ClosedEditOutputProfileWindow(((EditOutputProfileViewModel)this.DataContext).Model.Id);
            ((EditOutputProfileViewModel)DataContext).Dispose();
            base.OnClosing(e);
        }
    }
}
