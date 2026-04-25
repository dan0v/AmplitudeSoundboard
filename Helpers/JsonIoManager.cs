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

using AmplitudeSoundboard;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Amplitude.Helpers
{

    public class JsonIoManager
    {
        private readonly Lazy<WindowManager> _windowManager;
        private WindowManager WindowManager => _windowManager.Value;

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

        public JsonIoManager(Lazy<WindowManager> windowManager)
        {
            _windowManager = windowManager;
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
            Justification = "All types are registered in the JsonSerializerOptions TypeInfoResolver via source-generated contexts.")]
        [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
            Justification = "All types are registered in the JsonSerializerOptions TypeInfoResolver via source-generated contexts.")]
        public T? ConvertObjectsFromJSON<T>(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            try
            {
                var typeInfo = jsonSerializerOptions.GetTypeInfo(typeof(T));
                var obj = JsonSerializer.Deserialize(json, typeInfo);
                return obj == null ? default : (T?)obj;
            }
            catch (Exception e)
            {
                WindowManager.ShowErrorString(e.Message);
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
                WindowManager.ShowErrorString(e.Message);
            }
            return "";
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
            Justification = "All types are registered in the JsonSerializerOptions TypeInfoResolver via source-generated contexts.")]
        [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
            Justification = "All types are registered in the JsonSerializerOptions TypeInfoResolver via source-generated contexts.")]
        public string ConvertObjectsToJSON<T>(T? obj)
        {
            if (obj == null)
            {
                return "";
            }

            try
            {
                var typeInfo = jsonSerializerOptions.GetTypeInfo(typeof(T));
                return JsonSerializer.Serialize(obj, typeInfo);
            }
            catch (Exception e)
            {
                WindowManager.ShowErrorString(e.Message);
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
                WindowManager.ShowErrorString(e.Message);
            }
        }
    }
}
