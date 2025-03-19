using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Anaglyph.DisplayCapture.Barcodes
{
    public class AnimationTrigger : MonoBehaviour
{
   [SerializeField] private Animator animator; // Reference to the Animator component
    // [SerializeField] private UnityEvent onAnimationComplete; // Event to trigger when animation finishes

    private float currentAnimationDuration; // Duration of the currently playing animation
    private float timer; // Tracks elapsed time
    private bool isPlaying; // Tracks if an animation is currently playing
    private int lastStateID; // Tracks the last played state to detect state changes

    void Start()
    {
        // Ensure the Animator is assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator component not found!", this);
                enabled = false;
                return;
            }
        }
    }

    void Update()
    {
        // Get the current state info from the base layer (layer 0)
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Check if the state has changed (new animation started)
        if (stateInfo.fullPathHash != lastStateID)
        {
            // New animation detected
            lastStateID = stateInfo.fullPathHash;
            isPlaying = true;
            timer = 0f;

            // Calculate the duration of the current animation
            currentAnimationDuration = GetCurrentAnimationDuration(stateInfo);
        }

        // If an animation is playing, increment the timer
        if (isPlaying)
        {
            timer += Time.deltaTime;

            // Check if the animation duration has been reached
            if (timer >= currentAnimationDuration)
            {
                // Trigger the event
                Debug.Log("Triggering Intro Audio Clip");
                //onAnimationComplete?.Invoke();

                SimulationManager.Instance.TriggerIntro();

                StartCoroutine(QueueReassembly());



                

                // Reset tracking
                isPlaying = false;
                timer = 0f;
            }
        }
    }

    IEnumerator QueueReassembly()
    {
        yield return new WaitForSeconds(15);

        SimulationManager.Instance.TriggerReAssembly();

        SimulationUI.Instance.TurnOnViewFinder();

        SimulationManager.Instance.EnableTracking();

        StopCoroutine(QueueReassembly());
    }

    // Function to get the duration of the currently playing animation
    private float GetCurrentAnimationDuration(AnimatorStateInfo stateInfo)
    {
        float duration = 0f;

        // Loop through all clips in the Animator Controller to find the matching one
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        foreach (AnimationClip clip in controller.animationClips)
        {
            // Use the state's speed to calculate the effective duration
            duration = clip.length / stateInfo.speed;
            break; // Assuming we want the first clip for simplicity; adjust if needed
        }

        if (duration == 0f)
        {
            Debug.LogWarning("Could not determine animation duration. Using default value of 1 second.", this);
            duration = 1f; // Fallback duration
        }

        return duration;
    }

    // Optional: Public method to manually restart tracking
    public void ResetTimer()
    {
        isPlaying = false;
        timer = 0f;
        lastStateID = 0;
    }
}

}