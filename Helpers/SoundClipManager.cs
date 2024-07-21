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

using Amplitude.Models;
using AmplitudeSoundboard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Amplitude.Helpers
{
    [JsonSerializable(typeof(Dictionary<string, SoundClip>))]
    public partial class SoundClipManagerContext : JsonSerializerContext { }

    public class SoundClipManager : INotifyPropertyChanged
    {
        private static SoundClipManager? _instance;
        public static SoundClipManager Instance => _instance ??= new SoundClipManager();

        private const string SOUNDCLIPS_FILE = "soundclips.json";

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
                if (string.IsNullOrEmpty(SoundClipListFilter.Trim()))
                {
                    return _soundClips.Values.OrderBy(c => c.Name).ToList();
                }
                else
                {
                    return _soundClips.Values.Where(c => c.Name.ToLowerInvariant().Contains(SoundClipListFilter.Trim().ToLowerInvariant())).OrderBy(c => c.Name).ToList();
                }
            }
        }

        private string _copiedClipId = "";
        public string CopiedClipId
        {
            get => _copiedClipId;
            set
            {
                if (value != _copiedClipId)
                {
                    _copiedClipId = value;
                    OnPropertyChanged();
                }
            }
        }

        private const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private Dictionary<string, SoundClip> _soundClips;
        public Dictionary<string, SoundClip> SoundClips => _soundClips;

        public async void RescaleAllBackgroundImages()
        {
            var rescaleTasks = SoundClips.Values.Select(clip => clip.SetAndRescaleBackgroundImage()).ToArray();
            await Task.WhenAll(rescaleTasks);
        }

        private SoundClipManager()
        {
            Dictionary<string, SoundClip>? retrievedClips = RetrieveSavedSoundClips();

            if (retrievedClips != null)
            {
                _soundClips = retrievedClips;
                InitializeSoundClips(SoundClips);
                StoreSavedSoundClips();
            }
            else
            {
                _soundClips = new Dictionary<string, SoundClip>();
            }
        }

        private void InitializeSoundClips(Dictionary<string, SoundClip> soundClips)
        {
            foreach (KeyValuePair<string, SoundClip> item in soundClips)
            {
                item.Value.InitializeId(item.Key);

                if (item.Value.OutputSettings.Any())
                {
                    item.Value.OutputProfileId = App.OutputProfileManager.FindOrCreateIdOfSimilarOutputProfile(item.Value.OutputSettings);
                    item.Value.OutputSettings = new ObservableCollection<OutputSettings>();
                }

                ValidateSoundClip(item.Value);
                RegisterSoundClipHotkey(item.Value);
            }
        }

        private void RegisterSoundClipHotkey(SoundClip value)
        {
            App.HotkeysManager.RegisterHotkeyAtStartup(value.Id, value.Hotkey);
        }

        /// <summary>
        /// Notify user of problems with their linked audio and image files at startup
        /// </summary>
        /// <param name="soundClips"></param>
        private void ValidateSoundClip(SoundClip clip)
        {
            if (!string.IsNullOrEmpty(clip.AudioFilePath) && !File.Exists(clip.AudioFilePath))
            {
                App.WindowManager.ShowErrorSoundClip(clip, ViewModels.ErrorListViewModel.SoundClipErrorType.MISSING_AUDIO_FILE);
            }
            if (!string.IsNullOrEmpty(clip.ImageFilePath) && !File.Exists(clip.ImageFilePath))
            {
                App.WindowManager.ShowErrorSoundClip(clip, ViewModels.ErrorListViewModel.SoundClipErrorType.MISSING_IMAGE_FILE);
            }

            var profile = App.OutputProfileManager.GetOutputProfile(clip.OutputProfileId);
            if (profile == null)
            {
                clip.OutputProfileId = OutputProfileManager.DEFAULT_OUTPUTPROFILE;
                profile = App.OutputProfileManager.GetOutputProfile(clip.OutputProfileId);
            }

            App.OutputProfileManager.ValidateOutputProfile(profile);
            App.SoundEngine.CheckDeviceExistsAndGenerateErrors(profile);
        }

        /// <summary>
        /// Save a new SoundClip and generate an ID, or overwrite an existing SoundClip
        /// </summary>
        /// <param name="clip"></param>
        public void SaveClip(SoundClip clip)
        {
            if (string.IsNullOrEmpty(clip.Name))
            {
                clip.Name = clip.AudioFilePath ?? clip.Id;
            }

            if (string.IsNullOrEmpty(clip.Id))
            {
                if (GenerateAndSetId(clip))
                {
                    SoundClips.Add(clip.Id, clip);
                    if (!string.IsNullOrEmpty(clip.Hotkey))
                    {
                        App.HotkeysManager.RegisterHotkeyAtStartup(clip.Id, clip.Hotkey);
                    }
                }
            }
            else if (SoundClips.TryGetValue(clip.Id, out SoundClip oldClip))
            {
                // Overwrite existing clip
                App.HotkeysManager.RemoveHotkey(clip.Id, oldClip.Hotkey);
                if (!string.IsNullOrEmpty(clip.Hotkey))
                {
                    App.HotkeysManager.RegisterHotkeyAtStartup(clip.Id, clip.Hotkey);
                }
                SoundClips[clip.Id] = clip;
            }
            else
            {
                App.WindowManager.ShowErrorString("SoundClip with ID: " + clip.Id + " could not be saved (does not exist)!");
            }
            ValidateSoundClip(clip);
            StoreSavedSoundClips();
            OnPropertyChanged(nameof(FilteredSoundClipList));
            OnPropertyChanged(nameof(SoundClips));
        }

        public void RemoveSoundClip(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            if (SoundClips.TryGetValue(id, out SoundClip clip))
            {
                App.HotkeysManager.RemoveHotkey(id, clip.Hotkey);

                SoundClips.Remove(id);

                StoreSavedSoundClips();
                OnPropertyChanged(nameof(FilteredSoundClipList));
                OnPropertyChanged(nameof(SoundClips));
            }
        }

        private bool GenerateAndSetId(SoundClip clip)
        {
            // New clip
            int hashCode = clip.GetHashCode();
            string id = DateTimeOffset.Now.ToUnixTimeMilliseconds() + hashCode + "";
            int attempt = 0;
            while (SoundClips.ContainsKey(id))
            {
                string suf = "";
                if (attempt >= alphabet.Length)
                {
                    if (attempt / alphabet.Length >= alphabet.Length)
                    {
                        // Something has gone wrong, there has been easily enough time to find an Id
                        App.WindowManager.ShowErrorString("A new Sound Clip could not be saved (could not generate Id, please try again later)!");
                        return false;
                    }
                    suf += alphabet[attempt / alphabet.Length] + alphabet[attempt % alphabet.Length];
                }
                else
                {
                    suf = alphabet[attempt] + "";
                }
                id = DateTimeOffset.Now.ToUnixTimeMilliseconds() + hashCode + suf;
                attempt++;
            }
            clip.InitializeId(id);
            return true;
        }

        public SoundClip? GetClip(string? id, bool ignoreErrors = false)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (SoundClips.TryGetValue(id, out SoundClip? clip))
            {
                return clip;
            }
            if (!ignoreErrors)
            {
                App.WindowManager.ShowErrorString("SoundClip with ID: " + id + " does not exist!");
            }
            return null;
        }

        private static Dictionary<string, SoundClip>? RetrieveSavedSoundClips()
        {
            var clipsInJson = App.JsonIoManager.RetrieveJSONFromFile(SOUNDCLIPS_FILE);
            return App.JsonIoManager.ConvertObjectsFromJSON<Dictionary<string, SoundClip>>(clipsInJson);
        }

        private void StoreSavedSoundClips()
        {
            var clipsInJson = App.JsonIoManager.ConvertObjectsToJSON(SoundClips);
            App.JsonIoManager.SaveJSONToFile(SOUNDCLIPS_FILE, clipsInJson);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
