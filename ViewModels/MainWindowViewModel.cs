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
using System.Collections.ObjectModel;

namespace Amplitude.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private static ThemeHandler ThemeHandler { get => App.ThemeHandler; }
        private static SoundClipManager Manager { get => App.SoundClipManager; }
        private static OptionsManager OptionsManager { get => App.OptionsManager; }

        public (int x, int y) WindowPosition = (0, 0);

        private bool GlobalSettingsWindowOpen { get => App.WindowManager.GlobalSettingsWindow != null; }

        private string StopAudioHotkey => string.IsNullOrEmpty(OptionsManager.Options.GlobalKillAudioHotkey) ? Localization.Localizer.Instance["StopAllAudio"] : Localization.Localizer.Instance["StopAllAudio"] + ": " + OptionsManager.Options.GlobalKillAudioHotkey;

        public MainWindowViewModel()
        {
            OptionsManager.PropertyChanged += OptionsManager_PropertyChanged;

            GridItemsRows.Clear();
            foreach (GridItemRow temp in App.OptionsManager.GetGridLayout())
            {
                GridItemsRows.Add(temp);
            }
            OnPropertyChanged(nameof(GridItemsRows));
        }

        private void OptionsManager_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OptionsManager.Options))
            {
                OnPropertyChanged(nameof(StopAudioHotkey));

                GridItemsRows.Clear();
                foreach (GridItemRow temp in App.OptionsManager.GetGridLayout())
                {
                    GridItemsRows.Add(temp);
                }
                OnPropertyChanged(nameof(GridItemsRows));
            }
        }

        private ObservableCollection<GridItemRow> _gridItemsRows  = new();
        private ObservableCollection<GridItemRow> GridItemsRows { get => _gridItemsRows; }

        public void ShowList()
        {
            App.WindowManager.ShowSoundClipListWindow(new Avalonia.PixelPoint(WindowPosition.x + 200, WindowPosition.y + 200));
        }

        public void ShowGlobalSettings()
        {
            App.WindowManager.ShowGlobalSettingsWindow(new Avalonia.PixelPoint(WindowPosition.x + 150, WindowPosition.y + 150));
        }
        
        public void ShowAbout()
        {
            App.WindowManager.ShowAboutWindow(new Avalonia.PixelPoint(WindowPosition.x + 100, WindowPosition.y + 100));
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
