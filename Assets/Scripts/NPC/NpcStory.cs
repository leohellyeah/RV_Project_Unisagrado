using System.Collections;
using UnityEngine;
using ProjetoRV.Systems;

namespace ProjetoRV.NPC
{
    public class NpcStory : MonoBehaviour, IInteractable
    {
        public enum StoryState
        {
            Idle,           // not playing, ready to start
            PlayingStory,   // story is playing
            Paused,         // story paused, waiting for next interaction
            PlayingSfx      // playing an interrupt/resume SFX (ignore inputs)
        }

        [Header("Story Audio")]
        public AudioClip storyClip;

        [Header("SFX (NPC Voice Reactions)")]
        [Tooltip("Played when the player interrupts the story (e.g., 'Yes, any questions?').")]
        public AudioClip interruptSfx;

        [Tooltip("Played before resuming the story (e.g., 'Alright, continuing...').")]
        public AudioClip resumeSfx;

        [Header("Audio Settings")]
        public bool spatial3D = true;

        [Tooltip("Volume base do NPC (0-1). Suba/baixe pra equalizar as vozes.")]
        [Range(0f, 1f)] public float volume = 1f;

        [Tooltip("Distância (m) em que o áudio ainda toca no volume máximo.")]
        public float hearFullDistance = 7f;

        [Tooltip("Distância (m) além da qual o áudio some por completo.")]
        public float hearMaxDistance = 40f;

        [Header("Behavior")]
        [Tooltip("If true, after the story ends, a new interaction starts it from the beginning.")]
        public bool restartAfterEnd = true;

        private AudioSource storySource;
        private AudioSource sfxSource;

        private StoryState state = StoryState.Idle;

        void Awake()
        {
            // Create / get story source
            storySource = GetComponent<AudioSource>();
            if (!storySource) storySource = gameObject.AddComponent<AudioSource>();

            // Create separate SFX source to avoid messing with story playback time
            sfxSource = gameObject.AddComponent<AudioSource>();

            ConfigureSource(storySource);
            ConfigureSource(sfxSource);

            storySource.playOnAwake = false;
            sfxSource.playOnAwake = false;

            if (storyClip != null)
                storySource.clip = storyClip;
        }

        void ConfigureSource(AudioSource src)
        {
            src.spatialBlend = spatial3D ? 1f : 0f;
            src.volume = volume;
            src.dopplerLevel = 0f;                       // sem alteracao de tom ao andar
            src.rolloffMode = AudioRolloffMode.Linear;   // queda previsivel (nao despenca apos 1m)
            src.minDistance = hearFullDistance;          // volume cheio dentro desse raio
            src.maxDistance = hearMaxDistance;           // some alem desse raio
        }

        public void Interact()
        {
            if (!storyClip) return;

            // Ignore interactions while reaction SFX is playing
            if (state == StoryState.PlayingSfx) return;

            // If story is playing -> interrupt
            if (state == StoryState.PlayingStory && storySource.isPlaying)
            {
                InterruptStory();
                return;
            }

            // If story is paused -> resume flow
            if (state == StoryState.Paused)
            {
                // If still playing any previous SFX (safety), ignore
                if (sfxSource.isPlaying) return;

                StartCoroutine(ResumeStoryRoutine());
                return;
            }

            // Idle or story ended -> start from beginning
            if (state == StoryState.Idle)
            {
                StartStoryFromBeginning();
                return;
            }

            // If story ended naturally but state didn't go Idle yet, handle it
            if (!storySource.isPlaying && storySource.time > 0f)
            {
                // ended or stopped
                if (restartAfterEnd) StartStoryFromBeginning();
            }
        }

        void StartStoryFromBeginning()
        {
            // Ensure sources
            storySource.clip = storyClip;

            storySource.Stop();
            storySource.time = 0f;
            storySource.Play();

            state = StoryState.PlayingStory;
            StartCoroutine(WatchForStoryEnd());
        }

        void InterruptStory()
        {
            // Pause keeps the current time so we can resume later
            storySource.Pause();

            // Play interrupt reaction SFX
            if (interruptSfx)
            {
                state = StoryState.PlayingSfx;
                sfxSource.PlayOneShot(interruptSfx);
                StartCoroutine(AfterSfxSetPaused());
            }
            else
            {
                // If no interrupt SFX, just go to paused state
                state = StoryState.Paused;
            }
        }

        IEnumerator AfterSfxSetPaused()
        {
            // Wait until SFX ends
            while (sfxSource.isPlaying) yield return null;

            // Now we are waiting for next interaction
            state = StoryState.Paused;
        }

        IEnumerator ResumeStoryRoutine()
        {
            // Play resume SFX first
            if (resumeSfx)
            {
                state = StoryState.PlayingSfx;
                sfxSource.PlayOneShot(resumeSfx);

                while (sfxSource.isPlaying) yield return null;
            }

            // Resume story from where it was paused
            storySource.UnPause();
            state = StoryState.PlayingStory;

            // Continue monitoring end
            StartCoroutine(WatchForStoryEnd());
        }

        IEnumerator WatchForStoryEnd()
        {
            // Wait until story finishes OR gets paused/interrupted
            while (state == StoryState.PlayingStory && storySource.isPlaying)
                yield return null;

            // If it stopped naturally (not paused), set idle
            if (state == StoryState.PlayingStory && !storySource.isPlaying)
            {
                state = StoryState.Idle;
                // Reset time so next start is clean (optional)
                // storySource.time = 0f;
            }
        }
    }
}