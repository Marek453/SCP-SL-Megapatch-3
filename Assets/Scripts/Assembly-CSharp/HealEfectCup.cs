using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HealEfectCup : NetworkBehaviour
{
    float time;
    public AudioSource EffectSound,SupSound;

    [Serializable]
    public class MyCup
    {
        public int ID;
        public AudioSource source;
        public int HP;
    }
    public MyCup[] ID;
    public float inventoryCooldown;

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
        PlayerStats stats = GetComponent<PlayerStats>();
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
                    CmdUse(i);
                }
            }
        }   
        }
        }
    }
    [Command]
   void CmdUse(int id)
   {
     PlayerStats stats = GetComponent<PlayerStats>();
    stats.SetHPAmount(stats.health += ID[id].HP);
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
