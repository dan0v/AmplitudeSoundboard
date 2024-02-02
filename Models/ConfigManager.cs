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

using Amplitude.Helpers;
using Amplitude.Localization;
using Amplitude.ViewModels;
using AmplitudeSoundboard;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Amplitude.Models
{
    [JsonSerializable(typeof(Config))]
    public partial class ConfigManagerContext : JsonSerializerContext { }

    public class ConfigManager : INotifyPropertyChanged
    {
        private static ConfigManager? _instance;
        public static ConfigManager Instance => _instance ??= new ConfigManager();

        private Config _config;
        public Config Config => _config;

        private const string CONFIG_FILE_LOCATION = "options.json";

        private ConfigManager()
        {
            var retrievedConfig = RetrieveConfigFromFile();

            if (retrievedConfig != null)
            {
                _config = retrievedConfig;

                if (string.IsNullOrEmpty(_config.Language))
                {
                    _config.Language = Localizer.Instance.TryUseSystemLanguageFallbackEnglish();
                }
                else
                {
                    Localizer.Instance.ChangeLanguage(_config.Language);
                }

                RegisterConfigHotkeys(Config);
            }
            else
            {
                _config = new Config();
            }

            OnPropertyChanged(nameof(Config));
        }

        private void RegisterConfigHotkeys(Config value)
        {
            App.HotkeysManager.RegisterHotkeyAtStartup(HotkeysManager.MASTER_STOP_SOUND_HOTKEY, value.GlobalKillAudioHotkey);
        }

        public void SaveAndOverwriteConfig(Config config)
        {
            try
            {
                config.ValidateAndCorrectGridLayoutSettings();

                if (_config.GridRows != config.GridRows
                    || _config.GridColumns != config.GridColumns
                    || _config.GridTileHeight != config.GridTileHeight
                    || _config.GridTileWidth != config.GridTileWidth)
                {
                    config.ApplyGridSizing();
                }
                if (_config.GlobalKillAudioHotkey != config.GlobalKillAudioHotkey)
                {
                    App.HotkeysManager.RemoveHotkey(HotkeysManager.MASTER_STOP_SOUND_HOTKEY, Config.GlobalKillAudioHotkey);
                    App.HotkeysManager.RegisterHotkeyAtStartup(HotkeysManager.MASTER_STOP_SOUND_HOTKEY, config.GlobalKillAudioHotkey);
                }
                if (_config.Language != config.Language)
                {
                    Localizer.Instance.ChangeLanguage(config.Language);
                }
                if (_config.ThemeId != config.ThemeId)
                {
                    App.ThemeHandler.SelectTheme(config.ThemeId);
                }

                var json = App.JsonIoManager.ConvertObjectsToJSON(config);
                App.JsonIoManager.SaveJSONToFile(CONFIG_FILE_LOCATION, json);
                _config = config;
            }
            catch (Exception e)
            {
                App.WindowManager.ShowErrorString(e.Message);
            }
            OnPropertyChanged(nameof(Config));
            App.SoundClipManager.RescaleAllBackgroundImages();
        }

        public Config? RetrieveConfigFromFile()
        {
            try
            {
                string json = App.JsonIoManager.RetrieveJSONFromFile(CONFIG_FILE_LOCATION);
                if (!string.IsNullOrEmpty(json)) 
                {
                    return App.JsonIoManager.ConvertObjectsFromJSON<Config>(json);
                }
            }
            catch (Exception e)
            {
                App.WindowManager.ShowErrorString(e.Message);
            }
            return null;
        }

        public List<GridItemRow> GetGridLayout()
        {
            List<GridItemRow> list = [];
            for (int row = 0; row < Config.GridSoundClipIds.Length; row++)
            {
                GridItemRow rowItem = new GridItemRow();

                if (Config.GridSoundClipIds[row] == null)
                {
                    for (int col = 0; col < Config.GridColumns; col++)
                    {
                        rowItem.List.Add(new SoundBoardGridItemViewModel(null, row, col));
                    }
                }
                else
                {
                    for (int col = 0; col < Config.GridSoundClipIds[row].Length; col++)
                    {
                        rowItem.List.Add(new SoundBoardGridItemViewModel(Config.GridSoundClipIds[row][col], row, col));
                    }
                }
                
                list.Add(rowItem);
            }
            return list;
        }

        public void ComputeGridTileSizes()
        {
            var rows = Config.GridRows;
            Config.ActualTileHeight = (int)(((App.WindowManager.MainWindow?.GridSize.height - (8 * rows) - 15) / rows) ?? Config.GridTileHeight ?? 100);
            
            var cols = Config.GridColumns;
            Config.ActualTileWidth = (int)(((App.WindowManager.MainWindow?.GridSize.width - (10 * cols) - 10) / cols) ?? Config.GridTileWidth ?? 100);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
