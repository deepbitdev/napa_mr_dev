using System.Collections.Generic;
using Anaglyph.DisplayCapture;
using Anaglyph.DisplayCapture.Barcodes;
using UnityEngine;


public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance;

    public BarcodeReader barcodeReader;


    private AudioSource audioSource;

    public enum SimulationEvent
    {
        Intro,
        BGMusic,
        ReAssembly,
        SelectBrakeFan,
        SelectTorqueWrench,
        SelectStandardWrench,
        WrongAnswerFx,
        CorrectAnswerFx,
        Completion
    }

    [System.Serializable]
    public class AudioClipEntry
    {
        public SimulationEvent eventType;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [SerializeField] private List<AudioClipEntry> audioClips = new List<AudioClipEntry>();

    private Dictionary<SimulationEvent, List<AudioClipEntry>> eventToClips;

    private HashSet<SimulationEvent> playedEvents;

    void Start()
    {
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        if(audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        eventToClips = new Dictionary<SimulationEvent, List<AudioClipEntry>>();
        playedEvents = new HashSet<SimulationEvent>();

        foreach(SimulationEvent evt in System.Enum.GetValues(typeof(SimulationEvent)))
        {
            eventToClips[evt] = new List<AudioClipEntry>();
        }

        foreach(var entry in audioClips)
        {
            if(entry.clip != null)
            {
                eventToClips[entry.eventType].Add(entry);
            }
        }

        SimulationUI.Instance.SimulationStartText();
    }

    public void PlayAudioForEvent(SimulationEvent eventType)
    {

         // Allow WrongAnswerFx to replay every time; skip the playedEvents check for it
        if (eventType != SimulationEvent.WrongAnswerFx && playedEvents.Contains(eventType))
        {
            Debug.Log($"Audio for {eventType} has already been played; skipping.");
            return;
        }


        if(eventToClips.ContainsKey(eventType) && eventToClips[eventType].Count > 0)
        {
            var clipsForEvent = eventToClips[eventType];

            AudioClipEntry selectedEntry = clipsForEvent[Random.Range(0, clipsForEvent.Count)];

            if(audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("Stopped currently playing audio to prevent overlap.");
            }

            audioSource.PlayOneShot(selectedEntry.clip, selectedEntry.volume);
            // playedEvents.Add(eventType);
            Debug.Log($"Played audio for {eventType}: {selectedEntry.clip.name}, volume: {selectedEntry.volume}");

             // Only add to playedEvents if it’s not WrongAnswerFx
            if (eventType != SimulationEvent.WrongAnswerFx)
            {
                playedEvents.Add(eventType);
            }
        }
        else
        {
            Debug.LogWarning($"No audio clips found for event: {eventType}");
        }
    }

    public void DisableTracking()
    {
        barcodeReader.enabled = false;
    }

    public void EnableTracking()
    {
        barcodeReader.enabled = true;
    }

    public void TriggerIntro()
    {
        PlayAudioForEvent(SimulationEvent.Intro);
    }

    public void TriggerReAssembly()
    {
        PlayAudioForEvent(SimulationEvent.ReAssembly);
    }

    public void TriggerWrongAnswerOne()
    {
        PlayAudioForEvent(SimulationEvent.SelectBrakeFan);
    }

    public void TriggerWrongAnswerTwo()
    {
        PlayAudioForEvent(SimulationEvent.SelectTorqueWrench);
    }

    public void TriggerCorrectAnswer()
    {
        PlayAudioForEvent(SimulationEvent.SelectStandardWrench);
    }

    public void TriggerCompletion()
    {
        PlayAudioForEvent(SimulationEvent.Completion);
    }

    public void WrongAnswerSound()
    {
        PlayAudioForEvent(SimulationEvent.WrongAnswerFx);
    }

    public void CorrectAnswerSound()
    {
        PlayAudioForEvent(SimulationEvent.CorrectAnswerFx);
    }

    public void PlayBGMusic()
    {
        PlayAudioForEvent(SimulationEvent.BGMusic);
    }

    public void ResetState()
    {
        // Reset any internal state (e.g., flags, timers)
        EnableTracking(); // Ensure tracking is enabled for a fresh start
        playedEvents.Clear();
        if(audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("Stopped audio during reset.");
        }
        // Stop any sounds if playing
        // Reset any other state as needed
    }


    public void AddAudioClip(SimulationEvent eventType, AudioClip clip, float volume = 1)
    {
        AudioClipEntry newEntry = new AudioClipEntry
        {
            eventType = eventType,
            clip = clip,
            volume = volume
        };

        audioClips.Add(newEntry);
        eventToClips[eventType].Add(newEntry);
    }
}
