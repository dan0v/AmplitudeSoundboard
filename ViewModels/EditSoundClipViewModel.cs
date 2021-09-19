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

using Amplitude.Helpers;
using Amplitude.Models;
using AmplitudeSoundboard;

namespace Amplitude.ViewModels
{
    public class EditSoundClipViewModel : ViewModelBase
    {
        public static ThemeHandler ThemeHandler { get => App.ThemeHandler; }
        private SoundClip _model;
        public SoundClip Model { get => _model; }

        public EditSoundClipViewModel()
        {
            _model = new SoundClip();
            SetBindings();
        }

        /// <summary>
        ///  Edit a (copy of an) existing SoundClip in this EditSoundClip window. Save to overwrite original with copy
        /// </summary>
        /// <param name="model"></param>
        public EditSoundClipViewModel(SoundClip model)
        {
            _model = model.ShallowCopy();
            SetBindings();
        }

        private void SetBindings()
        {
            Model.PropertyChanged += Model_PropertyChanged;
            SaveButtonTooltip = HasNameField ? "" : Localization.Localizer.Instance["SaveButtonDisabledTooltip"];
        }

        /// <summary>
        /// Update ViewModel properties when underlying model changes detected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.Name))
            {
                HasNameField = !string.IsNullOrEmpty(Model.Name);
                SaveButtonTooltip = HasNameField ? "" : Localization.Localizer.Instance["SaveButtonDisabledTooltip"];
            }
        }

        private bool _hasNameField;
        public bool HasNameField
        {
            get => _hasNameField;
            set
            {
                if (value != _hasNameField)
                {
                    _hasNameField = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _saveButtonTooltip = "";
        public string SaveButtonTooltip
        {
            get => _saveButtonTooltip;
            set
            {
                if (value != _saveButtonTooltip)
                {
                    _saveButtonTooltip = value;
                    OnPropertyChanged();
                }
            }
        }

        public void PlaySound()
        {
            Model.PlayAudio();
        }

        public void StopAudio()
        {
            App.SoundEngine.Reset();
        }

        public void SetClipAudioFilePath(string[] url)
        {
            if (url.Length > 0)
            {
                Model.AudioFilePath = url[0];
            }
        }

        public void SetClipImageFilePath(string[] url)
        {
            if (url.Length > 0)
            {
                Model.ImageFilePath = url[0];
            }
        }

        public void IncreaseVolumeSmall()
        {
            if (Model.Volume < 100)
            {
                Model.Volume += 1f;
            }
        }
        public void DecreaseVolumeSmall()
        {
            if (Model.Volume > 0)
            {
                Model.Volume -= 1f;
            }
        }

        public void SaveClip()
        {
            App.SoundClipManager.SaveClip(Model);
            _model = Model.ShallowCopy();
            OnPropertyChanged(nameof(Model));
        }

        public void CreateHotkey()
        {

        }
    }
}