// -----------------------------------------------------------------------
// <copyright file="Speaker.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Toys
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using AdminToys;

    using Enums;

    using Exiled.API.Features.Audio;

    using Interfaces;

    using MEC;

    using Mirror;

    using UnityEngine;

    using VoiceChat;
    using VoiceChat.Codec;
    using VoiceChat.Codec.Enums;
    using VoiceChat.Networking;

    using Object = UnityEngine.Object;

    /// <summary>
    /// A wrapper class for <see cref="SpeakerToy"/>.
    /// </summary>
    public class Speaker : AdminToy, IWrapper<SpeakerToy>
    {
        private const int FrameSize = VoiceChatSettings.PacketSizePerChannel;
        private const float FrameTime = (float)FrameSize / VoiceChatSettings.SampleRate;

        private float[] frame;
        private byte[] encoded;
        private float[] resampleBuffer;

        private double resampleTime;
        private int resampleBufferFilled;

        private IPcmSource source;
        private OpusEncoder encoder;
        private CoroutineHandle playBackRoutine;

        private bool isPitchDefault = true;
        private bool isPlayBackInitialized = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Speaker"/> class.
        /// </summary>
        /// <param name="speakerToy">The <see cref="SpeakerToy"/> of the toy.</param>
        internal Speaker(SpeakerToy speakerToy)
            : base(speakerToy, AdminToyType.Speaker) => this.Base = speakerToy;

        /// <summary>
        /// Invoked when the audio playback starts.
        /// </summary>
        public event Action OnPlaybackStarted;

        /// <summary>
        /// Invoked when the audio playback is paused.
        /// </summary>
        public event Action OnPlaybackPaused;

        /// <summary>
        /// Invoked when the audio playback is resumed from a paused state.
        /// </summary>
        public event Action OnPlaybackResumed;

        /// <summary>
        /// Invoked when the audio playback loops back to the beginning.
        /// </summary>
        public event Action OnPlaybackLooped;

        /// <summary>
        /// Invoked when the audio track finishes playing.
        /// If looping is enabled, this triggers every time the track finished.
        /// </summary>
        public event Action<string> OnPlaybackFinished;

        /// <summary>
        /// Invoked when the audio playback stops completely (either manually or end of file).
        /// </summary>
        public event Action OnPlaybackStopped;

        /// <summary>
        /// Gets the prefab.
        /// </summary>
        public static SpeakerToy Prefab => PrefabHelper.GetPrefab<SpeakerToy>(PrefabType.SpeakerToy);

        /// <summary>
        /// Gets the base <see cref="SpeakerToy"/>.
        /// </summary>
        public SpeakerToy Base { get; }

        /// <summary>
        /// Gets or sets the network channel used for sending audio packets from this speaker <see cref="Channels"/>.
        /// </summary>
        public int Channel { get; set; } = Channels.ReliableOrdered2;

        /// <summary>
        /// Gets or sets a value indicating whether the audio playback should loop when it reaches the end.
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the speaker should be destroyed after playback finishes.
        /// </summary>
        public bool DestroyAfter { get; set; }

        /// <summary>
        /// Gets or sets the play mode for this speaker, determining how audio is sent to players.
        /// </summary>
        public SpeakerPlayMode PlayMode { get; set; }

        /// <summary>
        /// Gets or sets the target player who will hear the audio played by this speaker when <see cref="PlayMode"/> is set to <see cref="SpeakerPlayMode.Player"/>.
        /// </summary>
        public Player TargetPlayer { get; set; }

        /// <summary>
        /// Gets or sets the list of target players who will hear the audio played by this speaker when <see cref="PlayMode"/> is set to <see cref="SpeakerPlayMode.PlayerList"/>.
        /// </summary>
        public HashSet<Player> TargetPlayers { get; set; }

        /// <summary>
        /// Gets or sets the predicate used to determine which players will hear the audio when <see cref="PlayMode"/> is set to <see cref="SpeakerPlayMode.Predicate"/>.
        /// The predicate should return <c>true</c> for players who should receive the audio.
        /// </summary>
        public Func<Player, bool> Predicate { get; set; }

        /// <summary>
        /// Gets a value indicating whether gets is a sound playing on this speaker or not.
        /// </summary>
        public bool IsPlaying => this.playBackRoutine.IsRunning && !this.IsPaused;

        /// <summary>
        /// Gets or sets a value indicating whether the playback is paused.
        /// </summary>
        /// <value>
        /// A <see cref="bool"/> where <c>true</c> means the playback is paused; <c>false</c> means it is not paused.
        /// </value>
        public bool IsPaused
        {
            get => this.playBackRoutine.IsAliveAndPaused;
            set
            {
                if (!this.playBackRoutine.IsRunning)
                    return;

                if (this.playBackRoutine.IsAliveAndPaused == value)
                    return;

                this.playBackRoutine.IsAliveAndPaused = value;
                if (value)
                    this.OnPlaybackPaused?.Invoke();
                else
                    this.OnPlaybackResumed?.Invoke();
            }
        }

        /// <summary>
        /// Gets or sets the current playback time in seconds.
        /// Returns 0 if not playing.
        /// </summary>
        public double CurrentTime
        {
            get => this.source?.CurrentTime ?? 0.0;
            set
            {
                if (this.source != null)
                {
                    this.source.CurrentTime = value;
                    this.resampleTime = 0.0;
                    this.resampleBufferFilled = 0;
                }
            }
        }

        /// <summary>
        /// Gets the total duration of the current track in seconds.
        /// Returns 0 if not playing.
        /// </summary>
        public double TotalDuration => this.source?.TotalDuration ?? 0.0;

        /// <summary>
        /// Gets the path to the last audio file played on this speaker.
        /// </summary>
        public string LastTrack { get; private set; }

        /// <summary>
        /// Gets or sets the playback pitch.
        /// </summary>
        /// <value>
        /// A <see cref="float"/> representing the pitch level of the audio source,
        /// where 1.0 is normal pitch, less than 1.0 is lower pitch (slower), and greater than 1.0 is higher pitch (faster).
        /// </value>
        public float Pitch
        {
            get;
            set
            {
                field = Mathf.Max(0.1f, Mathf.Abs(value));
                this.isPitchDefault = Mathf.Abs(field - 1.0f) < 0.0001f;
                if (this.isPitchDefault)
                {
                    this.resampleTime = 0.0;
                    this.resampleBufferFilled = 0;
                }
            }
        }

        /// <summary>
        /// Gets or sets the volume of the audio source.
        /// </summary>
        /// <value>
        /// A <see cref="float"/> representing the volume level of the audio source,
        /// where 0.0 is silent and 1.0 is full volume if it's more it's will amplify it.
        /// </value>
        public float Volume
        {
            get => this.Base.NetworkVolume;
            set => this.Base.NetworkVolume = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the audio source is spatialized.
        /// </summary>
        /// <value>
        /// A <see cref="bool"/> where <c>true</c> means the audio source is spatial, allowing
        /// for 3D audio positioning relative to the listener; <c>false</c> means it is non-spatial.
        /// </value>
        public bool IsSpatial
        {
            get => this.Base.NetworkIsSpatial;
            set => this.Base.NetworkIsSpatial = value;
        }

        /// <summary>
        /// Gets or sets the maximum distance at which the audio source can be heard.
        /// </summary>
        /// <value>
        /// A <see cref="float"/> representing the maximum hearing distance for the audio source.
        /// Beyond this distance, the audio will not be audible.
        /// </value>
        public float MaxDistance
        {
            get => this.Base.NetworkMaxDistance;
            set => this.Base.NetworkMaxDistance = value;
        }

        /// <summary>
        /// Gets or sets the minimum distance at which the audio source reaches full volume.
        /// </summary>
        /// <value>
        /// A <see cref="float"/> representing the distance from the source at which the audio is heard at full volume.
        /// Within this range, volume will not decrease with proximity.
        /// </value>
        public float MinDistance
        {
            get => this.Base.NetworkMinDistance;
            set => this.Base.NetworkMinDistance = value;
        }

        /// <summary>
        /// Gets or sets the controller ID of speaker.
        /// </summary>
        public byte ControllerId
        {
            get => this.Base.NetworkControllerId;
            set => this.Base.NetworkControllerId = value;
        }

        /// <summary>
        /// Creates a new <see cref="Speaker"/>.
        /// </summary>
        /// <param name="position">The position of the <see cref="Speaker"/>.</param>
        /// <param name="rotation">The rotation of the <see cref="Speaker"/>.</param>
        /// <param name="scale">The scale of the <see cref="Speaker"/>.</param>
        /// <param name="spawn">Whether the <see cref="Speaker"/> should be initially spawned.</param>
        /// <returns>The new <see cref="Speaker"/>.</returns>
        public static Speaker Create(Vector3? position, Vector3? rotation, Vector3? scale, bool spawn)
        {
            Speaker speaker = new(Object.Instantiate(Prefab))
            {
                Position = position ?? Vector3.zero,
                Rotation = Quaternion.Euler(rotation ?? Vector3.zero),
                Scale = scale ?? Vector3.one,
            };

            if (spawn)
                speaker.Spawn();

            return speaker;
        }

        /// <summary>
        /// Creates a new <see cref="Speaker"/>.
        /// </summary>
        /// <param name="transform">The transform to create this <see cref="Speaker"/> on.</param>
        /// <param name="spawn">Whether the <see cref="Speaker"/> should be initially spawned.</param>
        /// <param name="worldPositionStays">Whether the <see cref="Speaker"/> should keep the same world position.</param>
        /// <returns>The new <see cref="Speaker"/>.</returns>
        public static Speaker Create(Transform transform, bool spawn, bool worldPositionStays = true)
        {
            Speaker speaker = new(Object.Instantiate(Prefab, transform, worldPositionStays))
            {
                Position = transform.position,
                Rotation = transform.rotation,
                Scale = transform.localScale.normalized,
            };

            if (spawn)
                speaker.Spawn();

            return speaker;
        }

        /// <summary>
        /// Plays audio through this speaker.
        /// </summary>
        /// <param name="message">An <see cref="AudioMessage"/> instance.</param>
        /// <param name="targets">Targets who will hear the audio. If <c>null</c>, audio will be sent to all players.</param>
        public static void Play(AudioMessage message, IEnumerable<Player> targets = null)
        {
            foreach (Player target in targets ?? Player.List)
                target.Connection.Send(message);
        }

        /// <summary>
        /// Plays audio through this speaker.
        /// </summary>
        /// <param name="samples">Audio samples.</param>
        /// <param name="length">The length of the samples array.</param>
        /// <param name="targets">Targets who will hear the audio. If <c>null</c>, audio will be sent to all players.</param>
        public void Play(byte[] samples, int? length = null, IEnumerable<Player> targets = null) => Play(new AudioMessage(this.ControllerId, samples, length ?? samples.Length), targets);

        /// <summary>
        /// Plays a wav file through this speaker.(File must be 16 bit, mono and 48khz.)
        /// </summary>
        /// <param name="path">The path to the wav file.</param>
        /// <param name="stream">Whether to stream the audio or preload it.</param>
        /// <param name="destroyAfter">Whether to destroy the speaker after playback.</param>
        /// <param name="loop">Whether to loop the audio.</param>
        public void Play(string path, bool stream = false, bool destroyAfter = false, bool loop = false)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("The specified file does not exist.", path);

            if (!path.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException($"The file type '{Path.GetExtension(path)}' is not supported. Please use .wav file.");

            this.TryInitializePlayBack();
            this.Stop();

            this.Loop = loop;
            this.LastTrack = path;
            this.DestroyAfter = destroyAfter;
            this.source = stream ? new WavStreamSource(path) : new PreloadedPcmSource(path);
            this.playBackRoutine = Timing.RunCoroutine(this.PlayBackCoroutine().CancelWith(this.GameObject));
        }

        /// <summary>
        /// Stops playback.
        /// </summary>
        public void Stop()
        {
            if (this.playBackRoutine.IsRunning)
            {
                Timing.KillCoroutines(this.playBackRoutine);
                this.OnPlaybackStopped?.Invoke();
            }

            this.source?.Dispose();
            this.source = null;
        }

        private void TryInitializePlayBack()
        {
            if (this.isPlayBackInitialized)
                return;

            this.isPlayBackInitialized = true;

            this.frame = new float[FrameSize];
            this.resampleBuffer = Array.Empty<float>();
            this.encoder = new(OpusApplicationType.Audio);
            this.encoded = new byte[VoiceChatSettings.MaxEncodedSize];

            AdminToyBase.OnRemoved += this.OnToyRemoved;
        }

        private IEnumerator<float> PlayBackCoroutine()
        {
            this.OnPlaybackStarted?.Invoke();

            this.resampleTime = 0.0;
            this.resampleBufferFilled = 0;

            float timeAccumulator = 0f;

            while (true)
            {
                timeAccumulator += Time.deltaTime;

                while (timeAccumulator >= FrameTime)
                {
                    timeAccumulator -= FrameTime;

                    if (this.isPitchDefault)
                    {
                        int read = this.source.Read(this.frame, 0, FrameSize);
                        if (read < FrameSize)
                            Array.Clear(this.frame, read, FrameSize - read);
                    }
                    else
                    {
                        this.ResampleFrame();
                    }

                    int len = this.encoder.Encode(this.frame, this.encoded);

                    if (len > 2)
                        this.SendPacket(len);

                    if (!this.source.Ended)
                        continue;

                    this.OnPlaybackFinished?.Invoke(this.LastTrack);

                    if (this.Loop)
                    {
                        this.source.Reset();
                        this.OnPlaybackLooped?.Invoke();
                        this.resampleTime = this.resampleBufferFilled = 0;
                        continue;
                    }

                    if (this.DestroyAfter)
                        this.Destroy();
                    else
                        this.Stop();

                    yield break;
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        private void ResampleFrame()
        {
            int requiredSize = (int)(FrameSize * Mathf.Abs(this.Pitch) * 2) + 10;

            if (this.resampleBuffer.Length < requiredSize)
            {
                this.resampleBuffer = new float[requiredSize];
                this.resampleTime = 0.0;
                this.resampleBufferFilled = 0;
            }

            int outputIdx = 0;

            while (outputIdx < FrameSize)
            {
                if (this.resampleBufferFilled == 0)
                {
                    int toRead = this.resampleBuffer.Length - 4;
                    int actualRead = this.source.Read(this.resampleBuffer, 0, toRead);

                    if (actualRead == 0)
                    {
                        while (outputIdx < FrameSize)
                            this.frame[outputIdx++] = 0f;
                        return;
                    }

                    this.resampleBufferFilled = actualRead;
                    this.resampleTime = 0.0;
                }

                int currentSample = (int)this.resampleTime;

                if (currentSample >= this.resampleBufferFilled - 1)
                {
                    if (this.resampleBufferFilled > 0)
                    {
                        this.resampleBuffer[0] = this.resampleBuffer[this.resampleBufferFilled - 1];

                        int toRead = this.resampleBuffer.Length - 5;
                        int actualRead = this.source.Read(this.resampleBuffer, 1, toRead);

                        if (actualRead == 0)
                        {
                            while (outputIdx < FrameSize)
                                this.frame[outputIdx++] = 0f;
                            return;
                        }

                        this.resampleBufferFilled = actualRead + 1;
                        this.resampleTime -= currentSample;
                    }
                    else
                    {
                        this.resampleBufferFilled = 0;
                    }

                    continue;
                }

                double frac = this.resampleTime - currentSample;
                float sample1 = this.resampleBuffer[currentSample];
                float sample2 = this.resampleBuffer[currentSample + 1];

                this.frame[outputIdx++] = (float)(sample1 + ((sample2 - sample1) * frac));

                this.resampleTime += this.Pitch;
            }
        }

        private void SendPacket(int len)
        {
            AudioMessage msg = new(this.ControllerId, this.encoded, len);

            switch (this.PlayMode)
            {
                case SpeakerPlayMode.Global:
                    NetworkServer.SendToReady(msg, this.Channel);
                    break;

                case SpeakerPlayMode.Player:
                    this.TargetPlayer?.Connection.Send(msg, this.Channel);
                    break;

                case SpeakerPlayMode.PlayerList:
                    using (NetworkWriterPooled writer = NetworkWriterPool.Get())
                    {
                        NetworkMessages.Pack(msg, writer);
                        ArraySegment<byte> segment = writer.ToArraySegment();

                        foreach (Player ply in this.TargetPlayers)
                        {
                            ply?.Connection.Send(segment, this.Channel);
                        }
                    }

                    break;

                case SpeakerPlayMode.Predicate:
                    using (NetworkWriterPooled writer = NetworkWriterPool.Get())
                    {
                        NetworkMessages.Pack(msg, writer);
                        ArraySegment<byte> segment = writer.ToArraySegment();

                        foreach (Player ply in Player.List)
                        {
                            if (this.Predicate(ply))
                                ply.Connection.Send(segment, this.Channel);
                        }
                    }

                    break;
            }
        }

        private void OnToyRemoved(AdminToyBase toy)
        {
            if (toy != this.Base)
                return;

            AdminToyBase.OnRemoved -= this.OnToyRemoved;

            this.Stop();

            this.encoder?.Dispose();
        }
    }
}