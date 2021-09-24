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
using Amplitude.Views;
using AmplitudeSoundboard;

namespace Amplitude.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        static ThemeHandler ThemeHandler { get => App.ThemeHandler; }
        static SoundClipManager Manager { get => App.SoundClipManager; }

        public (int x, int y) WindowPosition = (0, 0);

        public bool GlobalSettingsWindowOpen { get => App.WindowManager.GlobalSettingsWindow != null; }

        private OptionsManager OptionsManager { get => App.OptionsManager; }

        private string StopAudioHotkey => OptionsManager.Options.GlobalKillAudioHotkey;

        public MainWindowViewModel()
        {
            OptionsManager.PropertyChanged += OptionsManager_PropertyChanged;
        }

        private void OptionsManager_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OptionsManager.Options))
            {
                OnPropertyChanged(nameof(StopAudioHotkey));
            }
        }

        public void ShowList()
        {
            SoundClipList? window = App.WindowManager.SoundClipListWindow;
            if (window != null)
            {
                window.Activate();
            }
            else
            {
                window = new SoundClipList
                {
                    DataContext = new SoundClipListViewModel(),
                    Position = new Avalonia.PixelPoint(WindowPosition.x + 200, WindowPosition.y + 200)
                };
                window.Show();
            }

        }

        public void ShowGlobalSettings()
        {
            GlobalSettings? window = App.WindowManager.GlobalSettingsWindow;
            if (window != null)
            {
                window.Activate();
            }
            else
            {
                window = new GlobalSettings
                {
                    MainWindow = this,
                    DataContext = new GlobalSettingsViewModel(),
                    Position = new Avalonia.PixelPoint(WindowPosition.x + 150, WindowPosition.y + 150)
                };
                window.Show();
            }

        }

        public void StopAudio()
        {
            App.SoundEngine.Reset(true);
        }

        public override void Dispose()
        {
            OptionsManager.PropertyChanged -= OptionsManager_PropertyChanged;
            base.Dispose();
        }
    }
}
