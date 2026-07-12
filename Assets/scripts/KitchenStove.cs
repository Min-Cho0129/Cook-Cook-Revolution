using System.Collections.Generic;
using UnityEngine;

//
//
//
// NOT USED ANYMORE - switched to script per burner instead of one script for the whole stove, but keeping this here in case we want to switch back
//
//
//

[System.Serializable]
public class StoveBurner
{
    public GameObject stoveBurnerObject;
    public KitchenStoveKnob kitchenStoveKnob;
}

public class KitchenStove : MonoBehaviour
{

    public StoveBurner[] stoveBurners = new StoveBurner[5];

    void Start()
    {
        for(int i = 0; i < stoveBurners.Length; i++)
        {
            StoveBurner burner = stoveBurners[i];
            if (burner.stoveBurnerObject != null)
            {
                // disable particle system if there is one attached to this burner
                ParticleSystem particles = burner.stoveBurnerObject.GetComponentInChildren<ParticleSystem>();
                BoxCollider cookTrigger = burner.stoveBurnerObject.GetComponentInChildren<BoxCollider>();

                if (particles != null)
                    particles.Stop();

                if(cookTrigger != null)
                    cookTrigger.enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < stoveBurners.Length; i++)
        {
            StoveBurner burner = stoveBurners[i];
            if (burner.kitchenStoveKnob.isOn)
            {
                // turn on particle system if there is one attached to this burner
                ParticleSystem particles = burner.stoveBurnerObject.GetComponentInChildren<ParticleSystem>();
                BoxCollider cookTrigger = burner.stoveBurnerObject.GetComponentInChildren<BoxCollider>();
                if (particles != null && !particles.isPlaying)
                {
                    particles.Play();

                    if(cookTrigger != null)
                        cookTrigger.enabled = true;
                }
                // TODO: create/enable trigger collider for this burner to cook food
            }
            else
            {
                // turn off particle system if there is one attached to this burner
                ParticleSystem particles = burner.stoveBurnerObject.GetComponentInChildren<ParticleSystem>();
                BoxCollider cookTrigger = burner.stoveBurnerObject.GetComponentInChildren<BoxCollider>();

                if (particles != null && particles.isPlaying)
                {
                    particles.Stop();

                    if(cookTrigger != null)
                        cookTrigger.enabled = false;

                }
                // TODO: disable trigger collider
            }
        }
    }
}
