using Amplitude.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amplitude.Helpers
{
    public class BlankHotkeysManager : IKeyboardHook
    {
        private static BlankHotkeysManager? _instance;
        public static BlankHotkeysManager Instance { get => _instance ??= new BlankHotkeysManager(); }

        public void Dispose()
        {
            
        }

        public void SetGlobalStopHotkey(Options options, Action<Options, string> callback)
        {
            
        }

        public void SetSoundClipHotkey(SoundClip clip, Action<SoundClip, string> callback)
        {
            
        }
    }
}
