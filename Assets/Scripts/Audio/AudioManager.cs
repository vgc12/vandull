using System;
using System.Collections.Generic;
using System.Threading;
using Attributes;
using Cysharp.Threading.Tasks;
using EventBus;
using Singletons;
using UI.States;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

namespace Audio
{
    /// <summary>
    ///     Manages audio playback with object pooling and easy mixer group configuration.
    ///     Supports multiple audio categories with customizable settings per category.
    /// </summary>
    public class AudioManager : PersistentSingleton<AudioManager>
    {
        #region Public API - Main Play Method

        /// <summary>
        ///     Plays audio with the specified category configuration.
        ///     This is the main method - all other Play methods use this internally.
        /// </summary>
        /// <param name="clip">Audio clip to play.</param>
        /// <param name="categoryName">Name of the audio category (e.g., "SFX", "UI", "Voice").</param>
        /// <param name="position">World position for 3D audio.</param>
        /// <param name="volume">Volume multiplier (0-1).</param>
        /// <param name="pitch">Pitch multiplier (default 1).</param>
        /// <param name="loop">Whether the audio should loop.</param>
        /// <returns>Handle to the playing audio source, or null if pool is exhausted.</returns>
        public PooledAudioSource PlaySound(
            AudioClip clip,
            string categoryName,
            Vector3 position = default,
            float volume = 1f,
            float pitch = 1f,
            bool loop = false)
        {
            if (clip == null)
            {
                Debug.LogWarning("AudioManager: Attempted to play null AudioClip");
                return null;
            }

            if (!_categoryLookup.TryGetValue(categoryName, out var category))
            {
                Debug.LogWarning($"AudioManager: Category '{categoryName}' not found. Using default settings.");
                category = defaultCategory;
            }

            var pooledSource = _audioPool.Get();
            if (pooledSource == null)
            {
                Debug.LogWarning("AudioManager: Audio pool exhausted!");
                return null;
            }

            // Apply category configuration
            var audioSource = pooledSource.AudioSource;
            category.ApplyTo(audioSource);

            // Set playback parameters
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = loop;
            pooledSource.transform.position = position;

            audioSource.Play();

            // Auto-return to pool when finished (only if not looping)
            if (!loop) pooledSource.ReturnToPoolWhenFinished(this.GetCancellationTokenOnDestroy()).Forget();

            return pooledSource;
        }

        #endregion

        #region Initialization

        /// <summary>
        ///     Initializes the audio pool, music source, and category lookup.
        /// </summary>
        private void InitializeAudioManager()
        {
            _audioSettingsChangedBinding = new EventBinding<SettingsUIState.AudioSettingsChangedEvent>(OnAudioSettingsChanged);
            EventBus<SettingsUIState.AudioSettingsChangedEvent>.Register(_audioSettingsChangedBinding);

            // Create parent for pooled objects
            _poolParent = new GameObject("AudioSourcePool").transform;
            _poolParent.SetParent(transform);

            // Build category lookup dictionary
            _categoryLookup = new Dictionary<string, AudioCategory>();
            foreach (var category in audioCategories)
                if (!string.IsNullOrEmpty(category.name))
                    _categoryLookup[category.name] = category;

            // Create music source (not pooled)
            var musicGameObject = new GameObject("MusicSource");
            musicGameObject.transform.SetParent(transform);
            _musicSource = musicGameObject.AddComponent<AudioSource>();
            _musicSource.outputAudioMixerGroup = musicMixerGroup;
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = 1f;
            _musicSource.spatialBlend = 0f;

            // Initialize object pool
            _audioPool = new ObjectPool<PooledAudioSource>(
                CreatePooledAudioSource,
                OnGetFromPool,
                OnReleaseToPool,
                OnDestroyPoolObject,
                true,
                initialPoolSize,
                maxPoolSize
            );

            // Pre-warm the pool
            var prewarmList = new List<PooledAudioSource>(initialPoolSize);
            for (var i = 0; i < initialPoolSize; i++) prewarmList.Add(_audioPool.Get());
            foreach (var source in prewarmList) _audioPool.Release(source);

            Debug.Log($"AudioManager initialized with {audioCategories.Length} categories");
        }

        private void OnAudioSettingsChanged(SettingsUIState.AudioSettingsChangedEvent obj)
        {
            SetVolume(musicMixerGroup, obj.MusicVolume);

            foreach (var category in audioCategories) SetVolume(category.mixerGroup, obj.MusicVolume);
        }

