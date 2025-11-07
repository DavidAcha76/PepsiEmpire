using UnityEngine;

public class SealFlashFX : MonoBehaviour
{
    public ParticleSystem flashFX;

    public void PlayFlash()
    {
        if (flashFX == null) return;
        flashFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        flashFX.Play();
    }
}
