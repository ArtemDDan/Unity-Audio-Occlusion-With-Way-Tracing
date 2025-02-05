using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOcclusion : MonoBehaviour
{
    public GameObject listener; // Assign the listener (e.g., the player's camera or player object)
    public string listenerTag = "Player"; // Tag to find the listener object automatically
    public bool autoAssignListener = false; // Automatically find the listener using the tag
    public float interpolationSpeed = 1.0f;
    public bool visualizeRays = false;
    public LayerMask occludingLayers;
    public float minVolume = 0.7f; // Used as a scaling factor (0.7 = 70% of original volume)
    public float minFrequency = 500f;
    public float coneAngle = 45f;
    public int rayCount = 10;
    public bool instantApply = false; // Flag for instant application of changes

    private List<AudioSource> audioSources = new List<AudioSource>();
    private List<AudioLowPassFilter> lowPassFilters = new List<AudioLowPassFilter>();
    private List<float> originalVolumes = new List<float>();
    private List<float> targetFrequencies = new List<float>();
    private List<float> targetVolumes = new List<float>();

    void Start()
    {
        // Automatically assign listener if flag is set and the listener is not already assigned
        if (autoAssignListener && listener == null)
        {
            GameObject taggedObject = GameObject.FindWithTag(listenerTag);
            if (taggedObject != null)
            {
                listener = taggedObject;
            }
            else
            {
                Debug.LogWarning($"SoundOcclusion: No GameObject found with tag '{listenerTag}'. Please assign a valid listener.");
            }
        }

        // Find all AudioSources in children and their children
        foreach (var audioSource in GetComponentsInChildren<AudioSource>())
        {
            audioSources.Add(audioSource);
            originalVolumes.Add(audioSource.volume); // Store the original volume from the inspector

            // Add an AudioLowPassFilter if it doesn't exist
            var lowPassFilter = audioSource.GetComponent<AudioLowPassFilter>();
            if (lowPassFilter == null)
            {
                lowPassFilter = audioSource.gameObject.AddComponent<AudioLowPassFilter>();
                lowPassFilter.cutoffFrequency = 22000f; // Default frequency
            }
            lowPassFilters.Add(lowPassFilter);

            // Initialize target values for each source
            targetFrequencies.Add(22000f);
            targetVolumes.Add(audioSource.volume);
        }
    }

    void Update()
    {
        if (!listener) return;

        for (int i = audioSources.Count - 1; i >= 0; i--) // Iterate backward to safely remove items
        {
            var audioSource = audioSources[i];

            // Skip if the AudioSource or its GameObject is null/destroyed
            if (audioSource == null || audioSource.gameObject == null)
            {
                RemoveAt(i);
                continue;
            }

            Vector3 direction = audioSource.transform.position - listener.transform.position;
            float distance = direction.magnitude;

            if (distance > audioSource.maxDistance)
                continue;

            int hitCount = 0;

            for (int j = 0; j < rayCount; j++)
            {
                float angle = ((float)j / rayCount) * 360f;
                Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * Mathf.Tan(coneAngle * Mathf.Deg2Rad);

                Ray ray = new Ray(listener.transform.position, direction + offset);
                if (Physics.Raycast(ray, distance, occludingLayers))
                {
                    hitCount++;
                    if (visualizeRays)
                    {
                        Debug.DrawRay(ray.origin, ray.direction * distance, Color.red);
                    }
                }
                else if (visualizeRays)
                {
                    Debug.DrawRay(ray.origin, ray.direction * distance, Color.green);
                }
            }

            float visibility = 1.0f - (float)hitCount / rayCount;

            // Adjust target values based on visibility
            targetFrequencies[i] = Mathf.Lerp(minFrequency, 22000f, visibility);
            targetVolumes[i] = Mathf.Lerp(originalVolumes[i] * minVolume, originalVolumes[i], visibility);

            // If instant apply is active, apply changes immediately
            if (instantApply)
            {
                ApplyImmediateChanges(i);
            }
        }
    }

    void FixedUpdate()
    {
        if (instantApply) return; // Skip interpolation if instant apply is active

        for (int i = lowPassFilters.Count - 1; i >= 0; i--) // Iterate backward to safely remove items
        {
            var lowPassFilter = lowPassFilters[i];
            var audioSource = audioSources[i];

            // Skip if the LowPassFilter, AudioSource, or their GameObjects are null/destroyed
            if (lowPassFilter == null || audioSource == null || audioSource.gameObject == null)
            {
                RemoveAt(i);
                continue;
            }

            // Update the LowPassFilter and volume values
            lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, targetFrequencies[i], Time.fixedDeltaTime * interpolationSpeed);
            audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolumes[i], Time.fixedDeltaTime * interpolationSpeed);
        }
    }

    private void ApplyImmediateChanges(int index)
    {
        if (index < 0 || index >= lowPassFilters.Count) return;

        lowPassFilters[index].cutoffFrequency = targetFrequencies[index];
        audioSources[index].volume = targetVolumes[index];
    }

    private void RemoveAt(int index)
    {
        // Remove the destroyed object/component from all relevant lists
        audioSources.RemoveAt(index);
        lowPassFilters.RemoveAt(index);
        originalVolumes.RemoveAt(index);
        targetFrequencies.RemoveAt(index);
        targetVolumes.RemoveAt(index);
    }
}
