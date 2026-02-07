using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Scp008EntityScript : NetworkBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<NetworkIdentity>().isLocalPlayer) return;
        CharacterClassManager ccm = other.GetComponent<CharacterClassManager>();
        if (base.tag != "SCP008") return;
        if (ccm.curClass != -1 && ccm.klasy[ccm.curClass].team != Team.SCP || ccm.curClass != -1 && ccm.klasy[ccm.curClass].team != Team.CHI)
        {
            if (ccm.GetComponent<Sco008PlayerScript>().Infect != true)
            {
                CmdSetInfectVoid(ccm.gameObject);
            }
        }
    }

    [Command]
    public void CmdSetInfectVoid(GameObject ccm)
    {
        ccm.GetComponent<Sco008PlayerScript>().SetInfect(true);
        ccm.GetComponent<Sco008PlayerScript>().StartInfectVoid();
    }
}
