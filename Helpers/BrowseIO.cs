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
using Avalonia.Controls;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Amplitude.Helpers
{
    class BrowseIO
    {
        public enum FileBrowserType
        {
            AUDIO,
            IMAGE
        }

        private static FileDialogFilter _audioFileTypesFilter = new FileDialogFilter
        {
            Name = "Audio file",
            Extensions = { "aac", "aiff", "alac", "flac", "m4a", "mp3", "mp4", "ogg", "opus", "wav" }
        };
        public static FileDialogFilter AudioFileTypesFilter
        {
            get => _audioFileTypesFilter;
        }

        private static FileDialogFilter _imageFileTypesFilter = new FileDialogFilter
        {
            Name = "Image file",
            Extensions = { "png", "jpg", "jpeg", "gif", "bmp" }
        };
        public static FileDialogFilter ImageFileTypesFilter
        {
            get => _imageFileTypesFilter;
        }

        public static async Task<string[]> OpenFileBrowser(Window parent, FileBrowserType type, bool allowMultiple = false)
        {
            string title = "";

            FileDialogFilter filter = AudioFileTypesFilter;

            switch (type)
            {
                case FileBrowserType.AUDIO:
                    title = Localization.Localizer.Instance["FileBrowseAudio"];
                    filter = AudioFileTypesFilter;
                    break;
                case FileBrowserType.IMAGE:
                    title = Localization.Localizer.Instance["FileBrowseImage"];
                    filter = ImageFileTypesFilter;
                    break;
            }

            var dialog = new OpenFileDialog()
            {
                Title = title,
                AllowMultiple = allowMultiple,
                Filters = { filter }
            };
            return await dialog.ShowAsync(parent);
        }

        public static bool ValidImage(string fileName, bool generateErrors = true)
        {
            if (!File.Exists(fileName))
            {
                if (generateErrors)
                {
                    string errorMessage = string.Format(Localization.Localizer.Instance["FileMissingString"], fileName);
                    App.WindowManager.ShowErrorString(errorMessage);
                }
                return false;
            }

            string fileType = Path.GetExtension(fileName).ToLower();
            if (fileType.Length >= 1)
            {
                fileType = fileType.Substring(1);
            }
            if (ImageFileTypesFilter.Extensions.Where(i => i.ToLower() == fileType).Count() < 1)
            {
                if (generateErrors)
                {
                    string errorMessage = string.Format(Localization.Localizer.Instance["FileBadFormatString"], fileName);
                    App.WindowManager.ShowErrorString(errorMessage);
                }
                return false;
            }
            return true;
        }

        public static bool ValidAudioFile(string fileName, bool generateErrors = true, SoundClip? clip = null)
        {
            if (!File.Exists(fileName))
            {
                if (generateErrors)
                {
                    if (clip != null)
                    {
                        App.WindowManager.ShowErrorSoundClip(clip, ViewModels.ErrorListViewModel.SoundClipErrorType.MISSING_AUDIO_FILE);
                    }
                    else
                    {
                        string errorMessage = string.Format(Localization.Localizer.Instance["FileMissingString"], fileName);
                        App.WindowManager.ShowErrorString(errorMessage);
                    }
                }
                return false;
            }

            string fileType = Path.GetExtension(fileName).ToLower();
            if (fileType.Length >= 1)
            {
                fileType = fileType.Substring(1);
            }
            if (!AudioFileTypesFilter.Extensions.Where(a => a.ToLower() == fileType).Any())
            {
                if (generateErrors)
                {
                    if (clip != null)
                    {
                        App.WindowManager.ShowErrorSoundClip(clip, ViewModels.ErrorListViewModel.SoundClipErrorType.BAD_AUDIO_FORMAT);
                    }
                    else
                    {
                        string errorMessage = string.Format(Localization.Localizer.Instance["FileBadFormatString"], fileName);
                        App.WindowManager.ShowErrorString(errorMessage);
                    }
                }
                return false;
            }
            return true;
        }
    }
}
