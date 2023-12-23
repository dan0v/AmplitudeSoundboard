/*
    AmplitudeSoundboard
    Copyright (C) 2021-2023 dan0v
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
using Amplitude.Models;
using Amplitude.Views;
using Avalonia.Input;

namespace Amplitude.ViewModels
{
    public sealed class SoundBoardGridItemViewModel : ViewModelBase
    {
        private string soundClipId = "";

        private double Height => OptionsManager.Options.AutoScaleTilesToWindow ? GetHeight() : OptionsManager.Options.GridTileHeight ?? 0;
        private double Width => OptionsManager.Options.AutoScaleTilesToWindow ? GetWidth() : OptionsManager.Options.GridTileWidth ?? 0;

        private int row = 0;
        private int col = 0;

        private Cursor Cursor => SoundClipExists ? new Cursor(StandardCursorType.Hand) : new Cursor(StandardCursorType.Arrow);

        private bool _soundClipExists = false;
        private bool SoundClipExists
        {
            get => _soundClipExists;
            set
            {
                if (value != _soundClipExists)
                {
                    _soundClipExists = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Cursor));
                }
            }
        }

        private SoundClip _model = new SoundClip();
        public SoundClip Model
        {
            get
            {
                SoundClip? model = SoundClipManager.GetClip(soundClipId, true);
                if (model != null)
                {
                    _model = model;
                    SoundClipExists = true;
                    return _model;
                }
                else if (!SoundClipExists)
                {
                    return _model;
                }
                else
                {
                    SoundClipExists = false;
                    _model = new SoundClip();
                    return _model;
                }
            }
        }

        private float _backgroundOpacity = 1f;
        public float BackgroundOpacity
        {
            get => _backgroundOpacity;
            set
            {
                if (value != _backgroundOpacity)
                {
                    _backgroundOpacity = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _globalSettingsWindowOpen = false;
        public bool GlobalSettingsWindowOpen
        {
            get => _globalSettingsWindowOpen;
            set
            {
                if (value != _globalSettingsWindowOpen)
                {
                    _globalSettingsWindowOpen = value;
                    if (_globalSettingsWindowOpen)
                    {
                        BackgroundOpacity = 0.5f;
                    }
                    else
                    {
                        BackgroundOpacity = 1f;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public SoundBoardGridItemViewModel() { }

        public SoundBoardGridItemViewModel(string clipId, int row, int col)
        {
            this.row = row;
            this.col = col;
            SoundClipManager.PropertyChanged += Manager_PropertyChanged;
            OptionsManager.PropertyChanged += OptionsManager_PropertyChanged;
            WindowManager.PropertyChanged += WindowManager_PropertyChanged;
            Model.PropertyChanged += Model_PropertyChanged;

            if (WindowManager.MainWindow != null)
            {
                WindowManager.MainWindow.PropertyChanged += MainWindow_PropertyChanged;
            }
            if (WindowManager.GlobalSettingsWindow == null)
            {
                GlobalSettingsWindowOpen = false;
            }
            else
            {
                GlobalSettingsWindowOpen = true;
            }

            this.soundClipId = clipId;
            Model.LoadBackgroundImage = true;
        }

        private void MainWindow_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindow.GridSize))
            {
                OnPropertyChanged(nameof(Height));
                OnPropertyChanged(nameof(Width));
            }
        }

        private void WindowManager_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WindowManager.GlobalSettingsWindow))
            {
                if (WindowManager.GlobalSettingsWindow == null)
                {
                    GlobalSettingsWindowOpen = false;
                }
                else
                {
                    GlobalSettingsWindowOpen = true;
                }
            }
        }

        private void OptionsManager_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OptionsManager.Options))
            {
                OnPropertyChanged(nameof(Height));
                OnPropertyChanged(nameof(Width));
            }
        }

        private double GetWidth()
        {
            var cols = OptionsManager.Options.GridColumns;
            // TODO this is wasteful, but fine for now
            OptionsManager.Options.ActualTileWidth = (int)(((WindowManager.MainWindow?.GridSize.width  - (11 * (cols + 1))) / cols) ?? OptionsManager.Options.GridTileWidth ?? 100);
            return OptionsManager.Options.ActualTileWidth;
        }

        private double GetHeight()
        {
            var rows = OptionsManager.Options.GridRows;
            // TODO this is wasteful, but fine for now
            OptionsManager.Options.ActualTileHeight = (int)(((WindowManager.MainWindow?.GridSize.height - (11 * (rows + 1))) / rows) ?? OptionsManager.Options.GridTileHeight ?? 100);
            return OptionsManager.Options.ActualTileHeight;
        }

        public void Unbind()
        {
            Model.LoadBackgroundImage = false;
            this.soundClipId = "";
            OptionsManager.Options.GridSoundClipIds[row, col] = null;
            OnPropertyChanged(nameof(Model));
            OptionsManager.SaveAndOverwriteOptions(OptionsManager.Options);
        }

        public void PasteClip()
        {
            if (!string.IsNullOrEmpty(soundClipId))
            {
                Model.LoadBackgroundImage = false;
            }
            soundClipId = SoundClipManager.CopiedClipId;
            SoundClipManager.CopiedClipId = "";
            OptionsManager.Options.GridSoundClipIds[row, col] = soundClipId;
            OnPropertyChanged(nameof(Model));
            OptionsManager.SaveAndOverwriteOptions(OptionsManager.Options);
        }

        public void CreateClipInPlace()
        {
            WindowManager.OpenEditSoundClipWindow((row, col));
        }

        private void Manager_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SoundClipManager.SoundClips))
            {
                OnPropertyChanged(nameof(Model));
            }
        }

        private void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.LoadBackgroundImage))
            {
                if (!Model.LoadBackgroundImage)
                {
                    Model.LoadBackgroundImage = true;
                }
            }
        }

        public override void Dispose()
        {
            SoundClipManager.PropertyChanged -= Manager_PropertyChanged;
            OptionsManager.PropertyChanged -= OptionsManager_PropertyChanged;
            WindowManager.PropertyChanged -= WindowManager_PropertyChanged;
            Model.PropertyChanged -= Model_PropertyChanged;
            base.Dispose();
        }
    }
}