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
using AmplitudeSoundboard;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amplitude.ViewModels
{
    public abstract class ViewModelBase : ReactiveObject, INotifyPropertyChanged, IDisposable
    {
        protected ThemeHandler ThemeHandler { get => App.ThemeHandler; }
        protected SoundClipManager SoundClipManager { get => App.SoundClipManager; }
        protected OptionsManager OptionsManager { get => App.OptionsManager; }
        protected OutputProfileManager OutputProfileManager { get => App.OutputProfileManager; }
        protected WindowManager WindowManager { get => App.WindowManager; }
        protected HotkeysManager HotkeysManager { get => App.HotkeysManager; }
        protected ISoundEngine SoundEngine { get => App.SoundEngine; }

        public bool CanUseHotkeys => FeatureManager.IsFeatureEnabled(FeatureManager.Feature.HOTKEYS);
        public double HotkeysOpacity => CanUseHotkeys ? 1 : 0.3d;

        public bool CanUseCustomTitlebar => FeatureManager.IsFeatureEnabled(FeatureManager.Feature.CUSTOM_TITLEBAR);

        public virtual void Dispose() { }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
