using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NoEffectCup : ItemCup
{
    float time;
    public AudioSource EffectSound,SupSound;
    
    public override void OnUse()
    {
        
    }

    void Update()
    {
        if (time >= 0f)
		{
			time -= Time.deltaTime;
		}
        if(isLocalPlayer)
        {
            inventoryCooldown -= Time.deltaTime;
			if (Cursor.lockState != CursorLockMode.Locked)
			{
				inventoryCooldown = 0.2f;
			}
        Inventory inv = GetComponent<Inventory>();
        for (int i = 0; i < ID.Length; i++)
        {
            if(inventoryCooldown <= 0f && inv.curItem == ID[i].ID && Input.GetKeyDown(NewInput.GetKey("Shoot"))&& time < 0f)
        {
            for (int f= 0; f < inv.items.Count; f++)
            {
                if(inv.items[f].id == ID[i].ID)
                {
                    inv.items.Remove(inv.items[f]);
                    inv.SetCurItem(-1);
                    EffectSound = ID[i].source;
                    CmdUse();
                }
            }
        }   
        }
        }
    }
    [Command]
   void CmdUse()
   {
    if(EffectSound != null)
    {
        StartCoroutine(PlaySound());
    }
    SupSound.Play();
   }

   IEnumerator PlaySound()
   {
    yield return new WaitForSeconds(1);
    EffectSound.Play();
   }
}
