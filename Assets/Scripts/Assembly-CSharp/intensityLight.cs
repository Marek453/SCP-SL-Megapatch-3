using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class intensityLight : NetworkBehaviour
{
    public float intensity;
    public AudioSource source;
    public AudioClip clip;
    public Light light;
    public bool isBlinking;
    private void InitAudio()
    {
        source = base.gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.clip = clip;
        source.loop = false;
        source.priority = 128;
        source.volume = 0.3f;
        source.spatialBlend = 1;
        source.reverbZoneMix = 1;
        source.dopplerLevel = 0;
        source.spread = 0;
        source.rolloffMode = AudioRolloffMode.Custom;
        source.maxDistance = 55;
    }
    private void Start()
    {
        InitAudio();
        light = GetComponent<Light>();
        intensity = light.intensity;
    }
    [ClientRpc]
    public void RpcBlink(int time)
    {
        isBlinking = true;
        StartCoroutine(blink());
        Invoke("Stop", time);
    }

    private IEnumerator blink()
    {
        while (isBlinking)
        {
            light.intensity = Random.Range(0, intensity);
            yield return new WaitForSeconds(0.05f);
        }
    }
    private void Stop()
    {
        if (DisableLightFacility.disableLightFacility.canToDisable > 25)
        {
            light.intensity = 0;
            if (PlayerManager.localPlayer.GetComponent<CharacterClassManager>().curClass == 18)
            {
                PlayerManager.localPlayer.GetComponent<VeryHighPerformance>().Online();
            }
        }
        else
        {
            light.intensity = intensity;
            if (PlayerManager.localPlayer.GetComponent<CharacterClassManager>().curClass == 18)
            {
                PlayerManager.localPlayer.GetComponent<VeryHighPerformance>().OffLineLight();
            }
        }
        if (source != null)
        {
            source.Play();
        }
        isBlinking = false;
        CancelInvoke();
    }
}
