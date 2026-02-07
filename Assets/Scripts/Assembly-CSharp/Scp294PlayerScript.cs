using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Scp294PlayerScript : NetworkBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if(!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.Escape) && GetComponent<FirstPersonController>().using294 && !CursorManager.pauseOpen && !CursorManager.eqOpen && !CursorManager.consoleOpen)
		{
			ToggleMenu();
		}
    }

    public void ToggleMenu()
    {
        CmdSetUse();
        CursorManager.Scp294PanelOpen = false;
			UserMainInterface.singleton.Scp294Panel.SetActive(false);
			base.GetComponent<FirstPersonController>().using294 = false;
            GetComponent<Scp457PlayerScript>().plyCam.gameObject.SetActive(true);
    }

    [Command]
    void CmdSetUse()
    {
        Scp294 _294 = GameObject.FindObjectOfType<Scp294>();
        _294.isUsed = false;
    }
}
