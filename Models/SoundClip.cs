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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using AmplitudeSoundboard;
using Amplitude.Helpers;
using Avalonia.Media.Imaging;

namespace Amplitude.Models
{
    public class SoundClip : INotifyPropertyChanged
    {
        public void InitializeId(string newId)
        {
            if (string.IsNullOrEmpty(Id))
            {
                _id = newId;
                OnPropertyChanged(nameof(Id));
            }
            else
            {
                throw new NotSupportedException("Do not alter Id once it has been set");
            }
        }

        private int _volume = 100;
        public int Volume {
            get => _volume;
            set
            {
                if (value != _volume)
                {
                    _volume = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _hotkey = "";
        public string Hotkey
        {
            get => _hotkey;
            set
            {
                if (value != _hotkey)
                {
                    _hotkey = value;
                }
                OnPropertyChanged(); // Alert even if not changed
                OnPropertyChanged(nameof(PlayAudioTooltip));
            }
        }

        private string _audioFilePath = "";
        public string AudioFilePath
        {
            get => _audioFilePath;
            set
            {
                if (value != _audioFilePath)
                {
                    _audioFilePath = value;
                    // Clear possibly cached clip
                    App.SoundEngine.ClearSoundClipCache(Id);
                    OnPropertyChanged();
                }
            }
        }

        private string _imageFilePath = "";
        public string ImageFilePath
        {
            get => _imageFilePath;
            set
            {
                if (value != _imageFilePath)
                {
                    _imageFilePath = value;
                    OnPropertyChanged();

                    if (BrowseIO.ValidImage(_imageFilePath, false))
                    {
                        _backgroundImage = new Bitmap(_imageFilePath);
                    }
                    else
                    {
                        _backgroundImage = null;
                    }
                    OnPropertyChanged(nameof(BackgroundImage));
                }
            }
        }

        private bool _preCache = false;
        public bool PreCache
        {
            get => _preCache;
            set
            {
                if (value != _preCache)
                {
                    _preCache = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _deviceName = ISoundEngine.DEFAULT_DEVICE_NAME;
        public string DeviceName
        {
            get => _deviceName;
            set
            {
                if (value != _deviceName)
                {
                    _deviceName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _id = null;
        // Do not write to JSON, since it is stored in dictionary anyway
        [JsonIgnore]
        public string Id
        {
            get => _id;
        }

        private Bitmap _backgroundImage = null;
        [JsonIgnore]
        public Bitmap BackgroundImage
        {
            get => _backgroundImage;
        }

        [JsonIgnore]
        public string? PlayAudioTooltip { get => string.IsNullOrEmpty(Id) ? null : string.IsNullOrEmpty(Hotkey) ? Localization.Localizer.Instance["PlaySound"] : Localization.Localizer.Instance["PlaySound"] + ": " + Hotkey; }

        public SoundClip() { }

        public void PlayAudio()
        {
            App.SoundEngine.Play(this);
        }

        public void CopySoundClipId()
        {
            App.SoundClipManager.CopiedClipId = Id;
        }

        public void OpenEditSoundClipWindow()
        {
            App.WindowManager.OpenEditSoundClipWindow(Id);
        }

        public static SoundClip? FromJSON(string json)
        {
            try
            {
                return (SoundClip?)JsonConvert.DeserializeObject(json, typeof(SoundClip));
            } catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            return null;
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public SoundClip ShallowCopy()
        {
            return (SoundClip)this.MemberwiseClone();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
