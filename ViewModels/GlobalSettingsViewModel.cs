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
using Amplitude.Models;
using Avalonia.Media;
using System.Linq;

namespace Amplitude.ViewModels
{
    public sealed class GlobalSettingsViewModel : ViewModelBase
    {
        private Options _model;
        public Options Model { get => _model; }
        public static string[] Languages { get => Localization.Localizer.Languages.Keys.ToArray(); }

        public GlobalSettingsViewModel()
        {
            _model = OptionsManager.Options.ShallowCopy();
            Model.PropertyChanged += Model_PropertyChanged;
        }

        private void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.GlobalKillAudioHotkey))
            {
                WaitingForHotkey = false;
            }
        }

        public bool CanSave
        {
            get
            {
                return !WaitingForHotkey;
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

        public Color HotkeyBackgroundColor => WaitingForHotkey ? ThemeHandler.TextBoxHighlightedColor : ThemeHandler.TextBoxNormalColor;

        public void RecordHotkey()
        {
            Model.GlobalKillAudioHotkey = Localization.Localizer.Instance["HotkeyCancelPlaceholder"];
            WaitingForHotkey = true;
            HotkeysManager.RecordGlobalStopSoundHotkey(Model);
        }

        public void SaveOptions()
        {
            OptionsManager.SaveAndOverwriteOptions(Model);
            _model = Model.ShallowCopy();
            OnPropertyChanged(nameof(Model));
        }

        public void ClearPositions()
        {
            WindowManager.ClearWindowSizesAndPositions();
        }

        public override void Dispose()
        {
            Model.PropertyChanged -= Model_PropertyChanged;
            base.Dispose();
        }
    }
}