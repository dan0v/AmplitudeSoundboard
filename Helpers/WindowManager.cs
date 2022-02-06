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

using Amplitude.Models;
using Amplitude.ViewModels;
using Amplitude.Views;
using AmplitudeSoundboard;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amplitude.Helpers
{
    public class WindowManager : INotifyPropertyChanged
    {
        private static WindowManager? _instance;
        public static WindowManager Instance { get => _instance ??= new WindowManager(); }

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

        public double DesktopScaling { get => MainWindow?.PlatformImpl.DesktopScaling ?? 1; }

        public void OpenEditSoundClipWindow(string? id = null)
        {
            if (id != null && App.WindowManager.EditSoundClipWindows.TryGetValue(id, out EditSoundClip window))
            {
                if (window.WindowState == WindowState.Minimized)
                {
                    window.WindowState = WindowState.Normal;
                }
                window.Activate();
            }
            else
            {
                Window sound = new EditSoundClip();
                SoundClip? clip = App.SoundClipManager.GetClip(id);

                sound.DataContext = clip == null ? new EditSoundClipViewModel() : new EditSoundClipViewModel(clip);

                PixelPoint? pos = SoundClipListWindow?.Position ?? MainWindow?.Position;
                if (pos != null)
                {
                    sound.Position = new PixelPoint(pos.Value.X + 50, pos.Value.Y + 50);
                }

                sound.Show();
            }
        }

        public void OpenEditSoundClipWindow((int row, int col) addToCell)
        {
            Window sound = new EditSoundClip();
            sound.DataContext = new EditSoundClipViewModel(addToCell);

            PixelPoint? pos = MainWindow?.Position;
            if (pos != null)
            {
                sound.Position = new PixelPoint(pos.Value.X + 50, pos.Value.Y + 50);
            }

            sound.Show();
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

        public MainWindow? _mainWindow = null;
        public MainWindow? MainWindow
        {
            get => _mainWindow;
            set
            {
                if (value != _mainWindow)
                {
                    _mainWindow = value;
                    OnPropertyChanged();
                }
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

        public bool ErrorListWindowOpen = false;
        private ErrorList _errorListWindow;
        public ErrorList ErrorListWindow
        {
            get
            {
                if (_errorListWindow == null)
                {
                    _errorListWindow = new ErrorList
                    {
                        DataContext = new ErrorListViewModel(),
                    };
                }
                return _errorListWindow;
            }
        }

        public void ShowErrorString(string errorString)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ShowErrorString(errorString);
                });
                return;
            }

            ((ErrorListViewModel)ErrorListWindow.DataContext)?.AddErrorString(errorString);
            ShowErrorListWindow();
        }

        public void ShowErrorSoundClip(SoundClip clip, ErrorListViewModel.ErrorType errorType, string? additionalData = null)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ShowErrorSoundClip(clip, errorType, additionalData);
                });
                return;
            }
            ((ErrorListViewModel)ErrorListWindow.DataContext)?.AddErrorSoundClip(clip, errorType, additionalData);
            ShowErrorListWindow();
        }

        private void ShowErrorListWindow(PixelPoint? desiredPosition = null)
        {
            if (ErrorListWindowOpen)
            {
                if (ErrorListWindow.WindowState == WindowState.Minimized)
                {
                    ErrorListWindow.WindowState = WindowState.Normal;
                }
                ErrorListWindow.Activate();
            }
            else
            {
                ErrorListWindowOpen = true;
                if (desiredPosition != null)
                {
                    ErrorListWindow.Position = (PixelPoint)desiredPosition;
                }
                ErrorListWindow.Show();
                ErrorListWindow.Activate();
            }
        }

        public void ShowSoundClipListWindow(PixelPoint? desiredPosition = null)
        {
            if (SoundClipListWindow != null)
            {
                if (SoundClipListWindow.WindowState == WindowState.Minimized)
                {
                    SoundClipListWindow.WindowState = WindowState.Normal;
                }
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

        public void ShowGlobalSettingsWindow(PixelPoint? desiredPosition = null)
        {
            if (GlobalSettingsWindow != null)
            {
                if (GlobalSettingsWindow.WindowState == WindowState.Minimized)
                {
                    GlobalSettingsWindow.WindowState = WindowState.Normal;
                }
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

        public void ShowAboutWindow(PixelPoint? desiredPosition = null)
        {
            if (AboutWindow != null)
            {
                if (AboutWindow.WindowState == WindowState.Minimized)
                {
                    AboutWindow.WindowState = WindowState.Normal;
                }
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
