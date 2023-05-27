using UnityEngine;


public class KeyboardSFX : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;


    [SerializeField]
    private AudioClip[] keyPressSounds;


    public void PlayKeyPressSFX(float pitch = 1.0f)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.pitch = pitch;
            //var soundIndex = Random.Range(0, keyPressSounds.Length - 1);
            var soundIndex = 1;
            var sound = keyPressSounds[0];
            //audioSource.clip = sound;
            audioSource.PlayOneShot(sound);
        }
    }

    public void PlayKeyPressPitchSFX(string text)
    {
        var value = int.Parse(text);
        var percent = value / 10.0f;
        const float basePitch = 0.9f;
        const float pitchRange = 0.2f;

        var pitch = basePitch + percent * pitchRange;
        
        PlayKeyPressSFX(pitch);
    }
    
}