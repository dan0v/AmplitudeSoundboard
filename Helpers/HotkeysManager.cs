/*
    Author: Taylor-Cozy
 */

/*
    AmplitudeSoundboard
    Copyright (C) 2021-2024 dan0v
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
        private static HotkeysManager? _instance;
        public static HotkeysManager Instance => _instance ??= new HotkeysManager();

        public const string UNBIND_HOTKEY = "UNBIND_HOTKEY";
        public const string MASTER_STOP_SOUND_HOTKEY = "MASTER_STOP_SOUND_HOTKEY";

        private HotkeysManager()
        {

        }

        public Dictionary<string, List<string>> Hotkeys = [];

        public void RegisterHotkeyAtStartup(string? id, string hotkeyString)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(hotkeyString))
            {
                return;
            }

            // Add hotkey to dict
            if (Hotkeys.TryGetValue(hotkeyString, out List<string>? val))
            {
                val?.Add(id);
            }
            else
            {
                Hotkeys.Add(hotkeyString, [id]);
            }
        }

        public void RecordGlobalStopSoundHotkey(Config config)
        {
            App.KeyboardHook.SetGlobalStopHotkey(config, RecordGlobalStopSoundHotkeyCallback);
        }

        public void RecordGlobalStopSoundHotkeyCallback(Config config, string hotkeyString)
        {
            if (string.IsNullOrEmpty(hotkeyString))
            {
                return;
            }

            if (config != null)
            {
                if (hotkeyString == UNBIND_HOTKEY)
                {
                    config.GlobalKillAudioHotkey = "";
                }
                else
                {
                    config.GlobalKillAudioHotkey = hotkeyString;
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

            if (Hotkeys.TryGetValue(hotkeyString, out List<string>? val))
            {
                if (val?.Count <= 1)
                {
                    Hotkeys.Remove(hotkeyString);
                }
                else
                {
                    val?.Remove(id);
                }
            }

            if (id != MASTER_STOP_SOUND_HOTKEY)
            {
                var clip = App.SoundClipManager.GetClip(id);

                if (clip != null)
                {
                    clip.Hotkey = "";
                }
            }
        }

        public static void StopAllSound()
        {
            App.SoundEngine.Reset();
        }
    }
}
