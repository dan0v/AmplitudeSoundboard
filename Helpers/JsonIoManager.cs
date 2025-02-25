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

using AmplitudeSoundboard;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Amplitude.Helpers
{

    public class JsonIoManager
    {
        private static JsonIoManager? _instance;
        public static JsonIoManager Instance => _instance ??= new JsonIoManager();

        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true,
            TypeInfoResolver = JsonTypeInfoResolver.Combine(
                SoundClipManagerContext.Default,
                OutputProfileManagerContext.Default,
                ConfigManagerContext.Default,
                ThemeManagerContext.Default,
                WindowManagerContext.Default
                )
        };

        public T? ConvertObjectsFromJSON<T>(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            try
            {
                var obj = JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
                return obj == null ? default : (T?)obj;
            }
            catch (Exception e)
            {
                App.WindowManager.ShowErrorString(e.Message);
            }
            return default(T);
        }

        public string RetrieveJSONFromFile(string file)
        {
            try
            {
                var path = Path.Join(App.APP_STORAGE, file);
                if (File.Exists(path))
                {
                    return File.ReadAllText(path);
                }
            }
            catch (Exception e)
            {
                App.WindowManager.ShowErrorString(e.Message);
            }
            return "";
        }

        public string ConvertObjectsToJSON<T>(T? obj)
        {
            if (obj == null)
            {
                return "";
            }

            try
            {
                return JsonSerializer.Serialize(obj, jsonSerializerOptions);
            }
            catch (Exception e)
            {
                App.WindowManager.ShowErrorString(e.Message);
            }
            return "";
        }

        public void SaveJSONToFile(string file, string json)
        {
            try
            {
                File.WriteAllText(Path.Join(App.APP_STORAGE, file), json);
            }
            catch (Exception e)
            {
                App.WindowManager.ShowErrorString(e.Message);
            }
        }
    }
}
