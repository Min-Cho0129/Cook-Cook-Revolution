using System.Collections.Generic;
using UnityEngine;

public class KitchenStoveBurner : MonoBehaviour
{

    public KitchenStoveKnob kitchenStoveKnob;

    ParticleSystem particles;

   void Start()
    {
        particles = GetComponentInChildren<ParticleSystem>();
        if (particles != null)
            particles.Stop();
    }

    void Update()
    {
        if (particles == null) return;
        
        if (kitchenStoveKnob.isOn && !particles.isPlaying)
            particles.Play();
        else if (!kitchenStoveKnob.isOn && particles.isPlaying)
            particles.Stop();
    }
    
}