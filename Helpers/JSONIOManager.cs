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

using AmplitudeSoundboard;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Amplitude.Helpers
{
    public class JSONIOManager
    {
        protected static Dictionary<string, T>? ConvertObjectsFromJSON<T>(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                var obj = JsonConvert.DeserializeObject(json, typeof(Dictionary<string, T>));
                return obj == null ? null : (Dictionary<string, T>?)obj;
            }
            catch (Exception e)
            {
                App.WindowManager.ShowErrorString(e.Message);
            }
            return null;
        }

        protected static string? RetrieveJSONFromFile(string file)
        {
            try
            {
                if (File.Exists(Path.Join(App.APP_STORAGE, file)))
                {
                    return File.ReadAllText(Path.Join(App.APP_STORAGE, file));
                }
            }
            catch (Exception e)
            {
                App.WindowManager.ShowErrorString(e.Message);
            }
            return null;
        }

        protected static void SaveJSONToFile(string file, string json)
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
