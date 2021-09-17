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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.IO;
using AmplitudeSoundboard;
using System.Collections.ObjectModel;

namespace Amplitude.Models
{
    public class SoundClipManager : INotifyPropertyChanged
    {
        private static SoundClipManager? _instance;
        public static SoundClipManager Instance 
        {
            get 
            {
                if (_instance == null) 
                {
                    _instance = new SoundClipManager();
                }
                return _instance;
            }
        }

        private ObservableCollection<SoundClip> soundClips;

        private SoundClipManager()
        {
            ObservableCollection<SoundClip>? retrievedClips = RetrieveSavedSoundClips();

            if (retrievedClips != null)
            {
                soundClips = retrievedClips;
            }
            else
            {
                soundClips = new ObservableCollection<SoundClip>();
            }

            AddClip();
        }

        public void AddClip() 
        {
            soundClips.Add(new SoundClip());

            StoreSavedSoundClips();
        }

        public SoundClip GetClip(int index) 
        {
            return soundClips[index];
        }

        public ObservableCollection<SoundClip>? RetrieveSavedSoundClips() {
            string? clipsInJson = RetrieveJSONFromFile();

            if (clipsInJson != null)
            {
                return ConvertClipsFromJSON(clipsInJson);
            }
            else
            {
                return null;
            }
        }

        public void StoreSavedSoundClips()
        {
            string clipsInJson = ConvertClipsToJSON();

            SaveJSONToFile(clipsInJson);
        }

        private static ObservableCollection<SoundClip>? ConvertClipsFromJSON(string json)
        {
            try
            {
                return (ObservableCollection<SoundClip>?)JsonConvert.DeserializeObject(json, typeof(ObservableCollection<SoundClip>));
            }
            catch (Exception e)
            {
                App.ErrorListWindow.AddErrorString(e.Message);
            }
            return null;
        }

        private string ConvertClipsToJSON()
        {
            return JsonConvert.SerializeObject(soundClips, Formatting.Indented);
        }

        private string? RetrieveJSONFromFile()
        {
            try
            {
                return File.ReadAllText(Path.Join(App.APP_STORAGE, @"soundclips.json"));
            }
            catch (Exception e)
            {
                App.ErrorListWindow.AddErrorString(e.Message);
            }
            return null;
        }

        private void SaveJSONToFile(string json) {
            try
            {
                File.WriteAllText(Path.Join(App.APP_STORAGE, @"soundclips.json"), json);
            }
            catch (Exception e)
            {
                App.ErrorListWindow.AddErrorString(e.Message);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
