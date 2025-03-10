/*
    AmplitudeSoundboard
    Copyright (C) 2021-2025 dan0v
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
using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.Generic;

namespace Amplitude.ViewModels
{
    public sealed class EditSoundClipViewModel : ViewModelBase
    {
        public string StopAudioHotkey => string.IsNullOrEmpty(ConfigManager.Config.GlobalKillAudioHotkey) ? Localization.Localizer.Instance["StopAllAudio"] : Localization.Localizer.Instance["StopAllAudio"] + ": " + ConfigManager.Config.GlobalKillAudioHotkey;

        private SoundClip _model;
        public SoundClip Model => _model;

        private (int row, int col)? addToGridCell = null;

        public bool CanSave => HasNameField && !WaitingForHotkey;

        public List<OutputProfile> OutputProfilesList => OutputProfileManager.OutputProfilesList;

        public OutputProfile? CurrentOutputProfile => OutputProfileManager.GetOutputProfile(Model.OutputProfileId);

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

        public Color HotkeyBackgroundColor => WaitingForHotkey ? ThemeManager.Theme.TextBoxHighlightedColor : ThemeManager.Theme.TextBoxNormalColor;

        public EditSoundClipViewModel()
        {
            _model = new SoundClip();
            CanRemoveOutputDevices = Model.OutputSettingsFromProfile.Count > 1;
            SetBindings();
        }

        public EditSoundClipViewModel((int row, int col) addToGrid) : this()
        {
            addToGridCell = addToGrid;
        }

        /// <summary>
        ///  Edit a (copy of an) existing SoundClip in this EditSoundClip window. Save to overwrite original with copy
        /// </summary>
        /// <param name="model"></param>
        public EditSoundClipViewModel(SoundClip model)
        {
            _model = model.ShallowCopy();
            CanRemoveOutputDevices = Model.OutputSettingsFromProfile.Count > 1;
            SetBindings();
        }

        private void SetBindings()
        {
            HasNameField = !string.IsNullOrEmpty(Model.Name);
            HasAudioFilePath = !string.IsNullOrEmpty(Model.AudioFilePath);
            SaveButtonTooltip = HasNameField ? "" : Localization.Localizer.Instance["SaveButtonDisabledTooltip"];
            OutputProfileManager.PropertyChanged += OutputProfileManager_PropertyChanged;
            Model.PropertyChanged += Model_PropertyChanged;
            ConfigManager.PropertyChanged += ConfigManager_PropertyChanged;
        }

        private void OutputProfileManager_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OutputProfileManager.OutputProfilesList))
            {
                OnPropertyChanged(nameof(OutputProfilesList));
                OnPropertyChanged(nameof(CurrentOutputProfile));
            }
        }

        private void ConfigManager_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ConfigManager.Config))
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

        public void OutputProfileSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var id = ((OutputProfile?)e.AddedItems[0])?.Id;
                
                if (id != null && id != Model.OutputProfileId)
                {
                    Model.OutputProfileId = id;
                }
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

        public void NewOutputProfile()
        {
            WindowManager.OpenEditOutputProfileWindow(null);
            OnPropertyChanged(nameof(OutputProfilesList));

        }

        public void EditOutputProfile()
        {
            WindowManager.OpenEditOutputProfileWindow(Model.OutputProfileId);
        }

        public void PlaySound()
        {
            Model.PlayAudio();
        }

        public void StopAudio()
        {
            App.SoundEngine.Reset();
        }

        public void SetClipAudioFilePath(string[]? url)
        {
            if (url?.Length > 0)
            {
                Model.AudioFilePath = url[0];
            }
        }

        public void DeleteSoundClip()
        {
            App.SoundClipManager.RemoveSoundClip(Model.Id);
        }

        public void SetClipImageFilePath(string[]? url)
        {
            if (url?.Length > 0)
            {
                Model.ImageFilePath = url[0];
            }
        }

        public void SaveClip()
        {
            SoundClip toSave = Model.ShallowCopy();
            App.SoundClipManager.SaveClip(toSave);
            // Copy back and forth to delay ID update until fully saved
            _model = toSave.ShallowCopy();
            OnPropertyChanged(nameof(Model));

            if (addToGridCell != null && addToGridCell.Value.row >= 0 && addToGridCell.Value.col >= 0 &&
                addToGridCell.Value.row < App.ConfigManager.Config.GridRows &&
                addToGridCell.Value.col < App.ConfigManager.Config.GridColumns)
            {
                ConfigManager.Config.GridSoundClipIds[addToGridCell.Value.row][addToGridCell.Value.col] = Model.Id;
                ConfigManager.SaveAndOverwriteConfig(ConfigManager.Config);
            }
        }

        public void RemoveOutputDevice()
        {
            if (Model.OutputSettingsFromProfile.Count > 0)
            {
                Model.OutputSettingsFromProfile.RemoveAt(Model.OutputSettingsFromProfile.Count - 1);
            }
        }

        public void AddOutputDevice()
        {
            Model.OutputSettingsFromProfile.Add(new OutputSettings());
        }

        public void RecordHotkey()
        {
            Model.Hotkey = Localization.Localizer.Instance["HotkeyCancelPlaceholder"];
            WaitingForHotkey = true;
            App.HotkeysManager.RecordSoundClipHotkey(Model);
        }

        public override void Dispose()
        {
            Model.PropertyChanged -= Model_PropertyChanged;
            ConfigManager.PropertyChanged -= ConfigManager_PropertyChanged;
            OutputProfileManager.PropertyChanged -= OutputProfileManager_PropertyChanged;
            base.Dispose();
        }
    }
}