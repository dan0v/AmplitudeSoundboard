/*
    Author: Taylor-Cozy
 */

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amplitude.Helpers
{
    public class HotkeysManager
    {
        /*
         * Functionality:
         *  List of hotkeys
         *  Register a hotkey
         *  Remove a hotkey
         */

        private static HotkeysManager _instance;
        public static HotkeysManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HotkeysManager();
                }
                return _instance;
            }
        }

        private HotkeysManager()
        {

        }

        public Dictionary<string, List<string>> Hotkeys = new Dictionary<string, List<string>>();

        void RegisterHotkey(string hotkeyString, string id)
        {

            // Add hotkey to dict
            if (Hotkeys.TryGetValue(hotkeyString, out List<string> val))
            {
                val.Add(id);
            }
            else
            {
                Hotkeys.Add(hotkeyString, new List<string> { id });
            }

        }

        void RemoveHotkey(string hotkeyString, string id)
        {
            if (Hotkeys.TryGetValue(hotkeyString, out List<string> val))
            {
                if (val.Count <= 1)
                {
                    Hotkeys.Remove(hotkeyString);
                }
                else
                {
                    val.Remove(id);
                }
            }
        }
    }
}
