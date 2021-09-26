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
using Amplitude.Models;
using AmplitudeSoundboard;
using Avalonia.Input;

namespace Amplitude.ViewModels
{
    public class SoundBoardGridItemViewModel : ViewModelBase
    {
        static ThemeHandler ThemeHandler { get => App.ThemeHandler; }
        static SoundClipManager Manager { get => App.SoundClipManager; }
        static OptionsManager OptionsManager { get => App.OptionsManager; }
        static WindowManager WindowManager { get => App.WindowManager; }

        private string soundClipId = "";

        private int Height => OptionsManager.Options.GridTileHeight;
        private int Width => OptionsManager.Options.GridTileWidth;

        private int row = 0;
        private int col = 0;

        private string ModelName { get => SoundClipExists ? Model.Name : ""; }//Localization.Localizer.Instance["EmptyTilePrompt"]; }

        private Cursor Cursor { get => SoundClipExists ? new Cursor(StandardCursorType.Hand) : new Cursor(StandardCursorType.Arrow); }

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
                    OnPropertyChanged(nameof(ModelName));
                }
            }
        }

        private SoundClip _model = new SoundClip();
        public SoundClip Model
        {
            get
            {
                SoundClip? model = Manager.GetClip(soundClipId, true);
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
            Manager.PropertyChanged += Manager_PropertyChanged;
            OptionsManager.PropertyChanged += OptionsManager_PropertyChanged;
            WindowManager.PropertyChanged += WindowManager_PropertyChanged;

            if (WindowManager.GlobalSettingsWindow == null)
            {
                GlobalSettingsWindowOpen = false;
            }
            else
            {
                GlobalSettingsWindowOpen = true;
            }

            this.soundClipId = clipId;
        }

        private void WindowManager_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {if (e.PropertyName == nameof(WindowManager.GlobalSettingsWindow))
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

        public void Unbind()
        {
            this.soundClipId = "";
            OnPropertyChanged(nameof(Model));
            OnPropertyChanged(nameof(ModelName));
        }

        public void PasteClip()
        {
            soundClipId = Manager.CopiedClipId;
            Manager.CopiedClipId = "";
            OptionsManager.Options.GridSoundClipIds[row, col] = soundClipId;
            OnPropertyChanged(nameof(Model));
            OnPropertyChanged(nameof(ModelName));
            OptionsManager.SaveAndOverwriteOptions(OptionsManager.Options);
        }

        private void Manager_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Manager.SoundClips))
            {
                OnPropertyChanged(nameof(Model));
            }
        }

        private void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.ImageFilePath))
            {
                OnPropertyChanged(ModelName);
            }
        }

        public override void Dispose()
        {
            Manager.PropertyChanged -= Manager_PropertyChanged;
            OptionsManager.PropertyChanged -= OptionsManager_PropertyChanged;
            WindowManager.PropertyChanged -= WindowManager_PropertyChanged;
            base.Dispose();
        }
    }
}