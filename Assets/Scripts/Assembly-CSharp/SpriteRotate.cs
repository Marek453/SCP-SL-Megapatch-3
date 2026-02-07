using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRotate : MonoBehaviour
{
	private Transform cam;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	private void LateUpdate()
	{

		if (cam == null)
		{
			cam = GameObject.Find("SpectatorCamera").transform;
		}
		else
		{
			base.transform.LookAt(cam);
		}
	}
}
