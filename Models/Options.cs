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

using AmplitudeSoundboard;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Amplitude.Models
{
    public class Options : INotifyPropertyChanged
    {
        private string _language = "English";
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
        public string Theme
        {
            get => _theme;
            set
            {
                if (value != _theme)
                {
                    _theme = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _masterVolume = 100;
        public int MasterVolume
        {
            get => _masterVolume;
            set
            {
                if (value != _masterVolume)
                {
                    _masterVolume = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _defaultOutputDevice = "";
        public string DefaultOutputDevice
        {
            get => _defaultOutputDevice;
            set
            {
                if (value != _defaultOutputDevice)
                {
                    _defaultOutputDevice = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _globalKillAudioHotkey;
        public string GlobalKillAudioHotkey
        {
            get => _globalKillAudioHotkey;
            set
            {
                if (value != _globalKillAudioHotkey)
                {
                    _globalKillAudioHotkey = value;
                    OnPropertyChanged();
                }
            }
        }

        public Options() { }

        public static Options RetrieveOptionsFromJSON()
        {
            try
            {
                if (File.Exists(Path.Join(App.APP_STORAGE, @"options.json")))
                {
                    string json = File.ReadAllText(Path.Join(App.APP_STORAGE, @"options.json"));
                    return (Options?)JsonConvert.DeserializeObject(json, typeof(Options));
                }
            }
            catch (Exception e)
            {
                App.WindowManager.ErrorListWindow.AddErrorString(e.Message);
            }
            return new Options();
        }

        public void SaveOptions()
        {
            App.ThemeHandler.SelectedTheme = Theme;
            try
            {
                File.WriteAllText(Path.Join(App.APP_STORAGE, @"options.json"), ToJSON());
            }
            catch (Exception e)
            {
                App.WindowManager.ErrorListWindow.AddErrorString(e.Message);
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
