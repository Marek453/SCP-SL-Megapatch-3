using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ItemCup : NetworkBehaviour
{
    [Serializable]
    public class MyCup
    {
        public int ID;
        public AudioSource source;
    }
    public MyCup[] ID;
    public virtual void OnUse(){}
    public float inventoryCooldown;
}
