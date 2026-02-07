using System.Collections;
using System.Collections.Generic;
using LlockhamIndustries.ExtensionMethods;
using UnityEngine;

public class RotationVent : MonoBehaviour
{
    public float Speed;

    public Vector3 speed;
    public bool isY;

    public bool Use = false;
    void FixedUpdate()
    {
        if(!Use)
        {
        if (!isY)
        {
            this.transform.Rotate(Vector3.right * Speed);
        }
        if(isY)
        {
            this.transform.Rotate(Vector3.up * Speed);
        }
        }
        else
        {
            this.transform.Rotate(speed);
        }
    }
}