        #endregion

        #region Configuration

        [Header("Audio Categories")]
        [SerializeField]
        [Required]
        [Tooltip("Define different audio categories (SFX, UI, Voice, etc.) with their mixer groups")]
        private AudioCategory[] audioCategories;

        [Header("Pool Settings")] [SerializeField] [Tooltip("Initial number of AudioSources to create in the pool")]
        private int initialPoolSize = 15;

        [SerializeField] [Tooltip("Maximum number of AudioSources that can exist")]
        private int maxPoolSize = 40;
        

        [Header("Music (Non-Pooled)")] [SerializeField] [Tooltip("Mixer group for background music")]
        private AudioMixerGroup musicMixerGroup;
        

        [Header("Debug")] [SerializeField] private AudioCategory defaultCategory;

        #endregion

        #region Private Fields

        private ObjectPool<PooledAudioSource> _audioPool;
        private readonly List<PooledAudioSource> _activeAudioSources = new();
        private Dictionary<string, AudioCategory> _categoryLookup;
        private AudioSource _musicSource;
        private Transform _poolParent;
        private EventBinding<SettingsUIState.AudioSettingsChangedEvent> _audioSettingsChangedBinding;

        #endregion


        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

            InitializeAudioManager();
        }

        private void OnDestroy()
        {
            _audioPool?.Clear();
        }

        #endregion

        #region Pool Callbacks

        private PooledAudioSource CreatePooledAudioSource()
        {
            var go = new GameObject("PooledAudioSource");
            go.transform.SetParent(_poolParent);

            var pooledSource = go.AddComponent<PooledAudioSource>();
            pooledSource.Initialize(this);

            return pooledSource;
        }

        private void OnGetFromPool(PooledAudioSource pooledSource)
        {
            pooledSource.gameObject.SetActive(true);
            _activeAudioSources.Add(pooledSource);
        }

        private void OnReleaseToPool(PooledAudioSource pooledSource)
        {
            pooledSource.Reset();
            pooledSource.gameObject.SetActive(false);
            _activeAudioSources.Remove(pooledSource);
        }

        private void OnDestroyPoolObject(PooledAudioSource pooledSource)
        {
            if (pooledSource != null && pooledSource.gameObject != null) Destroy(pooledSource.gameObject);
        }

        #endregion

        #region Public API - Convenience Methods by Category

        /// <summary>
        ///     Plays a 3D sound effect using the "SoundEffects" category.
        /// </summary>
        public PooledAudioSource PlaySfx(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            return PlaySound(clip, "SoundEffects", position, volume, pitch);
        }

        /// <summary>
        ///     Plays a 2D UI sound using the "UI" category.
        /// </summary>
        public PooledAudioSource PlayUI(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            return PlaySound(clip, "UI", Vector3.zero, volume, pitch);
        }

        /// <summary>
        ///     Plays a voice line using the "Voice" category.
        /// </summary>
        public PooledAudioSource PlayVoice(AudioClip clip, Vector3 position, float volume = 1f)
        {
            return PlaySound(clip, "Dialogue", position, volume);
        }

        /// <summary>
        ///     Plays an ambient sound using the "Ambient" category.
        /// </summary>
        public PooledAudioSource PlayAmbient(AudioClip clip, Vector3 position, float volume = 1f)
        {
            return PlaySound(clip, "Ambient", position, volume);
        }

        /// <summary>
        ///     Plays a looping sound with the specified category. Must be manually stopped.
        /// </summary>
        public PooledAudioSource PlayLooping(AudioClip clip, string categoryName, Vector3 position, float volume = 1f)
        {
            return PlaySound(clip, categoryName, position, volume, 1f, true);
        }

        /// <summary>
        ///     Stops and returns a pooled audio source to the pool.
        /// </summary>
        public void StopSound(PooledAudioSource pooledSource)
        {
            if (pooledSource == null) return;

            pooledSource.AudioSource.Stop();
            _audioPool.Release(pooledSource);
        }

        /// <summary>
        ///     Stops all currently playing sounds.
        /// </summary>
        public void StopAllSounds()
        {
            // Create a copy to avoid modification during iteration
            var activeCopy = new List<PooledAudioSource>(_activeAudioSources);
            foreach (var source in activeCopy) StopSound(source);
        }

        #endregion

        #region Public API - Music

