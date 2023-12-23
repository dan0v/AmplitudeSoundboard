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
using Amplitude.Localization;
using Amplitude.ViewModels;
using AmplitudeSoundboard;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Amplitude.Models
{
    public class OptionsManager : INotifyPropertyChanged
    {
        private static OptionsManager? _instance;
        public static OptionsManager Instance => _instance ??= new OptionsManager();

        private Options _options;
        public Options Options => _options;

        private const string OPTIONS_FILE_LOCATION = "options.json";

        private OptionsManager()
        {
            var retrievedOptions = RetrieveOptionsFromFile();

            if (retrievedOptions != null)
            {
                _options = retrievedOptions;

                if (string.IsNullOrEmpty(_options.Language))
                {
                    _options.Language = Localizer.Instance.TryUseSystemLanguageFallbackEnglish();
                }
                else
                {
                    Localizer.Instance.ChangeLanguage(_options.Language);
                }

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
            try
            {
                options.ValidateAndCorrectGridLayoutSettings();
                options.ApplyGridSizing();
                App.JsonIoManager.SaveJSONToFile(OPTIONS_FILE_LOCATION, options.ToJSON());
                App.HotkeysManager.RemoveHotkey(HotkeysManager.MASTER_STOP_SOUND_HOTKEY, Options.GlobalKillAudioHotkey);
                App.HotkeysManager.RegisterHotkeyAtStartup(HotkeysManager.MASTER_STOP_SOUND_HOTKEY, options.GlobalKillAudioHotkey);
                _options = options;
                Localizer.Instance.ChangeLanguage(options.Language);
                App.ThemeHandler.SelectTheme(options.ThemeId);
            }
            catch (Exception e)
            {
                App.WindowManager.ShowErrorString(e.Message);
            }
            OnPropertyChanged(nameof(Options));
            App.SoundClipManager.RescaleAllBackgroundImages();
        }

        public Options? RetrieveOptionsFromFile()
        {
            try
            {
                string json = App.JsonIoManager.RetrieveJSONFromFile(OPTIONS_FILE_LOCATION);
                if (!string.IsNullOrEmpty(json)) 
                {
                    return App.JsonIoManager.ConvertObjectsFromJSON<Options>(json);
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
        public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
