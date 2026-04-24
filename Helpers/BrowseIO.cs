/*
    AmplitudeSoundboard
    Copyright (C) 2021-2026 dan0v
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
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Splat;
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

        private static readonly string[] AudioFileExtensions = ["aac", "aiff", "alac", "flac", "m4a", "mp3", "mp4", "ogg", "opus", "wav"];
        private static readonly string[] ImageFileExtensions = ["png", "jpg", "jpeg", "gif", "bmp"];

        private static FilePickerFileType AudioFileTypesFilter => new("Audio file")
        {
            Patterns = AudioFileExtensions.Select(e => $"*.{e}").ToList()
        };

        private static FilePickerFileType ImageFileTypesFilter => new("Image file")
        {
            Patterns = ImageFileExtensions.Select(e => $"*.{e}").ToList()
        };

        public static async Task<string[]?> OpenFileBrowser(Window parent, FileBrowserType type, bool allowMultiple = false)
        {
            string title = "";
            FilePickerFileType filter = AudioFileTypesFilter;

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

            var storageProvider = parent.StorageProvider;
            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = allowMultiple,
                FileTypeFilter = [filter]
            };

            var result = await storageProvider.OpenFilePickerAsync(options);

            if (result == null || result.Count == 0)
            {
                return null;
            }

            return result.Select(f => f.TryGetLocalPath()).Where(p => p != null).ToArray()!;
        }

        public static bool ValidImage(string fileName, bool generateErrors = true)
        {
            if (!File.Exists(fileName))
            {
                if (generateErrors)
                {
                    string errorMessage = string.Format(Localization.Localizer.Instance["FileMissingString"], fileName);
                    Locator.Current.GetService<WindowManager>()!.ShowErrorString(errorMessage);
                }
                return false;
            }

            string fileType = Path.GetExtension(fileName).ToLower();
            if (fileType.Length >= 1)
            {
                fileType = fileType.Substring(1);
            }
            if (!ImageFileExtensions.Any(i => i.ToLower() == fileType))
            {
                if (generateErrors)
                {
                    string errorMessage = string.Format(Localization.Localizer.Instance["FileBadFormatString"], fileName);
                    Locator.Current.GetService<WindowManager>()!.ShowErrorString(errorMessage);
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
                        Locator.Current.GetService<WindowManager>()!.ShowErrorSoundClip(clip, ViewModels.ErrorListViewModel.SoundClipErrorType.MISSING_AUDIO_FILE);
                    }
                    else
                    {
                        string errorMessage = string.Format(Localization.Localizer.Instance["FileMissingString"], fileName);
                        Locator.Current.GetService<WindowManager>()!.ShowErrorString(errorMessage);
                    }
                }
                return false;
            }

            string fileType = Path.GetExtension(fileName).ToLower();
            if (fileType.Length >= 1)
            {
                fileType = fileType.Substring(1);
            }
            if (!AudioFileExtensions.Any(a => a.ToLower() == fileType))
            {
                if (generateErrors)
                {
                    if (clip != null)
                    {
                        Locator.Current.GetService<WindowManager>()!.ShowErrorSoundClip(clip, ViewModels.ErrorListViewModel.SoundClipErrorType.BAD_AUDIO_FORMAT);
                    }
                    else
                    {
                        string errorMessage = string.Format(Localization.Localizer.Instance["FileBadFormatString"], fileName);
                        Locator.Current.GetService<WindowManager>()!.ShowErrorString(errorMessage);
                    }
                }
                return false;
            }
            return true;
        }
    }
}