        /// <summary>
        ///     Plays background music with optional fade-in.
        /// </summary>
        public async UniTask PlayMusic(AudioClip clip, float fadeInDuration = 1f, CancellationToken ct = default)
        {
            if (clip == null) return;

            // Fade out current music
            if (_musicSource.isPlaying) await FadeOut(_musicSource, 0.5f, ct);

            _musicSource.clip = clip;
            _musicSource.volume = 0f;
            _musicSource.Play();

            // Fade in new music
            await FadeIn(_musicSource, 1f, fadeInDuration, ct);
        }

        /// <summary>
        ///     Stops the current music with optional fade-out.
        /// </summary>
        public async UniTask StopMusic(float fadeOutDuration = 1f, CancellationToken ct = default)
        {
            if (!_musicSource.isPlaying) return;

            await FadeOut(_musicSource, fadeOutDuration, ct);
            _musicSource.Stop();
        }

        /// <summary>
        ///     Pauses the current music.
        /// </summary>
        public void PauseMusic()
        {
            _musicSource.Pause();
        }

        /// <summary>
        ///     Resumes paused music.
        /// </summary>
        public void ResumeMusic()
        {
            _musicSource.UnPause();
        }

        #endregion

        #region Volume Control

        /// <summary>
        ///     Sets the master volume for all sounds.
        /// </summary>
        public void SetVolume(AudioMixerGroup mixerGroup, float volume)
        {
            mixerGroup.audioMixer.SetFloat("Volume", Mathf.Log10(Mathf.Clamp01(volume * .01f)) * 20f);
        }

        /// <summary>
        ///     Gets a category by name for runtime modification.
        /// </summary>
        public AudioCategory GetCategory(string categoryName)
        {
            return _categoryLookup.TryGetValue(categoryName, out var category) ? category : null;
        }

        #endregion

        #region Fade Utilities

        private async UniTask FadeIn(AudioSource source, float targetVolume, float duration, CancellationToken ct)
        {
            var elapsed = 0f;
            var startVolume = source.volume;

            while (elapsed < duration)
            {
                ct.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            source.volume = targetVolume;
        }

        private async UniTask FadeOut(AudioSource source, float duration, CancellationToken ct)
        {
            var elapsed = 0f;
            var startVolume = source.volume;

            while (elapsed < duration)
            {
                ct.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            source.volume = 0f;
        }

        #endregion


        #region Debug Info

        /// <summary>
        ///     Gets the number of currently active audio sources.
        /// </summary>
        public int ActiveAudioSourceCount => _activeAudioSources.Count;

        /// <summary>
        ///     Gets all available category names.
        /// </summary>
        public string[] GetCategoryNames()
        {
            var names = new string[audioCategories.Length];
            for (var i = 0; i < audioCategories.Length; i++) names[i] = audioCategories[i].name;
            return names;
        }

        #endregion
    }

    #region Pooled Audio Source

    /// <summary>
    ///     Wrapper component for pooled audio sources.
    ///     Handles automatic return to pool when audio finishes.
    /// </summary>
    public class PooledAudioSource : MonoBehaviour
    {
        private AudioSource _audioSource;
        private AudioManager _manager;

        public AudioSource AudioSource
        {
            get
            {
                if (!_audioSource)
                {
                    _audioSource = GetComponent<AudioSource>();
                    if (!_audioSource)
                    {
                        _audioSource = gameObject.AddComponent<AudioSource>();
                        _audioSource.playOnAwake = false;
                    }
                }

                return _audioSource;
            }
        }

        public void Reset()
        {
            if (_audioSource == null) return;
            _audioSource.Stop();
            _audioSource.clip = null;
            _audioSource.loop = false;
            _audioSource.pitch = 1f;
            _audioSource.volume = 1f;
            transform.position = Vector3.zero;
        }

        public void Initialize(AudioManager manager)
        {
            _manager = manager;
        }

        public async UniTaskVoid ReturnToPoolWhenFinished(CancellationToken ct)
        {
            try
            {
                // Wait until audio is no longer playing
                while (_audioSource.isPlaying) await UniTask.Yield(PlayerLoopTiming.Update, ct);

                // Return to pool
                _manager.StopSound(this);
            }
            catch (OperationCanceledException)
            {
                // Object was destroyed, that's fine
            }
        }

        public void Stop()
        {
            _manager.StopSound(this);
        }
    }

    #endregion
}