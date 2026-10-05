using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance; // Singleton. GK

    public void Awake() // GK
    {
        instance = this; // GK
    }

    public AudioSource[] soundEffects; // The sound effects. GK 

    public void PlaySFX(int sfxToPlay) // Play the sound effect. GK
    {
        if (sfxToPlay >= 0 && sfxToPlay < soundEffects.Length && soundEffects[sfxToPlay] != null)
        {
            soundEffects[sfxToPlay].PlayOneShot(soundEffects[sfxToPlay].clip); // Play smoothly without cutting off previous audio
        }
    }
    public void PlaySFXPitched(int sfxToPlay) // Play the sound effect with pitch. GK
    {
        if (sfxToPlay >= 0 && sfxToPlay < soundEffects.Length && soundEffects[sfxToPlay] != null)
        {
            soundEffects[sfxToPlay].pitch = Random.Range(.8f, 1.2f); // Set the pitch. GK
            soundEffects[sfxToPlay].PlayOneShot(soundEffects[sfxToPlay].clip);
        }
    }
}