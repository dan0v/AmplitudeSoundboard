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

using Amplitude.Helpers;
using Amplitude.Models;
using Amplitude.Views;
using Avalonia.Input;

namespace Amplitude.ViewModels
{
    public sealed class SoundBoardGridItemViewModel : ViewModelBase
    {
        private string? soundClipId = null;

        public double Height
        {
            get
            {
                if (ConfigManager.Config.AutoScaleTilesToWindow)
                {
                    return ConfigManager.Config.ActualTileHeight;
                }

                return ConfigManager.Config.GridTileHeight ?? 100;
            }
        }

        public double Width
        {
            get
            {
                if (ConfigManager.Config.AutoScaleTilesToWindow)
                {
                    return ConfigManager.Config.ActualTileWidth;
                }

                return ConfigManager.Config.GridTileWidth ?? 100;
            }
        }

        private readonly int row = 0;
        private readonly int col = 0;

        public Cursor Cursor => SoundClipExists ? new Cursor(StandardCursorType.Hand) : new Cursor(StandardCursorType.Arrow);

        private bool _soundClipExists = false;
        public bool SoundClipExists
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

        private SoundClip _model = new();
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

        public SoundBoardGridItemViewModel(string? clipId, int row, int col)
        {
            this.row = row;
            this.col = col;
            SoundClipManager.PropertyChanged += Manager_PropertyChanged;
            ConfigManager.PropertyChanged += ConfigManager_PropertyChanged;
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
                ConfigManager.ComputeGridTileSizes();
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

        private void ConfigManager_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ConfigManager.Config))
            {
                ConfigManager.ComputeGridTileSizes();
                OnPropertyChanged(nameof(Height));
                OnPropertyChanged(nameof(Width));
            }
        }

        public void Unbind()
        {
            Model.LoadBackgroundImage = false;
            this.soundClipId = "";
            ConfigManager.Config.GridSoundClipIds[row][col] = null;
            OnPropertyChanged(nameof(Model));
            ConfigManager.SaveAndOverwriteConfig(ConfigManager.Config);
        }

        public void PasteClip()
        {
            if (SoundClipManager.GetClip(SoundClipManager.CopiedClipId, true) == null)
            {
                SoundClipManager.CopiedClipId = "";
                return;
            }

            if (!string.IsNullOrEmpty(soundClipId))
            {
                Model.LoadBackgroundImage = false;
            }

            soundClipId = SoundClipManager.CopiedClipId;
            ConfigManager.Config.GridSoundClipIds[row][col] = soundClipId;
            OnPropertyChanged(nameof(Model));
            ConfigManager.SaveAndOverwriteConfig(ConfigManager.Config);
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
            ConfigManager.PropertyChanged -= ConfigManager_PropertyChanged;
            WindowManager.PropertyChanged -= WindowManager_PropertyChanged;
            Model.PropertyChanged -= Model_PropertyChanged;
            base.Dispose();
        }
    }
}