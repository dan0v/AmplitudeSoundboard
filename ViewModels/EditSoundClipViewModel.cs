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
using Amplitude.Views;
using AmplitudeSoundboard;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Windows.Input;

namespace Amplitude.ViewModels
{
    public class EditSoundClipViewModel : ViewModelBase
    {
        private SoundClip _model;
        public SoundClip Model { get => _model; }

        public EditSoundClipViewModel()
        {
            _model = new SoundClip();
        }

        /// <summary>
        ///  Edit an existing soundclip from this EditSoundClip window
        /// </summary>
        /// <param name="model"></param>
        public EditSoundClipViewModel(SoundClip model)
        {
            this._model = model;
        }

        public void PlaySound()
        {
            Model.PlayAudio();
        }

        public void SetClipFilePath(string[] url)
        {
            if (url.Length > 0)
            {
                Model.FilePath = url[0];
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

        public void StopAudio()
        {
            App.SoundEngine.Reset();
        }

        public void CreateHotkey()
        {

        }

    }
}