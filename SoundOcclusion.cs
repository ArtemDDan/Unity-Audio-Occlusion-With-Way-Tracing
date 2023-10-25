using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundOcclusion : MonoBehaviour
{
    public GameObject listener;
    public GameObject soundObject;
    //Multiple LPF array uncomment if needed
    //public AudioLowPassFilter[] lowPassFilters;
    public AudioLowPassFilter lowPassFilter;
    public AudioSource[] audioSources;
    public float interpolationSpeed = 1.0f;
    public bool visualizeRays = false;
    public LayerMask occludingLayers;
    public float minVolume = 0f;
    public float minFrequency = 500f;
    public float coneAngle = 45f; 
    public int rayCount = 10; 
    private float targetFrequency = 22000f;
    private float targetVolume = 1f;

    void Start()
    {
        audioSources = GetComponents<AudioSource>();
        //Multiple LPFs uncomment if needed
        //lowPassFilters = GetComponents<AudioLowPassFilter>();
    }

    void Update()
    {
        Vector3 direction = soundObject.transform.position - listener.transform.position;
        float distance = direction.magnitude;

        float maxDistance = 0f;
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource.maxDistance > maxDistance)
            {
                maxDistance = audioSource.maxDistance;
            }
        }

        if (distance > maxDistance)
        {
            return;
        }

        int hitCount = 0;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = ((float)i / rayCount) * 360f;
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * Mathf.Tan(coneAngle * Mathf.Deg2Rad);

            //Randomly generated rays;
            //Vector3 offset = Random.insideUnitSphere * Mathf.Tan(coneAngle * Mathf.Deg2Rad); 
        
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
        targetFrequency = Mathf.Lerp(minFrequency, 22000f, visibility);
        targetVolume = Mathf.Lerp(minVolume, 1f, visibility);
    }

    void FixedUpdate()
    {
        //Multiple LPF options uncomment if needed 
        //foreach (AudioLowPassFilter lowPassFilter in lowPassFilters)
        //{
        //    lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, targetFrequency, Time.fixedDeltaTime * interpolationSpeed);
        //}
        lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, targetFrequency, Time.fixedDeltaTime * interpolationSpeed);
        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.fixedDeltaTime * interpolationSpeed);
        }
    }
}

