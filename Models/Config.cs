/*
    AmplitudeSoundboard
    Copyright (C) 2021-2024 dan0v
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

using Amplitude.Localization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Amplitude.Models
{
    public class Config : INotifyPropertyChanged
    {
        private const int DEFAULT_GRID_SIZE = 5;
        public string?[][] GridSoundClipIds = new string?[DEFAULT_GRID_SIZE][];

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

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public int ActualTileHeight = 100;
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public int ActualTileWidth = 100;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public int DesiredImageHeight => AutoScaleTilesToWindow ? ActualTileHeight : GridTileHeight ?? 1;
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public int DesiredImageWidth => AutoScaleTilesToWindow ? ActualTileWidth : GridTileWidth ?? 1;

        private bool _checkForUpdates = true;
        public bool CheckForUpdates
        {
            get => _checkForUpdates;
            set
            {
                if (value != _checkForUpdates)
                {
                    _checkForUpdates = value;
                    OnPropertyChanged();
                }
            }
        }

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

        private bool _stopAudioOnRepeatTrigger = false;
        public bool StopAudioOnRepeatTrigger
        {
            get => _stopAudioOnRepeatTrigger;
            set
            {
                if (value != _stopAudioOnRepeatTrigger)
                {
                    _stopAudioOnRepeatTrigger = value;
                    OnPropertyChanged();
                }
            }
        }

        public Config()
        {
            Language = Localizer.Instance.TryUseSystemLanguageFallbackEnglish();

            for (int row = 0; row < DEFAULT_GRID_SIZE; row++)
            {
                GridSoundClipIds[row] = new string[DEFAULT_GRID_SIZE];
            }
        }

        public void ApplyGridSizing()
        {
            string?[][] newGrid = new string[_gridRows ?? 1][];
            
            for (int row = 0; row < newGrid.Length; row++)
            {
                newGrid[row] = new string?[_gridColumns ?? 1];

                if (row >= GridSoundClipIds.Length)
                {
                    continue;
                }
                for (int col = 0; col < newGrid[row].Length; col++)
                {
                    if (GridSoundClipIds[row] == null || col >= GridSoundClipIds[row]?.Length)
                    {
                        break;
                    }
                    newGrid[row][col] = GridSoundClipIds[row][col];
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

        public Config ShallowCopy()
        {
            return (Config)this.MemberwiseClone();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
