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
using Amplitude.Views;
using AmplitudeSoundboard;
using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amplitude.Helpers
{
    public class WindowManager : INotifyPropertyChanged
    {
        private static WindowManager _instance;
        public static WindowManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WindowManager();
                }
                return _instance;
            }
        }
        
        private Dictionary<string, EditSoundClip> _editSoundClipWindows = new Dictionary<string, EditSoundClip>();
        public Dictionary<string, EditSoundClip> EditSoundClipWindows
        {
            get => _editSoundClipWindows;
            set
            {
                if (value != _editSoundClipWindows)
                {
                    _editSoundClipWindows = value;
                    OnPropertyChanged();
                }
            }
        }

        public void OpenEditSoundClipWindow(string id)
        {
            if (App.WindowManager.EditSoundClipWindows.TryGetValue(id, out EditSoundClip window))
            {
                window.Activate();
            }
            else
            {

                Window sound = new EditSoundClip
                {
                    DataContext = new EditSoundClipViewModel(App.SoundClipManager.GetClip(id)),
                };

                sound.Show();
            }
        }

        public void OpenedEditSoundClipWindow(string id, EditSoundClip editSoundClip)
        {
            if (!string.IsNullOrEmpty(id) && !EditSoundClipWindows.ContainsKey(id))
            {
                EditSoundClipWindows.Add(id, editSoundClip);
                OnPropertyChanged(nameof(EditSoundClipWindows));
            }
        }

        public void ClosedEditSoundClipWindow(string id)
        {
            if (!string.IsNullOrEmpty(id) && EditSoundClipWindows.ContainsKey(id))
            {
                EditSoundClipWindows.Remove(id);
                OnPropertyChanged(nameof(EditSoundClipWindows));
            }
        }

        public GlobalSettings? _globalSettingsWindow = null;
        public GlobalSettings? GlobalSettingsWindow
        {
            get => _globalSettingsWindow;
            set
            {
                if (value != _globalSettingsWindow)
                {
                    _globalSettingsWindow = value;
                    OnPropertyChanged();
                }
            }
        }

        public SoundClipList? _soundClipListWindow = null;
        public SoundClipList? SoundClipListWindow
        {
            get => _soundClipListWindow;
            set
            {
                if (value != _soundClipListWindow)
                {
                    _soundClipListWindow = value;
                    OnPropertyChanged();
                }
            }
        }

        private About? _aboutWindow = null;
        public About? AboutWindow
        {
            get => _aboutWindow;
            set
            {
                if (value != _aboutWindow)
                {
                    _aboutWindow = value;
                    OnPropertyChanged();
                }
            }
        }

        private ErrorList _errorListWindow;
        public ErrorList ErrorListWindow
        {
            get
            {
                if (_errorListWindow == null)
                {
                    _errorListWindow = new ErrorList();
                }
                return _errorListWindow;
            }
        }

        public void ShowSoundClipListWindow(PixelPoint? desiredPosition)
        {
            if (SoundClipListWindow != null)
            {
                SoundClipListWindow.Activate();
            }
            else
            {
                SoundClipListWindow = new SoundClipList
                {
                    DataContext = new SoundClipListViewModel(),
                };
                if (desiredPosition != null)
                {
                    SoundClipListWindow.Position = (PixelPoint)desiredPosition;
                }
                SoundClipListWindow.Show();
            }
        }

        public void ShowGlobalSettingsWindow(PixelPoint? desiredPosition)
        {
            if (GlobalSettingsWindow != null)
            {
                GlobalSettingsWindow.Activate();
            }
            else
            {
                GlobalSettingsWindow = new GlobalSettings
                {
                    DataContext = new GlobalSettingsViewModel(),
                };
                if (desiredPosition != null)
                {
                    GlobalSettingsWindow.Position = (PixelPoint)desiredPosition;
                }
                GlobalSettingsWindow.Show();
            }
        }

        public void ShowAboutWindow(PixelPoint? desiredPosition)
        {
            if (AboutWindow != null)
            {
                AboutWindow.Activate();
            }
            else
            {
                AboutWindow = new About();
                if (desiredPosition != null)
                {
                    AboutWindow.Position = (PixelPoint)desiredPosition;
                }
                AboutWindow.Show();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
