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

using Amplitude.Models;
using AmplitudeSoundboard;
using Avalonia.Media;
using System.Collections.Generic;

namespace Amplitude.ViewModels
{
    public class EditSoundClipViewModel : ViewModelBase
    {
        static ThemeHandler ThemeHandler { get => App.ThemeHandler; }
        private OptionsManager OptionsManager { get => App.OptionsManager; }

        private string StopAudioHotkey => string.IsNullOrEmpty(OptionsManager.Options.GlobalKillAudioHotkey) ? Localization.Localizer.Instance["StopAllAudio"] : Localization.Localizer.Instance["StopAllAudio"] + ": " + OptionsManager.Options.GlobalKillAudioHotkey;

        private SoundClip _model;
        public SoundClip Model { get => _model; }

        public bool CanSave { get => HasNameField && !WaitingForHotkey; }

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
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        private bool _waitingForHotkey;
        public bool WaitingForHotkey
        {
            get => _waitingForHotkey;
            set
            {
                if (value != _waitingForHotkey)
                {
                    _waitingForHotkey = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanSave));
                    OnPropertyChanged(nameof(HotkeyBackgroundColor));
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

        private bool _canRemoveOutputDevices = false;
        public bool CanRemoveOutputDevices
        {
            get => _canRemoveOutputDevices;
            set
            {
                if (value != _canRemoveOutputDevices)
                {
                    _canRemoveOutputDevices = value;
                    OnPropertyChanged();
                }
            }
        }

        public Color HotkeyBackgroundColor => WaitingForHotkey ? ThemeHandler.TextBoxHighlightedColor : ThemeHandler.TextBoxNormalColor;

        public List<string> DeviceList => App.SoundEngine.OutputDeviceList;

        public EditSoundClipViewModel()
        {
            _model = new SoundClip();
            Model.OutputSettings.Add(new OutputSettings());
            CanRemoveOutputDevices = Model.OutputSettings.Count > 1;
            SetBindings();
        }

        /// <summary>
        ///  Edit a (copy of an) existing SoundClip in this EditSoundClip window. Save to overwrite original with copy
        /// </summary>
        /// <param name="model"></param>
        public EditSoundClipViewModel(SoundClip model)
        {
            _model = model.CreateCopy();
            CanRemoveOutputDevices = Model.OutputSettings.Count > 1;
            SetBindings();
        }

        private void SetBindings()
        {
            HasNameField = !string.IsNullOrEmpty(Model.Name);
            HasAudioFilePath = !string.IsNullOrEmpty(Model.AudioFilePath);
            SaveButtonTooltip = HasNameField ? "" : Localization.Localizer.Instance["SaveButtonDisabledTooltip"];
            Model.PropertyChanged += Model_PropertyChanged;
            Model.OutputSettings.CollectionChanged += OutputSettings_CollectionChanged;
            OptionsManager.PropertyChanged += OptionsManager_PropertyChanged;
        }

        private void OutputSettings_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CanRemoveOutputDevices = Model.OutputSettings.Count > 1;
        }

        private void OptionsManager_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OptionsManager.Options))
            {
                OnPropertyChanged(nameof(StopAudioHotkey));
            }
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
            if (e.PropertyName == nameof(Model.Hotkey))
            {
                WaitingForHotkey = false;
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

        public void SaveClip()
        {
            SoundClip toSave = Model.CreateCopy();
            App.SoundClipManager.SaveClip(toSave);
            // Copy back and forth to delay ID update until fully saved
            _model = toSave.CreateCopy();
            OnPropertyChanged(nameof(Model));
        }

        public void RemoveOutputDevice()
        {
            if (Model.OutputSettings.Count > 0)
            {
                Model.OutputSettings.RemoveAt(Model.OutputSettings.Count - 1);
            }
        }

        public void AddOutputDevice()
        {
            Model.OutputSettings.Add(new OutputSettings());
        }

        public void RecordHotkey()
        {
            WaitingForHotkey = true;
            App.HotkeysManager.RecordSoundClipHotkey(Model);
        }

        public override void Dispose()
        {
            Model.PropertyChanged -= Model_PropertyChanged;
            OptionsManager.PropertyChanged -= OptionsManager_PropertyChanged;
            base.Dispose();
        }
    }
}