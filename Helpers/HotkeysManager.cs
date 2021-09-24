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

using Amplitude.Models;
using AmplitudeSoundboard;
using System.Collections.Generic;

namespace Amplitude.Helpers
{
    public class HotkeysManager
    {
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

        public const string UNBIND_HOTKEY = "UNBIND_HOTKEY";
        public const string MASTER_STOP_SOUND_HOTKEY = "MASTER_STOP_SOUND_HOTKEY";

        private HotkeysManager()
        {

        }

        public Dictionary<string, List<string>> Hotkeys = new Dictionary<string, List<string>>();

        public void RegisterHotkeyAtStartup(string? id, string hotkeyString)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(hotkeyString))
            {
                return;
            }

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

        public void RecordGlobalStopSoundHotkey(Options options)
        {
            App.KeyboardHook.SetGlobalStopHotkey(options, RecordGlobalStopSoundHotkeyCallback);
        }

        public void RecordGlobalStopSoundHotkeyCallback(Options options, string hotkeyString)
        {
            if (string.IsNullOrEmpty(hotkeyString))
            {
                return;
            }

            if (options != null)
            {
                if (hotkeyString == UNBIND_HOTKEY)
                {
                    options.GlobalKillAudioHotkey = "";
                }
                else
                {
                    options.GlobalKillAudioHotkey = hotkeyString;
                }
            }
        }

        public void RecordSoundClipHotkey(SoundClip clip)
        {
            App.KeyboardHook.SetSoundClipHotkey(clip, RecordSoundClipHotkeyCallback);
        }

        public void RecordSoundClipHotkeyCallback(SoundClip clip, string hotkeyString)
        {
            if (string.IsNullOrEmpty(hotkeyString))
            {
                return;
            }

            if (clip != null)
            {
                if (hotkeyString == UNBIND_HOTKEY)
                {
                    clip.Hotkey = "";
                }
                else
                {
                    clip.Hotkey = hotkeyString;
                }
            }
        }

        public void RemoveHotkey(string? id, string? hotkeyString)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(hotkeyString))
            {
                return;
            }

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

            if (id != MASTER_STOP_SOUND_HOTKEY)
            {
                SoundClip clip = App.SoundClipManager.GetClip(id);

                if (clip != null)
                {
                    clip.Hotkey = "";
                }
            }
        }

        public static void StopAllSound()
        {
            App.SoundEngine.Reset(true);
        }
    }
}
