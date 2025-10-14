using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    private AudioSource audioSource;

    public AudioClip rightStep;
    public AudioClip leftStep;

    private void Awake()
    {
        audioSource = GetComponentInChildren<AudioSource>();
    }

    public void PlayRightStep()
    {
        audioSource.PlayOneShot(rightStep);
    }

    public void PlayLeftStep()
    {
        audioSource.PlayOneShot(leftStep);
    }
}
