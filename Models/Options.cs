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

using Amplitude.Helpers;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amplitude.Models
{
    public class Options : INotifyPropertyChanged
    {
        public string[,] GridSoundClipIds = new string[5, 5];

        private string _language = ""; // Start blank, so that system language can be attempted first
        public string Language
        {
            get => _language;
            set
            {
                if (value != _language)
                {
                    _language = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _theme = "Dark";
        [Obsolete]
        public string Theme
        {
            internal get => _theme;
            set
            {
                if (value != _theme)
                {
                    _theme = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _themeId = 0;
        public int ThemeId
        {
            get => _themeId;
            set
            {
                // UI sends -1 when theme list is refreshed, so ignore this
                if (value != -1 && value != _themeId)
                {
                    _themeId = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _masterVolume = 100;
        [Obsolete]
        public int MasterVolume
        {
            internal get => _masterVolume;
            set
            {
                OutputSettings.Volume = value;
            }
        }

        private string _defaultOutputDevice = ISoundEngine.DEFAULT_DEVICE_NAME;
        [Obsolete]
        public string DefaultOutputDevice
        {
            internal get => _defaultOutputDevice;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    OutputSettings.DeviceName = value;
                }
            }
        }

        private OutputSettings _outputSettings = new OutputSettings();
        public OutputSettings OutputSettings
        {
            get => _outputSettings;
            set
            {
                if (value != _outputSettings)
                {
                    _outputSettings = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _globalKillAudioHotkey = "";
        public string GlobalKillAudioHotkey
        {
            get => _globalKillAudioHotkey;
            set
            {
                if (value != _globalKillAudioHotkey)
                {
                    _globalKillAudioHotkey = value;
                }
                OnPropertyChanged(); // Alert even if not changed
            }
        }

        private int? _gridRows = 5;
        public int? GridRows
        {
            get => _gridRows;
            set
            {
                if (value != _gridRows)
                {
                    _gridRows = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _gridColumns = 5;
        public int? GridColumns
        {
            get => _gridColumns;
            set
            {
                if (value != _gridColumns)
                {
                    _gridColumns = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _autoScaleTilesToWindow = true;
        public bool AutoScaleTilesToWindow
        {
            get => _autoScaleTilesToWindow;
            set
            {
                if (value != _autoScaleTilesToWindow)
                {
                    _autoScaleTilesToWindow = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _gridTileHeight = 100;
        public int? GridTileHeight
        {
            get => _gridTileHeight;
            set
            {
                if (value != _gridTileHeight)
                {
                    _gridTileHeight = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _gridTileWidth = 100;
        public int? GridTileWidth
        {
            get => _gridTileWidth;
            set
            {
                if (value != _gridTileWidth)
                {
                    _gridTileWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public int ActualTileHeight = 100;
        [JsonIgnore]
        public int ActualTileWidth = 100;

        [JsonIgnore]
        public int DesiredImageHeight => AutoScaleTilesToWindow ? ActualTileHeight : GridTileHeight ?? 1;
        [JsonIgnore]
        public int DesiredImageWidth => AutoScaleTilesToWindow ? ActualTileWidth : GridTileWidth ?? 1;

        private bool _hideTutorial = false;
        public bool HideTutorial
        {
            get => _hideTutorial;
            set
            {
                if (value != _hideTutorial)
                {
                    _hideTutorial = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _cacheAudio = true;
        public bool CacheAudio
        {
            get => _cacheAudio;
            set
            {
                if (value != _cacheAudio)
                {
                    _cacheAudio = value;
                    if (!_cacheAudio)
                    {
                        PreCacheAudio = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private bool _preCacheAudio = false;
        public bool PreCacheAudio
        {
            get => _preCacheAudio;
            set
            {
                if (value != _preCacheAudio)
                {
                    _preCacheAudio = value;
                    OnPropertyChanged();
                }
            }
        }

        public Options() { }

        public void ApplyGridSizing()
        {
            string[,] newGrid = new string[_gridRows ?? 1, _gridColumns ?? 1];

            for (int row = 0; row <= newGrid.GetUpperBound(0); row++)
            {
                if (row > GridSoundClipIds.GetUpperBound(0))
                {
                    break;
                }
                for (int col = 0; col <= newGrid.GetUpperBound(1); col++)
                {
                    if (col > GridSoundClipIds.GetUpperBound(1))
                    {
                        break;
                    }
                    newGrid[row, col] = GridSoundClipIds[row, col];
                }
            }

            GridSoundClipIds = newGrid;
        }

        public void ValidateAndCorrectGridLayoutSettings()
        {
            if (GridRows == null || GridRows < 1)
            {
                GridRows = 1;
            }
            if (GridColumns == null || GridColumns < 1)
            {
                GridColumns = 1;
            }
            if (GridTileHeight == null || GridTileHeight < 1)
            {
                GridTileHeight = 1;
            }
            if (GridTileWidth == null || GridTileWidth < 1)
            {
                GridTileWidth = 1;
            }
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public Options ShallowCopy()
        {
            return (Options)this.MemberwiseClone();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
