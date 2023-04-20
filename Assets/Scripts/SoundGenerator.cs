using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundGenerator : MonoBehaviour
{
//    int sampleFreq = 44000;
//    float frequency = 4400;
//
//    float[] samples = new float[44000];
    private float volume = 1.0f;
    public float frequency = 50;
    public GameObject start;
    public GameObject target4;
    public float lThreshold = 1.5f;
    public float wThreshold = 1.5f;
    private bool isPlaying = false;
    private AudioClip ac;
    public AudioClip go;
    public AudioClip done;
    public AudioClip ok;
    private AudioSource source;
    private AudioSource okSource;
    private AudioSource goSource;
    private AudioSource doneSource;

    public void Play()
    {
        isPlaying = true;
    }

    public void Stop()
    {
        isPlaying = false;
    }

    public void playDone()
    {
        Debug.Log("PLAY DONE");
        doneSource.PlayOneShot(done, 1.0F);
    }

    public void playOk()
    {
        Debug.Log("PLAY OK");
        okSource.PlayOneShot(ok, 1.0F);
    }

    public void playGo()
    {
        Debug.Log("PLAY GO");
        goSource.PlayOneShot(go, 1.0F);
    }

    // Start is called before the first frame update
    void Start()
    {
        int sampleFreq = 44100;
        float[] samples = new float[44100];
        for (int i = 0; i < samples.Length; i++)
            samples[i] = Mathf.Repeat(i * frequency / sampleFreq, 1) * 2f - 1f;
        ac = AudioClip.Create("Test", samples.Length, 1, sampleFreq, false);
        ac.SetData(samples, 0);
        source = gameObject.AddComponent<AudioSource>();
        source.clip = ac;
        source.loop = true;
        
        okSource = gameObject.AddComponent<AudioSource>();
        okSource.clip = ok;
        okSource.loop = false;
        
        doneSource = gameObject.AddComponent<AudioSource>();
        doneSource.clip = go;
        doneSource.loop = false;
        
        goSource = gameObject.AddComponent<AudioSource>();
        goSource.clip = go;
        goSource.loop = false;
    }

    private int i = 0;

    void FixedUpdate()
    {
        if (isOutOfBound(start.transform.position, target4.transform.position, gameObject.transform.position))
        {
            // if (!source.isPlaying) source.Play();
            return;
        }

        if (source.isPlaying) source.Stop();
    }

    private bool isOutOfBound(Vector3 origin, Vector3 target, Vector3 rawPosition)
    {
        float lDistance = ArrowPointer.GetProjectedDistance(origin, target, rawPosition);
        // Debug.Log("lin " + lDistance);
        if (lDistance > lThreshold)
        {
//            Debug.Log("lOut "+lDistance);
            return true;
        }

        Vector3 direction = (origin - target).normalized;
        Vector3 startingPoint = origin;

        Ray ray = new Ray(startingPoint, direction);
        float wDistance = Vector3.Cross(ray.direction, ArrowPointer.toFloorPlane(rawPosition) - ray.origin).magnitude;

        if (wDistance > wThreshold)
        {
            Debug.Log("wOut " + wDistance + " " + rawPosition);
            return true;
        }

        return false;
    }

    // Update is called once per frame
    void Update()
    {
    }
}