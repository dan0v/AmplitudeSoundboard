/*
    AmplitudeSoundboard
    Copyright (C) 2021-2025 dan0v
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Timers;

namespace Amplitude.Helpers
{
    [JsonSerializable(typeof(Dictionary<string, WindowSizeAndPosition>))]
    public partial class WindowManagerContext : JsonSerializerContext { }

    public class WindowManager : INotifyPropertyChanged
    {
        private static WindowManager? _instance;
        public static WindowManager Instance => _instance ??= new WindowManager();

        public WindowManager()
        {
            windowPositionAndScaleTimer.Elapsed += WindowPositionAndScaleTimerElapsed;
        }

        private readonly object windowPositionSaveLock = new object();

        private readonly Random randomizer = new Random();

        private const int WINDOW_POSITION_AND_SCALE_TIMER_MS = 500;
        private Timer windowPositionAndScaleTimer = new Timer
        {
            Interval = WINDOW_POSITION_AND_SCALE_TIMER_MS,
            AutoReset = false,
        };

        private const string WINDOW_POSITION_FILE_LOCATION = "window-positions.json";

        private Dictionary<string, WindowSizeAndPosition> windowSizesAndPositions = new();

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

        private Dictionary<string, EditOutputProfile> _editOutputProfileWindows = new Dictionary<string, EditOutputProfile>();
        public Dictionary<string, EditOutputProfile> EditOutputProfileWindows
        {
            get => _editOutputProfileWindows;
            set
            {
                if (value != _editOutputProfileWindows)
                {
                    _editOutputProfileWindows = value;
                    OnPropertyChanged();
                }
            }
        }

        public double DesktopScaling => MainWindow?.DesktopScaling ?? 1;

        public void OpenEditOutputProfileWindow(string? Id = null)
        {
            if (Id != null && EditOutputProfileWindows.TryGetValue(Id, out EditOutputProfile? window))
            {
                if (window?.WindowState == WindowState.Minimized)
                {
                    window.WindowState = WindowState.Normal;
                }
                window?.Activate();
            }
            else
            {
                EditOutputProfile outputProf = new EditOutputProfile();
                OutputProfile? profile = App.OutputProfileManager.GetOutputProfile(Id);

                outputProf.DataContext = profile == null ? new EditOutputProfileViewModel() : new EditOutputProfileViewModel(profile);

                if (windowSizesAndPositions.TryGetValue(EditOutputProfile.WindowId, out var info))
                {
                    SetAvailableWindowDetails(outputProf, info);
                }

                PixelPoint? pos = SoundClipListWindow?.Position ?? MainWindow?.Position;
                if (pos != null)
                {
                    outputProf.Position = new PixelPoint(pos.Value.X + randomizer.Next(50, 100), pos.Value.Y + randomizer.Next(50, 100));
                }

                outputProf.Show();
            }
        }

        public void OpenedEditOutputProfileWindow(string id, EditOutputProfile editOutputProfile)
        {
            if (!string.IsNullOrEmpty(id) && !EditOutputProfileWindows.ContainsKey(id))
            {
                EditOutputProfileWindows.Add(id, editOutputProfile);
                OnPropertyChanged(nameof(EditOutputProfileWindows));
            }
        }

        public void ClosedEditOutputProfileWindow(string Id)
        {
            if (!string.IsNullOrEmpty(Id) && EditOutputProfileWindows.ContainsKey(Id))
            {
                EditOutputProfileWindows.Remove(Id);
                OnPropertyChanged(nameof(EditOutputProfileWindows));
            }
        }

        public void OpenEditSoundClipWindow(string? id = null)
        {
            if (id != null && EditSoundClipWindows.TryGetValue(id, out EditSoundClip? window))
            {
                if (window?.WindowState == WindowState.Minimized)
                {
                    window.WindowState = WindowState.Normal;
                }
                window?.Activate();
            }
            else
            {
                Window sound = new EditSoundClip();
                SoundClip? clip = App.SoundClipManager.GetClip(id);

                sound.DataContext = clip == null ? new EditSoundClipViewModel() : new EditSoundClipViewModel(clip);

                if (windowSizesAndPositions.TryGetValue(EditSoundClip.WindowId, out var info))
                {
                    SetAvailableWindowDetails(sound, info);
                }

                PixelPoint? pos = SoundClipListWindow?.Position ?? MainWindow?.Position;
                if (pos != null)
                {
                    sound.Position = new PixelPoint(pos.Value.X + randomizer.Next(50, 100), pos.Value.Y + randomizer.Next(50, 100));
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

        public void OpenedEditSoundClipWindow(string? id, EditSoundClip editSoundClip)
        {
            if (!string.IsNullOrEmpty(id) && !EditSoundClipWindows.ContainsKey(id))
            {
                EditSoundClipWindows.Add(id, editSoundClip);
                OnPropertyChanged(nameof(EditSoundClipWindows));
            }
        }

        public void ClosedEditSoundClipWindow(string? id)
        {
            if (!string.IsNullOrEmpty(id) && EditSoundClipWindows.ContainsKey(id))
            {
                EditSoundClipWindows.Remove(id);
                OnPropertyChanged(nameof(EditSoundClipWindows));
            }
        }

        private MainWindow? _mainWindow = null;
        public MainWindow? MainWindow => _mainWindow;

        public void SetMainWindow(MainWindow window)
        {
            if (windowSizesAndPositions.TryGetValue(MainWindow.WindowId, out var mainWindowData))
            {
                if (mainWindowData != null)
                {
                    if (mainWindowData.Height.HasValue)
                    {
                        window.Height = mainWindowData.Height.Value;
                    }
                    if (mainWindowData.Width.HasValue)
                    {
                        window.Width = mainWindowData.Width.Value;
                    }
                    if (mainWindowData.WindowPosition != null)
                    {
                        window.Position = mainWindowData.WindowPosition.ToPixelPoint();
                    }
                }
            }

            _mainWindow = window;
        }

        private GlobalSettings? _globalSettingsWindow = null;
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

        private ThemeSettings? _themeSettingsWindow = null;
        public ThemeSettings? ThemeSettingsWindow
        {
            get => _themeSettingsWindow;
            set
            {
                if (value != _themeSettingsWindow)
                {
                    _themeSettingsWindow = value;
                    OnPropertyChanged();
                }
            }
        }

        private SoundClipList? _soundClipListWindow = null;
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
        private ErrorList? _errorListWindow = null;
        public ErrorList ErrorListWindow
        {
            get
            {
                _errorListWindow ??= new ErrorList
                {
                    DataContext = new ErrorListViewModel(),
                };

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

            ((ErrorListViewModel?)ErrorListWindow.DataContext)?.AddErrorString(errorString);
            ShowErrorListWindow();
        }

        public void ShowErrorOutputProfile(OutputProfile profile, ErrorListViewModel.OutputProfileErrorType errorType, string? additionalData = null)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ShowErrorOutputProfile(profile, errorType, additionalData);
                });
                return;
            }
            ((ErrorListViewModel?)ErrorListWindow.DataContext)?.AddErrorOutputProfile(profile, errorType, additionalData);
            ShowErrorListWindow();
        }

        public void ShowErrorSoundClip(SoundClip clip, ErrorListViewModel.SoundClipErrorType errorType, string? additionalData = null)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ShowErrorSoundClip(clip, errorType, additionalData);
                });
                return;
            }
            ((ErrorListViewModel?)ErrorListWindow.DataContext)?.AddErrorSoundClip(clip, errorType, additionalData);
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

        public void ShowSoundClipListWindow(PixelPoint? fallbackPosition = null)
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
                if (windowSizesAndPositions.TryGetValue(SoundClipList.WindowId, out var soundClipsInfo))
                {
                    SetAvailableWindowDetails(SoundClipListWindow, soundClipsInfo);
                }
                if (fallbackPosition != null && soundClipsInfo?.WindowPosition == null)
                {
                    SoundClipListWindow.Position = fallbackPosition.Value;
                }
                SoundClipListWindow.Show();
            }
        }

        public void ShowGlobalSettingsWindow(PixelPoint? fallbackPosition = null)
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
                if (windowSizesAndPositions.TryGetValue(GlobalSettings.WindowId, out var info))
                {
                    SetAvailableWindowDetails(GlobalSettingsWindow, info);
                }
                if (fallbackPosition != null && info?.WindowPosition == null)
                {
                    GlobalSettingsWindow.Position = (PixelPoint)fallbackPosition;
                }
                GlobalSettingsWindow.Show();
            }
        }

        public void ShowThemeSettingsWindow(PixelPoint? fallbackPosition = null)
        {
            if (ThemeSettingsWindow != null)
            {
                if (ThemeSettingsWindow.WindowState == WindowState.Minimized)
                {
                    ThemeSettingsWindow.WindowState = WindowState.Normal;
                }
                ThemeSettingsWindow.Activate();
            }
            else
            {
                ThemeSettingsWindow = new ThemeSettings
                {
                    DataContext = new ThemeSettingsViewModel(),
                };
                if (windowSizesAndPositions.TryGetValue(ThemeSettings.WindowId, out var info))
                {
                    SetAvailableWindowDetails(ThemeSettingsWindow, info);
                }
                if (fallbackPosition != null && info?.WindowPosition == null)
                {
                    ThemeSettingsWindow.Position = (PixelPoint)fallbackPosition;
                }
                ThemeSettingsWindow.Show();
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

        public void WindowSizesOrPositionsChanged()
        {
            if (!windowPositionAndScaleTimer.Enabled)
            {
                windowPositionAndScaleTimer.Enabled = true;
            }
            else
            {
                windowPositionAndScaleTimer.Interval = WINDOW_POSITION_AND_SCALE_TIMER_MS;
            }
        }

        public async void SaveWindowSizesAndPositions()
        {
            var dict = CaptureWindowSizesAndPositions();
            await Task.Run(() =>
            {
                lock (windowPositionSaveLock)
                {
                    windowSizesAndPositions[MainWindow.WindowId] = dict[MainWindow.WindowId];

                    if (SoundClipListWindow != null)
                    {
                        windowSizesAndPositions[SoundClipList.WindowId] = dict[SoundClipList.WindowId];
                    }

                    if (GlobalSettingsWindow != null)
                    {
                        windowSizesAndPositions[GlobalSettings.WindowId] = dict[GlobalSettings.WindowId];
                    }

                    if (EditSoundClipWindows.Any())
                    {
                        windowSizesAndPositions[EditSoundClip.WindowId] = dict[EditSoundClip.WindowId];
                    }

                    try
                    {
                        var json = App.JsonIoManager.ConvertObjectsToJSON(windowSizesAndPositions);
                        App.JsonIoManager.SaveJSONToFile(WINDOW_POSITION_FILE_LOCATION, json);
                    }
                    catch { }
                }
            });
        }

        private Dictionary<string, WindowSizeAndPosition> CaptureWindowSizesAndPositions()
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                return Dispatcher.UIThread.InvokeAsync(CaptureWindowSizesAndPositions).Result;
            }

            var soundClips = new WindowSizeAndPosition(null, null, null);
            if (EditSoundClipWindows.Any())
            {
                var lastTouchedWindow = EditSoundClipWindows.OrderByDescending(s => s.Value.lastTouchedTime).First().Value;
                var maxHeight = lastTouchedWindow.Height;
                var maxWidth = lastTouchedWindow.Width;
                soundClips = new WindowSizeAndPosition(null, maxHeight, maxWidth);
            }
            return new Dictionary<string, WindowSizeAndPosition>()
            {
                { MainWindow.WindowId, new WindowSizeAndPosition(MainWindow?.Position, MainWindow?.Height, MainWindow?.Width) },
                { SoundClipList.WindowId, new WindowSizeAndPosition(SoundClipListWindow?.Position, SoundClipListWindow?.Height, SoundClipListWindow?.Width) },
                { GlobalSettings.WindowId, new WindowSizeAndPosition(null, GlobalSettingsWindow?.Height, GlobalSettingsWindow?.Width) },
                { EditSoundClip.WindowId, soundClips },
            };
        }

        public void ReadWindowSizesAndPositions()
        {
            lock (windowPositionSaveLock)
            {
                try
                {
                    var saved = App.JsonIoManager.RetrieveJSONFromFile(WINDOW_POSITION_FILE_LOCATION);
                    var processed = App.JsonIoManager.ConvertObjectsFromJSON<Dictionary<string, WindowSizeAndPosition>>(saved);

                    if (processed != null)
                    {
                        windowSizesAndPositions = processed;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }
        private void SetAvailableWindowDetails(Window window, WindowSizeAndPosition info)
        {
            if (info.WindowPosition != null)
            {
                window.Position = info.WindowPosition.ToPixelPoint();
            }
            if (info.Height.HasValue)
            {
                window.Height = info.Height.Value;
            }
            if (info.Width.HasValue)
            {
                window.Width = info.Width.Value;
            }
        }

        public void ClearWindowSizesAndPositions()
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.InvokeAsync(ClearWindowSizesAndPositions);
                return;
            }

            lock (windowPositionSaveLock)
            {
                windowSizesAndPositions = new();
                try
                {
                    File.Delete(WINDOW_POSITION_FILE_LOCATION);
                }
                catch { }
            }

            foreach (Window win in EditSoundClipWindows.Values)
            {
                win.Close();
            }
            EditSoundClipWindows.Clear();

            SoundClipListWindow?.Close();
            AboutWindow?.Close();
            GlobalSettingsWindow?.Close();
            ThemeSettingsWindow?.Close();
        }

        private (double height, double width) lastMainWindowSize;

        private void WindowPositionAndScaleTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    WindowPositionAndScaleTimerElapsed(sender, e);
                });
                return;
            }

            if (MainWindow != null)
            {
                var newWindowSize = MainWindow.WindowSize;

                if (App.ConfigManager.Config.AutoScaleTilesToWindow && lastMainWindowSize != newWindowSize)
                {
                    lastMainWindowSize = newWindowSize;
                    App.SoundClipManager.RescaleAllBackgroundImages();
                }
            }

            App.WindowManager.SaveWindowSizesAndPositions();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
