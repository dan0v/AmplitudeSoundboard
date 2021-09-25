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

using System.Threading.Tasks;
using Avalonia.Controls;

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
            Extensions = { "wav", "aiff", "mp3", "m4a", "mp4" }
        };
        public static FileDialogFilter AudioFileTypesFilter
        {
            get => _audioFileTypesFilter;
        }

        private static FileDialogFilter _imageFileTypesFilter = new FileDialogFilter
        {
            Name = "Image file",
            Extensions = { "png", "jpg", "jpeg", "gif" }
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
    }   
}
