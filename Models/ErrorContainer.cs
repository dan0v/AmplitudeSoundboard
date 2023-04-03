/*
    AmplitudeSoundboard
    Copyright (C) 2021-2023 dan0v
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

using AmplitudeSoundboard;

namespace Amplitude.Models
{
    public class ErrorContainer
    {
        public string ErrorMessage { get; set; } = "";
        public bool LinkedSoundClip { get; set; } = false;
        public string SoundClipId { get; set; } = "";
        public bool LinkedOutputProfile { get; set; } = false;
        public string OutputProfileId { get; set; } = "";

        public ErrorContainer(string errorMessage)
        {
            this.ErrorMessage = errorMessage;
        }

        public ErrorContainer(string errorMessage, SoundClip clip)
        {
            this.ErrorMessage = errorMessage;
            if (!string.IsNullOrEmpty(clip?.Id))
            {
                this.LinkedSoundClip = true;
                this.SoundClipId = clip?.Id;
            }
        }

        public ErrorContainer(string errorMessage, OutputProfile profile)
        {
            this.ErrorMessage = errorMessage;
            if (!string.IsNullOrEmpty(profile?.Id))
            {
                this.LinkedOutputProfile = true;
                this.OutputProfileId = profile?.Id;
            }
        }

        public void OpenEditSoundClipWindow()
        {
            if (App.SoundClipManager.GetClip(SoundClipId) != null)
            {
                App.WindowManager.OpenEditSoundClipWindow(SoundClipId);
            }
        }
        public void OpenEditOutputProfileWindow()
        {
            if (App.OutputProfileManager.GetOutputProfile(OutputProfileId) != null)
            {
                App.WindowManager.OpenEditOutputProfileWindow(OutputProfileId);
            }
        }
    }
}
