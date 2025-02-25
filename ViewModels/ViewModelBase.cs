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

using Amplitude.Helpers;
using AmplitudeSoundboard;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amplitude.ViewModels
{
    public abstract class ViewModelBase : ReactiveObject, INotifyPropertyChanged, IDisposable
    {
        protected ThemeManager ThemeManager => App.ThemeManager;
        protected SoundClipManager SoundClipManager => App.SoundClipManager;
        protected ConfigManager ConfigManager => App.ConfigManager;
        protected OutputProfileManager OutputProfileManager => App.OutputProfileManager;
        protected WindowManager WindowManager => App.WindowManager;
        protected HotkeysManager HotkeysManager => App.HotkeysManager;
        protected ISoundEngine SoundEngine => App.SoundEngine;

        protected bool CanUseHotkeys => FeatureManager.IsFeatureEnabled(FeatureManager.Feature.HOTKEYS);
        protected double HotkeysOpacity => CanUseHotkeys ? 1 : 0.3d;

        protected bool CanUseCustomTitlebar => FeatureManager.IsFeatureEnabled(FeatureManager.Feature.CUSTOM_TITLEBAR);

        protected bool CanAdjustWindowOpacity => FeatureManager.IsFeatureEnabled(FeatureManager.Feature.BACKGROUND_OPACTIY);

        public virtual void Dispose() { }

        public new event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
