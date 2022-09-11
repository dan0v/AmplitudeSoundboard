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

using Amplitude.Models;
using AmplitudeSoundboard;
using Avalonia.Media;
using System.Collections.Generic;

namespace Amplitude.ViewModels
{
    public sealed class EditOutputProfileViewModel : ViewModelBase
    {
        private OutputProfile _model;
        public OutputProfile Model => _model;

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

        public EditOutputProfileViewModel()
        {
            _model = new OutputProfile();
            CanRemoveOutputDevices = Model.OutputSettings.Count > 1;
            SetBindings();
        }

        /// <summary>
        ///  Edit a (copy of an) existing SoundClip in this EditSoundClip window. Save to overwrite original with copy
        /// </summary>
        /// <param name="model"></param>
        public EditOutputProfileViewModel(OutputProfile model)
        {
            _model = model.ShallowCopy();
            CanRemoveOutputDevices = Model.OutputSettings.Count > 1;
            SetBindings();
        }

        private void SetBindings()
        {
            HasNameField = !string.IsNullOrEmpty(Model.Name);
            Model.PropertyChanged += Model_PropertyChanged;
            Model.OutputSettings.CollectionChanged += OutputSettings_CollectionChanged;
        }

        private void OutputSettings_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CanRemoveOutputDevices = Model.OutputSettings.Count > 1;
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
            }
        }

        public void DeleteOutputProfile()
        {
            App.OutputProfileManager.RemoveOutputProfile(Model.Id);
        }

        public void SaveOutputProfile()
        {
            OutputProfile toSave = Model.ShallowCopy();
            App.OutputProfileManager.SaveOutputProfile(toSave);
            // Copy back and forth to incorporate validations done during saving
            _model = toSave.ShallowCopy();
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

        public override void Dispose()
        {
            Model.PropertyChanged -= Model_PropertyChanged;
            base.Dispose();
        }
    }
}