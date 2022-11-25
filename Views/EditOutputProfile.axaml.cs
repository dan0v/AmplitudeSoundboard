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
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.ComponentModel;

namespace Amplitude.Views
{
    public partial class EditOutputProfile : Window
    {
        Button btn_removeOutputProfile;

        public EditOutputProfile()
        {
            InitializeComponent();

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            btn_removeOutputProfile = this.Find<Button>("btn_removeOutputProfile");
            btn_removeOutputProfile.Click += Btn_removeOutputProfile_Click;
        }

        private void Btn_removeOutputProfile_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            App.WindowManager.ClosedEditOutputProfileWindow(((EditOutputProfileViewModel)this.DataContext).Model.Id);
            ((EditOutputProfileViewModel)DataContext).Dispose();
            base.OnClosing(e);
        }
    }
}
