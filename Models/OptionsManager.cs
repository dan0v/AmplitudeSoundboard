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

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.IO;
using AmplitudeSoundboard;
using Amplitude.Helpers;
using System.Collections.Generic;
using Amplitude.ViewModels;

namespace Amplitude.Models
{
    public class OptionsManager : INotifyPropertyChanged
    {
        private static OptionsManager? _instance;
        public static OptionsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new OptionsManager();
                }
                return _instance;
            }
        }

        private Options _options;
        public Options Options { get => _options; }

        public const string OPTIONS_FILE_NAME = "options.json";

        private OptionsManager()
        {
            Options? retrievedOptions = RetrieveOptionsFromFile();

            if (retrievedOptions != null)
            {
                _options = retrievedOptions;
                RegisterOptionsHotkeys(Options);
            }
            else
            {
                _options = new Options();
            }

            OnPropertyChanged(nameof(Options));
        }

        private void RegisterOptionsHotkeys(Options value)
        {
            App.HotkeysManager.RegisterHotkeyAtStartup(HotkeysManager.MASTER_STOP_SOUND_HOTKEY, value.GlobalKillAudioHotkey);
        }

        public void SaveAndOverwriteOptions(Options options)
        {
            App.ThemeHandler.SelectedTheme = options.Theme;
            try
            {
                options.UpdateGridSize();
                File.WriteAllText(Path.Join(App.APP_STORAGE, OPTIONS_FILE_NAME), options.ToJSON());
                App.HotkeysManager.RemoveHotkey(HotkeysManager.MASTER_STOP_SOUND_HOTKEY, Options.GlobalKillAudioHotkey);
                App.HotkeysManager.RegisterHotkeyAtStartup(HotkeysManager.MASTER_STOP_SOUND_HOTKEY, options.GlobalKillAudioHotkey);
                _options = options;
            }
            catch (Exception e)
            {
                App.WindowManager.ErrorListWindow.AddErrorString(e.Message);
            }
            OnPropertyChanged(nameof(Options));
            App.SoundClipManager.RescaleAllBackgroundImages();
        }

        public Options? RetrieveOptionsFromFile()
        {
            try
            {
                if (File.Exists(Path.Join(App.APP_STORAGE, OPTIONS_FILE_NAME)))
                {
                    string json = File.ReadAllText(Path.Join(App.APP_STORAGE, OPTIONS_FILE_NAME));
                    return (Options?)JsonConvert.DeserializeObject(json, typeof(Options));
                }
            }
            catch (Exception e)
            {
                App.WindowManager.ErrorListWindow.AddErrorString(e.Message);
            }
            return null;
        }

        public List<GridItemRow> GetGridLayout()
        {
            List<GridItemRow> list = new();
            for (int row = 0; row <= Options.GridSoundClipIds.GetUpperBound(0); row++)
            {
                GridItemRow rowItem = new GridItemRow();
                for (int col = 0; col <= Options.GridSoundClipIds.GetUpperBound(1); col++)
                {
                    rowItem.List.Add(new SoundBoardGridItemViewModel(Options.GridSoundClipIds[row, col], row, col));
                }
                list.Add(rowItem);
            }
            return list;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
