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
using AmplitudeSoundboard;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
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

        private int _volume = 100;
        public int Volume
        {
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

        private bool _loopClip = false;
        public bool LoopClip
        {
            get => _loopClip;
            set
            {
                if (value != _loopClip)
                {
                    _loopClip = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public ObservableCollection<OutputSettings> OutputSettingsFromProfile => App.OutputProfileManager.GetOutputProfile(OutputProfileId)?.OutputSettings;

        private ObservableCollection<OutputSettings> _outputSettings = new ObservableCollection<OutputSettings>();
        [Obsolete]
        public ObservableCollection<OutputSettings> OutputSettings
        {
            get => _outputSettings;
            set
            {
                if (value != _outputSettings)
                {
                    _outputSettings = value;
                }
            }
        }

        private string _outputProfileId = OutputProfileManager.DEFAULT_OUTPUTPROFILE;
        public string OutputProfileId
        {
            get => _outputProfileId;
            set
            {
                if (value != _outputProfileId)
                {
                    _outputProfileId = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(OutputSettingsFromProfile));
                }
            }
        }

        private string _id = null;
        // Do not write to JSON, since it is stored in dictionary anyway
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string Id => _id;

        private Bitmap? _backgroundImage = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Bitmap? BackgroundImage => _backgroundImage;

        private bool _loadBackgroundImage = false;
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
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

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string? PlayAudioTooltip => string.IsNullOrEmpty(Id) ? null : string.IsNullOrEmpty(Hotkey) ? Localization.Localizer.Instance["PlaySound"] : Localization.Localizer.Instance["PlaySound"] + ": " + Hotkey;

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

        public async Task SetAndRescaleBackgroundImage()
        {
            if (LoadBackgroundImage && BrowseIO.ValidImage(_imageFilePath, false))
            {
                double intendedHeight = App.ConfigManager.Config.DesiredImageHeight;
                double intendedWidth = App.ConfigManager.Config.DesiredImageWidth;

                if (_backgroundImage != null)
                {
                    double currentWidth = _backgroundImage.PixelSize.Width;
                    double currentHeight = _backgroundImage.PixelSize.Height;

                    if (intendedHeight == currentHeight || intendedWidth == currentWidth)
                    {
                        return;
                    }
                }

                var newImage = new Bitmap(_imageFilePath);
                double imageWidth = newImage.PixelSize.Width;
                double imageHeight = newImage.PixelSize.Height;

                double scaleFactor = intendedWidth > intendedHeight ? imageWidth / intendedWidth : imageHeight / intendedHeight;
                scaleFactor /= App.WindowManager.DesktopScaling;
                try
                {
                    _backgroundImage = newImage.CreateScaledBitmap(new PixelSize((int)(imageWidth / scaleFactor), (int)(imageHeight / scaleFactor)), BitmapInterpolationMode.HighQuality);
                    newImage.Dispose();
                }
                catch (Exception e)
                {
                    _backgroundImage = null;
                    App.WindowManager.ShowErrorString(e.Message);
                }
            }
            else
            {
                _backgroundImage = null;
            }

            if (!Dispatcher.UIThread.CheckAccess())
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    OnPropertyChanged(nameof(BackgroundImage));
                });
                return;
            }

            OnPropertyChanged(nameof(BackgroundImage));
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
