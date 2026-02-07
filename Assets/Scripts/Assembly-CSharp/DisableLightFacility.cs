using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DisableLightFacility : NetworkBehaviour
{
    public static DisableLightFacility disableLightFacility;
    [SyncVar]
    public int canToDisable;
    [SyncVar]
    public bool isStarted = false;
    private GameObject Host;
    private MTFRespawn mtfRespawn;
    private intensityLight[] lights;
    private void Awake()
    {
        disableLightFacility = this;
    }
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        lights = GameObject.FindObjectsOfType<intensityLight>();
        if (Host == null)
        {
            Host = GameObject.Find("Host");
        }
        mtfRespawn = Host.GetComponent<MTFRespawn>();
    }
    [Server]
    public void SetRandom(int New)
    {
        canToDisable = New;
    }
    [Server]
    private void Update()
    {
        if (mtfRespawn == null)
        {
            Debug.LogError($"{nameof(mtfRespawn)} is {mtfRespawn}");
            return;
        }
        switch (mtfRespawn.isSH && !isStarted)
            {
                case true:
                    DisableLight();
                    break;
            }
    }

    public void DisableLight()
    {
        StartCoroutine(Dis(false));
    }
    private void BlinkFacility()
    {
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].RpcBlink(3);
        }
    }
    private IEnumerator Dis(bool isRandom)
    {
        if (!isRandom)
        {
            canToDisable = 26;
        }
        else
        {
            canToDisable = Random.Range(0, 100);
        }
        isStarted = true;
        yield return new WaitForSeconds(1);
        BlinkFacility();
        yield return new WaitForSeconds(30);
        StartCoroutine(Dis(true));
    }
}
