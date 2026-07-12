using UnityEngine;
using System.Collections.Generic;

public class FryingPan : MonoBehaviour
{
    public bool onFlame = false;

    [Header("Audio")]
    public AudioSource sizzleAudioSource;
    public AudioSource sfxAudioSource;
    public AudioClip sizzleLoop;
    public AudioClip cookingStageClip;

    [Header("VFX")]
    public ParticleSystem sizzleParticles;

    [SerializeField]
    List<KitchenStoveBurner> burnersTouching = new List<KitchenStoveBurner>();

    private HashSet<CookableIngredient> ingredientsOnPan = new HashSet<CookableIngredient>();

    void Update()
    {
        ingredientsOnPan.RemoveWhere(i => i == null);

        for (int i = 0; i < burnersTouching.Count; i++)
        {
            if (burnersTouching[i] != null && burnersTouching[i].kitchenStoveKnob.isOn)
            {
                onFlame = true;
                UpdateSizzle();
                return;
            }
        }

        onFlame = false;
        UpdateSizzle();
    }

    void UpdateSizzle()
    {
        bool shouldSizzle = onFlame && ingredientsOnPan.Count > 0;

        if (shouldSizzle)
        {
            if (!sizzleAudioSource.isPlaying)
            {
                sizzleAudioSource.clip = sizzleLoop;
                sizzleAudioSource.loop = true;
                sizzleAudioSource.Play();
            }
            if (sizzleParticles != null && !sizzleParticles.isPlaying)
                sizzleParticles.Play();
        }
        else
        {
            if (sizzleAudioSource.isPlaying && sizzleAudioSource.clip == sizzleLoop)
                sizzleAudioSource.Stop();
            if (sizzleParticles != null && sizzleParticles.isPlaying)
                sizzleParticles.Stop();
        }
    }

    void OnDisable()
    {
        burnersTouching.Clear();
        ingredientsOnPan.Clear();
        onFlame = false;
        sizzleAudioSource.Stop();
        if (sizzleParticles != null) sizzleParticles.Stop();
    }

    void OnTriggerStay(Collider other)
    {
        CookableIngredient ingredient = other.GetComponent<CookableIngredient>();
        if (ingredient != null && onFlame)
        {
            bool advanced = ingredient.Cook(Time.fixedDeltaTime);
            if (advanced)
                sfxAudioSource.PlayOneShot(cookingStageClip);
        }

        EggLiquid egg = other.GetComponent<EggLiquid>();
        if (egg != null)
            egg.SetOnHotPan(onFlame);

        HotPanSizzle sizzle = other.GetComponent<HotPanSizzle>();
        if (sizzle != null)
            sizzle.SetOnHotPan(onFlame);
    }

    void OnTriggerEnter(Collider other)
    {
        KitchenStoveBurner burner = other.GetComponent<KitchenStoveBurner>();
        if (burner != null && other.isTrigger)
        {
            if (!burnersTouching.Contains(burner))
                burnersTouching.Add(burner);
        }

        CookableIngredient ingredient = other.GetComponent<CookableIngredient>();
        if (ingredient != null)
            ingredientsOnPan.Add(ingredient);
    }

    void OnTriggerExit(Collider other)
    {
        KitchenStoveBurner burner = other.GetComponent<KitchenStoveBurner>();
        if (burner != null && other.isTrigger)
            burnersTouching.Remove(burner);

        CookableIngredient ingredient = other.GetComponent<CookableIngredient>();
        if (ingredient != null)
            ingredientsOnPan.Remove(ingredient);

        EggLiquid egg = other.GetComponent<EggLiquid>();
        if (egg != null)
            egg.SetOnHotPan(false);

        HotPanSizzle sizzle = other.GetComponent<HotPanSizzle>();
        if (sizzle != null)
            sizzle.SetOnHotPan(false);
    }
}