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
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
                    SetAndRescaleBackgroundImage().Wait();
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

        private bool _nameVisibleOnGridTile = true;
        public bool NameVisibleOnGridTile
        {
            get => _nameVisibleOnGridTile;
            set
            {
                if (value != _nameVisibleOnGridTile)
                {
                    _nameVisibleOnGridTile = value;
                    OnPropertyChanged();
                }
            }
        }

        // legacy soundclips loading
        private string _deviceName = ISoundEngine.DEFAULT_DEVICE_NAME;
        [Obsolete]
        public string DeviceName
        {
            internal get => _deviceName;
            set
            {
                if (OutputSettings.Count <= 0)
                {
                    OutputSettings.Add(new OutputSettings());
                }
                OutputSettings[0].DeviceName = value;
            }
        }

        // legacy soundclips loading
        private int _volume = 100;
        [Obsolete]
        public int Volume
        {
            internal get => _volume;
            set
            {
                if (OutputSettings.Count <= 0)
                {
                    OutputSettings.Add(new OutputSettings());
                }
                OutputSettings[0].Volume = value;
            }
        }

        private ObservableCollection<OutputSettings> _outputSettings = new ObservableCollection<OutputSettings>();
        public ObservableCollection<OutputSettings> OutputSettings
        {
            get => _outputSettings;
            set
            {
                if (value != _outputSettings)
                {
                    _outputSettings = value;
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

        private bool _loadBackgroundImage = false;
        [JsonIgnore]
        public bool LoadBackgroundImage
        {
            get => _loadBackgroundImage;
            set
            {
                if (value != _loadBackgroundImage)
                {
                    _loadBackgroundImage = value;
                    OnPropertyChanged();
                    SetAndRescaleBackgroundImage().Wait();
                }
            }
        }


        [JsonIgnore]
        public string? PlayAudioTooltip { get => string.IsNullOrEmpty(Id) ? null : string.IsNullOrEmpty(Hotkey) ? Localization.Localizer.Instance["PlaySound"] : Localization.Localizer.Instance["PlaySound"] + ": " + Hotkey; }

        public SoundClip() { }

        public void PlayAudio()
        {
            App.SoundEngine.Play(this);
        }

        public void AddAudioToQueue()
        {
            App.SoundEngine.AddToQueue(this);
        }

        public void CopySoundClipId()
        {
            App.SoundClipManager.CopiedClipId = Id;
        }

        public void OpenEditSoundClipWindow()
        {
            App.WindowManager.OpenEditSoundClipWindow(Id);
        }

        public async Task SetAndRescaleBackgroundImage(bool fromBackgroundThread = false)
        {
            if (LoadBackgroundImage && BrowseIO.ValidImage(_imageFilePath, false))
            {
                _backgroundImage = new Bitmap(_imageFilePath);
                double initialWidth = _backgroundImage.PixelSize.Width;
                double initialHeight = _backgroundImage.PixelSize.Height;
                double intendedHeight = App.OptionsManager.Options.DesiredImageHeight;
                double intendedWidth = App.OptionsManager.Options.DesiredImageWidth;
                double scaleFactor = intendedWidth > intendedHeight ? initialWidth / intendedWidth : initialHeight / intendedHeight;
                scaleFactor /= App.WindowManager.DesktopScaling;
                try
                {
                    _backgroundImage = _backgroundImage.CreateScaledBitmap(new PixelSize((int)(initialWidth / scaleFactor), (int)(initialHeight / scaleFactor)), Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.HighQuality);
                }
                catch (Exception e)
                {
                    App.WindowManager.ShowErrorString(e.ToString());
                }
            }
            else
            {
                _backgroundImage = null;
            }

            void triggerOnChanged()
            {
                if (fromBackgroundThread)
                {
                    OnPropertyChanged(nameof(BackgroundImage));
                }
                else
                {
                    OnPropertyChanged(nameof(BackgroundImage));
                }
            }

            if (!Dispatcher.UIThread.CheckAccess())
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    triggerOnChanged();
                });
                return;
            }

            triggerOnChanged();
        }

        public static SoundClip? FromJSON(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<SoundClip>(json);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            return null;
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public SoundClip CreateCopy()
        {
            var copy = (SoundClip)this.MemberwiseClone();
            copy.OutputSettings = new ObservableCollection<OutputSettings>();

            foreach (var setting in OutputSettings)
            {
                copy.OutputSettings.Add(setting.ShallowCopy());
            }

            return copy;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
