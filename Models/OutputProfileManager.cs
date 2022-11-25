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
using AmplitudeSoundboard;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Amplitude.Models
{
    public class OutputProfileManager : JSONIOManager, INotifyPropertyChanged
    {
        private static OutputProfileManager? _instance;
        public static OutputProfileManager Instance { get => _instance ??= new OutputProfileManager(); }

        private const string MIGRATED_PROFILES_NAME_BASE = "Migrated_Profile";
        private int migratedProfileCounter = 1;
        
        private const string OUTPUTPROFILES_FILE = "profiles.json";
        public const string DEFAULT_OUTPUTPROFILE = "DEFAULT";

        private Dictionary<string, OutputProfile> _outputProfiles;
        public Dictionary<string, OutputProfile> OutputProfiles { get => _outputProfiles; }

        public List<OutputProfile> OutputProfilesList { get => OutputProfiles.Values.ToList(); }

        private OutputProfileManager()
        {
            Dictionary<string, OutputProfile>? retrievedOutputProfiles = RetrieveSavedOutputProfiles();

            if (retrievedOutputProfiles != null)
            {
                _outputProfiles = retrievedOutputProfiles;
            }
            else
            {
                _outputProfiles = new Dictionary<string, OutputProfile>();
            }

            if (!_outputProfiles.ContainsKey(DEFAULT_OUTPUTPROFILE))
            {
                var profile = new OutputProfile();
                profile.InitializeId(DEFAULT_OUTPUTPROFILE);
                profile.Name = "Default";
                profile.OutputSettings.Add(new OutputSettings());
                AddOutputProfile(profile);
            }
        }

        public string FindOrCreateIdOfSimilarOutputProfile(Collection<OutputSettings> settings)
        {
            foreach (var profile in OutputProfiles)
            {
                var profileDevs = profile.Value.OutputSettings.Select(s => s.DeviceName).ToList();
                var settingsDevs = settings.Select(s => s.DeviceName == ISoundEngine.GLOBAL_DEFAULT_DEVICE_NAME ? ISoundEngine.DEFAULT_DEVICE_NAME : s.DeviceName);
                if (settingsDevs.All(s => profileDevs.Contains(s)))
                {
                    return profile.Key;
                }
            }
            var newProf = new OutputProfile(settings);
            newProf.Name = MIGRATED_PROFILES_NAME_BASE + "_" + migratedProfileCounter++;
            newProf.InitializeId(null);
            AddOutputProfile(newProf);
            OnPropertyChanged(nameof(OutputProfilesList));
            return newProf.Id;
        }

        public void AddOutputProfile(OutputProfile profile)
        {
            OutputProfiles.Add(profile.Id, profile);
            StoreSavedOutputProfiles();
        }

        /// <summary>
        /// Save a new OutputProfile, or overwrite an existing OutputProfile
        /// </summary>
        /// <param name="clip"></param>
        public void SaveOutputProfile(OutputProfile profile)
        {
            if (string.IsNullOrEmpty(profile.Id))
            {
                profile.InitializeId(null);
            }

            if (string.IsNullOrEmpty(profile.Name))
            {
                profile.Name = profile.Id;
            }

            ValidateOutputProfile(profile);

            if (OutputProfiles.ContainsKey(profile.Id))
            {
                OutputProfiles[profile.Id] = profile;
                StoreSavedOutputProfiles();
            }
            else
            {
                AddOutputProfile(profile);
            }

            OnPropertyChanged(nameof(OutputProfiles));
            OnPropertyChanged(nameof(OutputProfilesList));
        }

        public void RemoveOutputProfile(string Id)
        {
            OutputProfiles.Remove(Id);
            OnPropertyChanged(nameof(OutputProfiles));
            OnPropertyChanged(nameof(OutputProfilesList));
            StoreSavedOutputProfiles();
        }

        private void StoreSavedOutputProfiles()
        {
            string profilesInJson = ConvertOutputProfilesToJSON();

            SaveJSONToFile(OUTPUTPROFILES_FILE, profilesInJson);
        }

        private string ConvertOutputProfilesToJSON()
        {
            return JsonConvert.SerializeObject(OutputProfiles, Formatting.Indented);
        }

        public void ValidateOutputProfile(OutputProfile? profile)
        {
            foreach (OutputSettings settings in profile?.OutputSettings ?? new ObservableCollection<OutputSettings>())
            {
                if (string.IsNullOrEmpty(settings.DeviceName) || settings.DeviceName == "DEFAULT" || settings.DeviceName == ISoundEngine.GLOBAL_DEFAULT_DEVICE_NAME)
                {
                    settings.DeviceName = ISoundEngine.DEFAULT_DEVICE_NAME;
                }
            }
        }

        public OutputProfile? GetOutputProfile(string? id, bool ignoreErrors = false)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (OutputProfiles.TryGetValue(id, out OutputProfile? profile))
            {
                return profile;
            }
            if (!ignoreErrors)
            {
                App.WindowManager.ShowErrorString("OutputProfile with ID: " + id + " does not exist!");
            }
            return null;
        }

        private static Dictionary<string, OutputProfile>? RetrieveSavedOutputProfiles()
        {
            string? clipsInJson = RetrieveJSONFromFile(OUTPUTPROFILES_FILE);
            var profiles = ConvertObjectsFromJSON<OutputProfile>(clipsInJson);
            foreach (var profile in profiles ?? new Dictionary<string, OutputProfile>())
            {
                profile.Value.InitializeId(profile.Key);
            }
            return profiles;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
