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
using AmplitudeSoundboard;
using SharpHook;
using SharpHook.Data;
using SharpHook.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Amplitude.Helpers
{
    public class SharpKeyboardHook : IKeyboardHook
    {
        private const string MacOSAccessibilityMessage =
            "Hotkeys need macOS Accessibility permission before Amplitude Soundboard can record or trigger keybinds. " +
            "Open System Settings > Privacy & Security > Accessibility, enable Amplitude Soundboard, then restart the app.";

        private IGlobalHook? sharpHook;
        private Task? hookTask;
        private bool hookAvailable;
        private bool disposed;

        private static List<(SoundClip clip, Action<SoundClip, string> callback)> soundClipCallbacks = new();
        private static (Config? config, Action<Config, string> callback) globalStopCallback;

        private static readonly SortedSet<KeyCode> keySet = new();
        private readonly object keySetLock = new();

        private static SharpKeyboardHook? _instance;
        public static IKeyboardHook Instance => _instance ??= new SharpKeyboardHook();

        private SharpKeyboardHook()
        {
            UioHookProvider.Instance.KeyTypedEnabled = false;
            StartHook(promptForMacOSAccessibility: true, showErrors: false);
        }

        private bool StartHook(bool promptForMacOSAccessibility, bool showErrors)
        {
            if (disposed)
            {
                return false;
            }

            if (hookTask != null && !hookTask.IsCompleted)
            {
                return true;
            }

#if MacOS
            if (!UioHookProvider.Instance.IsAxApiEnabled(promptForMacOSAccessibility))
            {
                hookAvailable = false;
                if (showErrors)
                {
                    ShowAccessibilityError();
                    OpenAccessibilitySettings();
                }
                return false;
            }
#endif

            try
            {
                sharpHook?.Dispose();
                sharpHook = new EventLoopGlobalHook(GlobalHookType.Keyboard);
                sharpHook.HookEnabled += HandleHookEnabled;
                sharpHook.HookDisabled += HandleHookDisabled;
                sharpHook.KeyPressed += HandleKeyPressed;
                sharpHook.KeyReleased += HandleKeyReleased;

                hookTask = sharpHook.RunAsync();
                hookTask.ContinueWith(HandleHookFailure, TaskContinuationOptions.OnlyOnFaulted);
                hookAvailable = true;
                return true;
            }
            catch (HookException ex)
            {
                HandleHookException(ex, showErrors);
            }
            catch (Exception ex)
            {
                hookAvailable = false;
                Debug.WriteLine(ex);
                if (showErrors)
                {
                    App.WindowManager.ShowErrorString($"Hotkeys could not be started: {ex.Message}");
                }
            }
            return false;
        }

        private void HandleHookEnabled(object? sender, HookEventArgs e)
        {
            hookAvailable = true;
        }

        private void HandleHookDisabled(object? sender, HookEventArgs e)
        {
            hookAvailable = false;
        }

        private void HandleHookFailure(Task task)
        {
            hookAvailable = false;
            if (task.Exception == null)
            {
                return;
            }

            foreach (var exception in task.Exception.Flatten().InnerExceptions)
            {
                Debug.WriteLine(exception);
                if (exception is HookException hookException)
                {
                    HandleHookException(hookException, showErrors: false);
                }
            }
        }

        private void HandleHookException(HookException ex, bool showErrors)
        {
            hookAvailable = false;
            Debug.WriteLine(ex);
#if MacOS
            if (ex.Result == UioHookResult.ErrorAxApiDisabled)
            {
                if (showErrors)
                {
                    ShowAccessibilityError();
                    OpenAccessibilitySettings();
                }
                return;
            }
#endif
            if (showErrors)
            {
                App.WindowManager.ShowErrorString($"Hotkeys could not be started: {ex.Message}");
            }
        }

        private bool EnsureHookAvailable()
        {
            if (hookAvailable && hookTask != null && !hookTask.IsCompleted)
            {
                return true;
            }

            return StartHook(promptForMacOSAccessibility: true, showErrors: true);
        }

        private static void ShowAccessibilityError()
        {
            App.WindowManager.ShowErrorString(MacOSAccessibilityMessage);
        }

        private static void OpenAccessibilitySettings()
        {
#if MacOS
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "x-apple.systempreferences:com.apple.preference.security?Privacy_Accessibility",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
#endif
        }

        private void HandleKeyReleased(object? sender, KeyboardHookEventArgs e)
        {
            lock (keySetLock)
            {
                keySet.Remove(e.Data.KeyCode);
            }
        }

        private void HandleKeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            var keyCode = e.Data.KeyCode;

            if (IgnoreKey(keyCode))
            {
                return;
            }

            if (IsKeySpecial(keyCode))
            {
                lock (keySetLock)
                {
                    keySet.Add(keyCode);
                }
            }
            else
            {
                ProcessHotkey(GetFriendlyKeyName(keyCode));
            }

        }

        private void ProcessHotkey(string currentKey)
        {
            var fullKey = FullKey(currentKey);

            if (globalStopCallback.config != null)
            {
                globalStopCallback.callback(globalStopCallback.config, fullKey);
                globalStopCallback.config = null;
            }
            else if (soundClipCallbacks.Count > 0)
            {
                foreach (var callback in soundClipCallbacks)
                {
                    callback.callback(callback.clip, fullKey);
                }
                soundClipCallbacks.Clear();
            }
            else if (currentKey != HotkeysManager.UNBIND_HOTKEY && App.HotkeysManager.Hotkeys.TryGetValue(fullKey, out List<string>? values))
            {
                // Go through and call all the Play methods on these id's
                foreach (string item in values)
                {
                    if (item == HotkeysManager.MASTER_STOP_SOUND_HOTKEY)
                    {
                        HotkeysManager.StopAllSound();
                    }
                    else
                    {
                        var clip = App.SoundClipManager.GetClip(item);
                        clip?.PlayAudio();
                    }
                }
            }
        }

        private string FullKey(string currentKey)
        {
            lock (keySetLock)
            {
                var keySetCopy = new SortedSet<KeyCode>(keySet);
            }

            if (keySet.Count > 0)
            {
                string full = "";
                foreach (KeyCode keyCode in keySet)
                {
                    full += GetFriendlyKeyName(keyCode) + "|";
                }
                return full + currentKey;
            }
            else
            {
                return currentKey;
            }
        }

        private static bool IgnoreKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.VcLeftMeta:
                case KeyCode.VcRightMeta:
                case KeyCode.VcUndefined:
                    return true;
                default: return false;
            }
        }

        private static bool IsKeySpecial(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.VcLeftAlt:
                case KeyCode.VcRightAlt:
                case KeyCode.VcLeftControl:
                case KeyCode.VcRightControl:
                case KeyCode.VcLeftShift:
                case KeyCode.VcRightShift:
                    return true;
                default: return false;
            }
        }

        private static string GetFriendlyKeyName(KeyCode keycode)
        {
            switch (keycode)
            {
                case KeyCode.VcTab: return "Tab";
                case KeyCode.VcEnter: return "Enter";
                case KeyCode.VcCapsLock: return "Caps";
                case KeyCode.VcEscape: return HotkeysManager.UNBIND_HOTKEY;
                case KeyCode.VcSpace: return "Space";
                case KeyCode.VcPageUp: return "PgUp";
                case KeyCode.VcPageDown: return "PgDown";
                case KeyCode.VcEnd: return "End";
                case KeyCode.VcHome: return "Home";
                case KeyCode.VcLeft: return "LArrow";
                case KeyCode.VcUp: return "UArrow";
                case KeyCode.VcRight: return "RArrow";
                case KeyCode.VcDown: return "DArrow";
                case KeyCode.VcDelete: return "Del";
                case KeyCode.VcLeftMeta: return "LWin";
                case KeyCode.VcRightMeta: return "RWin";
                case KeyCode.VcPeriod: return ".";
                case KeyCode.VcNumPadDecimal: return "Num.";
                case KeyCode.VcNumPadEnter: return "NumEnter";
                case KeyCode.VcNumPadMultiply: return "Num*";
                case KeyCode.VcNumPadAdd: return "Num+";
                case KeyCode.VcNumPadSubtract: return "Num-";
                case KeyCode.VcNumPadDivide: return "Num/";
                case KeyCode.VcNumLock: return "NumLck";
                case KeyCode.VcLeftShift: return "LShift";
                case KeyCode.VcRightShift: return "RShift";
                case KeyCode.VcLeftControl: return "LCtrl";
                case KeyCode.VcRightControl: return "RCtrl";
                case KeyCode.VcLeftAlt: return "LAlt";
                case KeyCode.VcRightAlt: return "RAlt";
                case KeyCode.VcSemicolon: return ";";
                case KeyCode.VcEquals: return "=";
                case KeyCode.VcComma: return ",";
                case KeyCode.VcMinus: return "-";
                case KeyCode.VcSlash: return "/";
                case KeyCode.VcBackslash: return "\\";
                case KeyCode.VcOpenBracket: return "[";
                case KeyCode.VcCloseBracket: return "]";
                case KeyCode.VcQuote: return "\'";
                case KeyCode.VcBackQuote: return "`";
                case KeyCode.VcNumPad0: return "Num0";
                case KeyCode.VcNumPad1: return "Num1";
                case KeyCode.VcNumPad2: return "Num2";
                case KeyCode.VcNumPad3: return "Num3";
                case KeyCode.VcNumPad4: return "Num4";
                case KeyCode.VcNumPad5: return "Num5";
                case KeyCode.VcNumPad6: return "Num6";
                case KeyCode.VcNumPad7: return "Num7";
                case KeyCode.VcNumPad8: return "Num8";
                case KeyCode.VcNumPad9: return "Num9";
            }

            return keycode.ToString()[2..];
        }

        public bool SetSoundClipHotkey(SoundClip clip, Action<SoundClip, string> callback)
        {
            if (!EnsureHookAvailable())
            {
                return false;
            }

            soundClipCallbacks.Add((clip, callback));
            return true;
        }

        public bool SetGlobalStopHotkey(Config config, Action<Config, string> callback)
        {
            if (!EnsureHookAvailable())
            {
                return false;
            }

            globalStopCallback = (config, callback);
            return true;
        }

        public void Dispose()
        {
            disposed = true;
            if (sharpHook != null)
            {
                sharpHook.HookEnabled -= HandleHookEnabled;
                sharpHook.HookDisabled -= HandleHookDisabled;
                sharpHook.KeyPressed -= HandleKeyPressed;
                sharpHook.KeyReleased -= HandleKeyReleased;
                sharpHook.Dispose();
            }
        }
    }
}
