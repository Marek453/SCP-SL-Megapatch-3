using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerOpen : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(!GetComponentInParent<GateWay>().isMoveing)
        {
            if(!GetComponentInParent<GateWay>().Lock)
            {
                if(other.tag == "Player")
                {
                    GetComponentInParent<GateWay>().Open();
                }
            }
        }
    }
}
