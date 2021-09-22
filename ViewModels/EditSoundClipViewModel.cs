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
using ReactiveUI;
using System.Reactive;

namespace Amplitude.ViewModels
{
    public class EditSoundClipViewModel : ViewModelBase
    {
        static ThemeHandler ThemeHandler { get => App.ThemeHandler; }
        private SoundClip _model;
        public SoundClip Model { get => _model; }

        public EditSoundClipViewModel()
        {
            _model = new SoundClip();
            _model.EditWindowOpen = true;
            SetBindings();
        }

        /// <summary>
        ///  Edit a (copy of an) existing SoundClip in this EditSoundClip window. Save to overwrite original with copy
        /// </summary>
        /// <param name="model"></param>
        public EditSoundClipViewModel(SoundClip model)
        {
            App.SoundClipManager.OpenedEditWindow(model.Id);
            _model = model.ShallowCopy();
            SetBindings();
        }

        private void SetBindings()
        {
            HasNameField = !string.IsNullOrEmpty(Model.Name);
            HasAudioFilePath = !string.IsNullOrEmpty(Model.AudioFilePath);
            SaveButtonTooltip = HasNameField ? "" : Localization.Localizer.Instance["SaveButtonDisabledTooltip"];
            Model.PropertyChanged += Model_PropertyChanged;
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
            if (e.PropertyName == nameof(Model.AudioFilePath))
            {
                HasAudioFilePath = !string.IsNullOrEmpty(Model.AudioFilePath);
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

        private bool _hasAudioFilePath;
        public bool HasAudioFilePath
        {
            get => _hasAudioFilePath;
            set
            {
                if (value != _hasAudioFilePath)
                {
                    _hasAudioFilePath = value;
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
            App.SoundEngine.Reset(true);
        }

        public void SetClipAudioFilePath(string[] url)
        {
            if (url.Length > 0)
            {
                Model.AudioFilePath = url[0];
            }
        }

        public void DeleteSoundClip()
        {
            App.SoundClipManager.RemoveSoundClip(Model.Id);
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
                Model.Volume += 1;
            }
        }
        public void DecreaseVolumeSmall()
        {
            if (Model.Volume > 0)
            {
                Model.Volume -= 1;
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