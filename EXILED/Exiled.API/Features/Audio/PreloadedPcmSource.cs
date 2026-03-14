// -----------------------------------------------------------------------
// <copyright file="PreloadedPcmSource.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Audio
{
    using System;

    using Exiled.API.Interfaces;

    using VoiceChat;

    /// <summary>
    /// Represents a preloaded PCM audio source.
    /// </summary>
    public sealed class PreloadedPcmSource : IPcmSource
    {
        /// <summary>
        /// The PCM data buffer.
        /// </summary>
        private readonly float[] data;

        /// <summary>
        /// The current read position in the data buffer.
        /// </summary>
        private int pos;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreloadedPcmSource"/> class.
        /// </summary>
        /// <param name="path">The path to the audio file.</param>
        public PreloadedPcmSource(string path) => this.data = WavUtility.WavToPcm(path);

        /// <summary>
        /// Initializes a new instance of the <see cref="PreloadedPcmSource"/> class.
        /// </summary>
        /// <param name="pcmData">The raw PCM float array.</param>
        public PreloadedPcmSource(float[] pcmData) => this.data = pcmData;

        /// <summary>
        /// Gets a value indicating whether the end of the PCM data buffer has been reached.
        /// </summary>
        public bool Ended => this.pos >= this.data.Length;

        /// <summary>
        /// Gets the total duration of the audio in seconds.
        /// </summary>
        public double TotalDuration => (double)this.data.Length / VoiceChatSettings.SampleRate;

        /// <summary>
        /// Gets or sets the current playback position in seconds.
        /// </summary>
        public double CurrentTime
        {
            get => (double)this.pos / VoiceChatSettings.SampleRate;
            set => this.Seek(value);
        }

        /// <summary>
        /// Reads a sequence of PCM samples from the preloaded buffer into the specified array.
        /// </summary>
        /// <param name="buffer">The destination array to copy the samples into.</param>
        /// <param name="offset">The zero-based index in <paramref name="buffer"/> at which to begin storing the data.</param>
        /// <param name="count">The maximum number of samples to read.</param>
        /// <returns>The number of samples read into <paramref name="buffer"/>.</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            int read = Math.Min(count, this.data.Length - this.pos);
            Array.Copy(this.data, this.pos, buffer, offset, read);
            this.pos += read;

            return read;
        }

        /// <summary>
        /// Seeks to the specified position in seconds.
        /// </summary>
        /// <param name="seconds">The target position in seconds.</param>
        public void Seek(double seconds)
        {
            long targetIndex = (long)(seconds * VoiceChatSettings.SampleRate);

            if (targetIndex < 0)
                targetIndex = 0;

            if (targetIndex > this.data.Length)
                targetIndex = this.data.Length;

            this.pos = (int)targetIndex;
        }

        /// <summary>
        /// Resets the read position to the beginning of the PCM data buffer.
        /// </summary>
        public void Reset() => this.pos = 0;

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}