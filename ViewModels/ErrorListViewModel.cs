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
using System;
using System.Collections.ObjectModel;

namespace Amplitude.ViewModels
{
    public sealed class ErrorListViewModel : ViewModelBase
    {
        public ObservableCollection<ErrorContainer> _errors = new();
        public ObservableCollection<ErrorContainer> Errors => _errors;

        public ErrorListViewModel(){ }

        public void AddErrorString(string errorString)
        {
            ErrorContainer error = new ErrorContainer(errorString);
            Errors.Add(error);
        }

        public void AddErrorSoundClip(SoundClip clip, ErrorType errorType, string? additionalData = null)
        {
            string errorString = "";

            switch (errorType)
            {
                case ErrorType.BAD_IMAGE_FORMAT:
                    errorString = string.Format(Localization.Localizer.Instance["SoundClipError"], clip.Name, string.Format(Localization.Localizer.Instance["FileBadFormatString"], clip.ImageFilePath));
                    break;
                case ErrorType.BAD_AUDIO_FORMAT:
                    errorString = string.Format(Localization.Localizer.Instance["SoundClipError"], clip.Name, string.Format(Localization.Localizer.Instance["FileBadFormatString"], clip.AudioFilePath));
                    break;
                case ErrorType.MISSING_IMAGE_FILE:
                    errorString = string.Format(Localization.Localizer.Instance["SoundClipError"], clip.Name, string.Format(Localization.Localizer.Instance["FileMissingString"], clip.ImageFilePath));
                    break;
                case ErrorType.MISSING_AUDIO_FILE:
                    errorString = string.Format(Localization.Localizer.Instance["SoundClipError"], clip.Name, string.Format(Localization.Localizer.Instance["FileMissingString"], clip.AudioFilePath));
                    break;
                case ErrorType.MISSING_DEVICE:
                    errorString = string.Format(Localization.Localizer.Instance["SoundClipError"], clip.Name, string.Format(Localization.Localizer.Instance["MissingDeviceString"], additionalData ?? ""));
                    break;
            }

            string[] lines = errorString.Split('\n');

            if (lines.Length > 1)
            {
                errorString = "";
                for (int i = 0; i < lines.Length; i++)
                {
                    errorString += (i < lines.Length - 1) ? lines[i] + Environment.NewLine : lines[i];
                }
            }

            ErrorContainer error = new ErrorContainer(errorString, clip.Id);

            Errors.Add(error);
        }

        public enum ErrorType
        {
            MISSING_IMAGE_FILE,
            MISSING_AUDIO_FILE,
            BAD_AUDIO_FORMAT,
            BAD_IMAGE_FORMAT,
            MISSING_DEVICE,
        }

        public void Dismiss()
        {
            Errors.Clear();
            WindowManager.ErrorListWindowOpen = false;
            WindowManager.ErrorListWindow.Hide();
        }
    }
}
