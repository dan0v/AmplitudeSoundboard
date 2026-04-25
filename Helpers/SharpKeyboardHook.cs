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
using SharpHook;
using SharpHook.Data;
using SharpHook.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Amplitude.Helpers
{
    public class SharpKeyboardHook : IKeyboardHook
    {
        private readonly Lazy<HotkeysManager> _hotkeysManager;
        private readonly Lazy<SoundClipManager> _soundClipManager;
        private readonly Lazy<WindowManager> _windowManager;

        private HotkeysManager HotkeysManager => _hotkeysManager.Value;
        private SoundClipManager SoundClipManager => _soundClipManager.Value;
        private WindowManager WindowManager => _windowManager.Value;
        
        private IGlobalHook? sharpHook;
        private Task? hookTask;
        private volatile bool hookAvailable;
        private volatile bool disposed;
        private static List<(SoundClip clip, Action<SoundClip, string> callback)> soundClipCallbacks = new();
        private static (Config? config, Action<Config, string> callback) globalStopCallback;

        private static readonly SortedSet<KeyCode> keySet = new();
        private readonly object keySetLock = new();

        public SharpKeyboardHook(Lazy<HotkeysManager> hotkeysManager, Lazy<SoundClipManager> soundClipManager, Lazy<WindowManager> windowManager)
        {
            _hotkeysManager = hotkeysManager;
            _soundClipManager = soundClipManager;
            _windowManager = windowManager;
            UioHookProvider.Instance.KeyTypedEnabled = false;
            StartHook();
        }

        private bool StartHook()
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
            if (!UioHookProvider.Instance.IsAxApiEnabled(true))
            {
                ShowMacOSAccessibilityError();
                OpenAccessibilitySettings();
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
                return true;
            }
            catch (HookException ex)
            {
                HandleHookException(ex);
            }
            catch (Exception ex)
            {
                WindowManager.ShowErrorString(string.Format(Localization.Localizer.Instance["HotkeysStartError"], ex.Message));
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
                if (exception is HookException hookException)
                {
                    HandleHookException(hookException);
                }
            }
        }

        private void HandleHookException(HookException ex)
        {
            hookAvailable = false;
#if MacOS
            if (ex.Result == UioHookResult.ErrorAxApiDisabled)
            {
                ShowMacOSAccessibilityError();
                OpenAccessibilitySettings();
                return;
            }
#endif
            WindowManager.ShowErrorString(string.Format(Localization.Localizer.Instance["HotkeysStartError"], ex.Message));
        }

        private bool EnsureHookAvailable()
        {
            if (disposed)
            {
                return false;
            }

            if (hookAvailable)
            {
                return true;
            }

            return StartHook();
        }

        private static void ShowMacOSAccessibilityError()
        {
            WindowManager.ShowErrorString(Localization.Localizer.Instance["MacOSAccessibilityError"]);
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
                WindowManager.ShowErrorString(ex.Message);
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
            else if (currentKey != HotkeysManager.UNBIND_HOTKEY && HotkeysManager.Hotkeys.TryGetValue(fullKey, out List<string>? values))
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
                        var clip = SoundClipManager.GetClip(item);
                        clip?.PlayAudio();
                    }
                }
            }
        }

        private string FullKey(string currentKey)
        {
            SortedSet<KeyCode> keySetCopy;
            lock (keySetLock)
            {
                keySetCopy = [.. keySet];
            }

            if (keySetCopy.Count > 0)
            {
                return string.Join("|", keySetCopy.Select(k => GetFriendlyKeyName(k))) + "|" + currentKey;
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
