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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

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

        private string _soundClipListFilter = "";
        public string SoundClipListFilter
        {
            get => _soundClipListFilter;
            set
            {
                if (value != _soundClipListFilter)
                {
                    _soundClipListFilter = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FilteredSoundClipList));
                }
            }
        }

        public List<SoundClip> FilteredSoundClipList
        {
            get
            {
                if (string.IsNullOrEmpty(SoundClipListFilter))
                {
                    return soundClips.Values.OrderBy(c => c.Name).ToList();
                }
                else
                {
                    return soundClips.Values.Where(c => c.Name.ToLowerInvariant().Contains(SoundClipListFilter.ToLowerInvariant())).OrderBy(c => c.Name).ToList();
                }
            }
        }

        private Dictionary<string, SoundClip> soundClips;

        private SoundClipManager()
        {
            Dictionary<string, SoundClip>? retrievedClips = RetrieveSavedSoundClips();

            if (retrievedClips != null)
            {
                soundClips = retrievedClips;

                // Notify user of problems with their linked audio and image files at startup
                ValidateSoundClips(soundClips);
            }
            else
            {
                soundClips = new Dictionary<string, SoundClip>();
            }
        }

        private void ValidateSoundClips(Dictionary<string, SoundClip> soundClips)
        {
            foreach (SoundClip clip in soundClips.Values)
            {
                if (!string.IsNullOrEmpty(clip.AudioFilePath) && !File.Exists(clip.AudioFilePath))
                {
                    App.ErrorListWindow.AddErrorSoundClip(clip, Views.ErrorList.ErrorType.MISSING_AUDIO_FILE);
                }
                if (!string.IsNullOrEmpty(clip.ImageFilePath) && !File.Exists(clip.ImageFilePath))
                {
                    App.ErrorListWindow.AddErrorSoundClip(clip, Views.ErrorList.ErrorType.MISSING_IMAGE_FILE);
                }
            }
        }

        public void AddNewClip(SoundClip clip) 
        {
            string id = DateTimeOffset.Now.ToUnixTimeMilliseconds() + clip.Name;
            clip.Id = id;
            if (string.IsNullOrEmpty(clip.Name))
            {
                clip.Name = clip.AudioFilePath ?? clip.Id;
            }
            soundClips.Add(clip.Id, clip);

            StoreSavedSoundClips();
            OnPropertyChanged(nameof(FilteredSoundClipList));
        }

        public void SaveClip(string id)
        {
            if (soundClips.TryGetValue(id, out SoundClip clip))
            {
                if (string.IsNullOrEmpty(clip.Name))
                {
                    clip.Name = clip.AudioFilePath ?? clip.Id;
                }

                StoreSavedSoundClips();
                OnPropertyChanged(nameof(FilteredSoundClipList));
            }

            App.ErrorListWindow.AddErrorString("SoundClip with ID: " + id + " could not be saved (does not exist)!");
        }

        public SoundClip? GetClip(string id) 
        {
            if (soundClips.TryGetValue(id, out SoundClip clip))
            {
                return clip;
            }
            App.ErrorListWindow.AddErrorString("SoundClip with ID: " + id + " does not exist!");
            return null;
        }

        public Dictionary<string, SoundClip>? RetrieveSavedSoundClips() {
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

        private static Dictionary<string, SoundClip>? ConvertClipsFromJSON(string json)
        {
            try
            {
                return (Dictionary<string, SoundClip>?)JsonConvert.DeserializeObject(json, typeof(Dictionary<string, SoundClip>));
            }
            catch (Exception e)
            {
                App.ErrorListWindow.AddErrorString(e.Message);
            }
            return null;
        }

        private string ConvertClipsToJSON()
        {
            return JsonConvert.SerializeObject(soundClips);
        }

        private string? RetrieveJSONFromFile()
        {
            try
            {
                if (File.Exists(Path.Join(App.APP_STORAGE, @"soundclips.json")))
                {
                    return File.ReadAllText(Path.Join(App.APP_STORAGE, @"soundclips.json"));
                }
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
