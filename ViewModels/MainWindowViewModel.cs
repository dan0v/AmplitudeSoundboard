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
using Amplitude.Views;
using AmplitudeSoundboard;
using Avalonia.Controls;

namespace Amplitude.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public static ThemeHandler ThemeHandler { get => App.ThemeHandler; }
        public void AddSound()
        {
            Window sound = new EditSoundClip
            {
                DataContext = new EditSoundClipViewModel(),
            };

            sound.Show();
        }

        public void ShowList()
        {
            Window list = new SoundClipList
            {
                DataContext = new SoundClipListViewModel(),
            };

            list.Show();
        }

        public void StopAudio()
        {
            App.SoundEngine.Reset(true);
        }

    }
}
