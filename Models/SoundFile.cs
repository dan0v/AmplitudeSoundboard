// License:     APL 2.0
// Author:      Benjamin N. Summerton <https://16bpp.net>
// https://gitlab.com/define-private-public/Bassoon
// Modifications by dan0v include changing field visibilities and removing unused fields

using libsndfileSharp;
using PortAudioSharp;
using System;

namespace Amplitude.Models;

public class SoundFile
{
    private bool disposed;

    internal SndFile audioFile;

    internal float volume = 1f;

    internal long cursor;

    internal bool playingBack;

    public Stream stream;

    internal int finalFrameSize;

    private readonly long totalFrames;

    public System.TimeSpan Duration => audioFile.Info.Duration;

    public float Volume
    {
        get
        {
            return volume;
        }
        set
        {
            volume = Math.Clamp(value, 0f, 1f);
        }
    }

    public bool IsPlaying => playingBack;

    public long TotalFrames => totalFrames;

    private float Cursor
    {
        get
        {
            return (float)cursor / (float)totalFrames * (float)audioFile.Info.Duration.TotalSeconds;
        }
        set
        {
            long value2 = (long)(value / (float)audioFile.Info.Duration.TotalSeconds * (float)totalFrames);
            value2 = Math.Clamp(value2, 0L, totalFrames);
            bool isPlaying = IsPlaying;
            if (cursor != totalFrames)
            {
                Pause();
            }

            cursor = value2;
            audioFile.Seek(cursor / audioFile.Info.channels, Seek.Set);
            if (isPlaying)
            {
                Play();
            }
        }
    }

    public SoundFile(string path, int device)
    {
        audioFile = new SndFile(path);
        StreamParameters defaultOutputParams = defaultOutputParameters();
        defaultOutputParams.channelCount = audioFile.Info.channels;
        defaultOutputParams.device = device;
        stream = new Stream(null, defaultOutputParams, audioFile.Info.samplerate, framesPerBuffer, StreamFlags.ClipOff, playCallback, this);
        var frameSize = (int)(audioFile.Info.channels * framesPerBuffer);
        finalFrameSize = frameSize < 0 ? int.MaxValue : frameSize;
        totalFrames = audioFile.Info.channels * audioFile.Info.frames;
    }

    private const uint framesPerBuffer = 4096;

    private StreamParameters defaultOutputParameters()
    {
        StreamParameters defaultOutputParams = default;
        defaultOutputParams.device = PortAudio.DefaultOutputDevice;
        if (defaultOutputParams.device == -1)
        {
            throw new Exception("No default audio output device available");
        }

        defaultOutputParams.channelCount = 2;
        defaultOutputParams.sampleFormat = SampleFormat.Float32;
        defaultOutputParams.suggestedLatency = PortAudio.GetDeviceInfo(defaultOutputParams.device).defaultLowOutputLatency;
        defaultOutputParams.hostApiSpecificStreamInfo = IntPtr.Zero;
        return defaultOutputParams;
    }

    ~SoundFile()
    {
        dispose(disposing: false);
    }

    public void Dispose()
    {
        dispose(disposing: true);
        System.GC.SuppressFinalize(this);
    }

    protected virtual void dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                audioFile.Dispose();
                stream.Dispose();
            }

            disposed = true;
        }
    }

    public void Play(bool restart = false)
    {
        if (restart)
        {
            Cursor = 0f;
        }
        if (stream.IsStopped)
        {
            stream.Start();
        }
        playingBack = true;
    }

    public void Pause()
    {
        if (stream.IsActive)
        {
            stream.Stop();
        }
        playingBack = false;
    }

    private unsafe static StreamCallbackResult playCallback(nint input, nint output, uint frameCount, ref StreamCallbackTimeInfo timeInfo, StreamCallbackFlags statusFlags, nint dataPtr)
    {
        SoundFile userData = Stream.GetUserData<SoundFile>(dataPtr);
        long num = 0L;
        float* ptr = (float*)(void*)output;
        for (uint num2 = 0u; num2 < userData.finalFrameSize; num2++)
        {
            *(ptr++) = 0f;
        }

        if (userData.playingBack)
        {
            num = userData.audioFile.readFloat(output, userData.finalFrameSize);
            ptr = (float*)(void*)output;
            for (int i = 0; i < num; i++)
            {
                *(ptr++) *= userData.volume;
            }
        }

        userData.cursor += num;
        if (userData.playingBack && num < frameCount)
        {
            userData.playingBack = false;
        }

        return StreamCallbackResult.Continue;
    }
}