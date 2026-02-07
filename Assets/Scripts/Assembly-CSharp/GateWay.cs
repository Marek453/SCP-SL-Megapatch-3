using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GateWay : NetworkBehaviour
{
    public Animator[] Doors;
    public string permissionLevel;
    public float TimeToEnd;
    public GameObject Efect;
    public AudioSource source;
    public AudioClip ClipOpen,ClipClose;
    public bool isMoveing;
    public Vector3 localPos;
	public Quaternion localRot;

    [SyncVar(hook = "ChangeState")]
    public bool isOpen;

    [SyncVar(hook = "LockState")]
    public bool Lock;

    public void SetLocalPos()
	{
		localPos = base.transform.localPosition;
		localRot = base.transform.localRotation;
	}

    public void SetZero()
	{
		localPos = Vector3.zero;
	}

    public void UpdatePos()
	{
		if (!(localPos == Vector3.zero))
		{
			base.transform.localPosition = localPos;
			base.transform.localRotation = localRot;
		}
	}


    public void ChangeState(bool d)
    {
        isOpen = d;
    }

    void Start()
    {
        Efect.SetActive(false);
    }

    public void LockState(bool d)
    {
        Lock = d;
    }

    void Update()
    {
        foreach(Animator door in Doors)
        {
            door.SetBool("isOpen",isOpen);
        }
    }

    public void Open()
    {
        isMoveing = true;
        ChangeState(true);
        source.PlayOneShot(ClipOpen);
        StartCoroutine(GateOpen());
    }

    IEnumerator GateOpen()
    {
        yield return new WaitForSeconds(2f);
        Efect.SetActive(true);
        yield return new WaitForSeconds(TimeToEnd);
        Efect.SetActive(false);
        yield return new WaitForSeconds(2);
        ChangeState(false);
        isMoveing = false;
        source.PlayOneShot(ClipClose);
    }

}
