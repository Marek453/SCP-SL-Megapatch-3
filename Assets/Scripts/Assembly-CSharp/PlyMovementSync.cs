using System.Runtime.InteropServices;
using AntiFaker;
using UnityEngine;
using UnityEngine.Networking;

public class PlyMovementSync : NetworkBehaviour
{
	public float rotation;

	public Vector3 position;

	[SyncVar]
	public float rotX;

	private bool allowInput;

	private float myRotation;

	private CharacterClassManager ccm;

	private AntiFakeCommands speedhack;

	private Transform plyCam;

	private Vector3 teleportPosition;

	public void SetupPosRot(Vector3 _p, float _r)
	{
		position = _p;
		rotation = _r;
	}

	private void FixedUpdate()
	{
		if (base.isLocalPlayer)
		{
			myRotation = base.transform.rotation.eulerAngles.y;
		}
		TransmitData();
	}

	[ClientCallback]
	private void TransmitData()
	{
		if (NetworkClient.active && base.isLocalPlayer)
		{
			CmdSyncData(myRotation, base.transform.position, GetComponent<PlayerInteract>().playerCamera.transform.localRotation.eulerAngles.x);
		}
	}

	private void Start()
	{
		plyCam = GetComponent<Scp049PlayerScript>().plyCam.transform;
		speedhack = GetComponent<AntiFakeCommands>();
		ccm = GetComponent<CharacterClassManager>();
		teleportPosition = Vector3.zero;
		allowInput = true;
	}

	[Command(channel = 5)]
	private void CmdSyncData(float rot, Vector3 pos, float x)
	{
		rotation = rot;
		if (teleportPosition != Vector3.zero)
		{
			position = teleportPosition;
			speedhack.SetPosition(teleportPosition);
			base.transform.position = teleportPosition;
			teleportPosition = Vector3.zero;
		}
		else if (allowInput && speedhack.CheckMovement(pos))
		{
			if (ccm.curClass == 2)
			{
				pos = new Vector3(0f, 2048f, 0f);
			}
			position = pos;
		}
		else
		{
		TargetSetPosition(base.connectionToClient, position);
		}
		rotX = x;
		plyCam.transform.localRotation = Quaternion.Euler(x, 0f, 0f);
	}

	[TargetRpc]
	private void TargetSetPosition(NetworkConnection target, Vector3 pos)
	{
		base.transform.position = pos;
		position = pos;
	}

	[TargetRpc]
	private void TargetSetRotation(NetworkConnection target, float rot)
	{
		myRotation = rot;
		rotation = rot;
		base.transform.rotation = Quaternion.Euler(0f, rot, 0f);
		try
		{
			FirstPersonController component = GetComponent<FirstPersonController>();
			if (component != null)
			{
				component.m_MouseLook.SetRotation(rot);
			}
		}
		catch
		{
		}
	}

	[Client]
	public void ClientSetRotation(float rot)
	{
		if (!NetworkClient.active)
		{
			Debug.LogWarning("[Client] function 'System.Void PlyMovementSync::ClientSetRotation(System.Single)' called on server");
		}
		else
		{
			myRotation = rot;
		}
	}

	[Server]
	public void SetPosition(Vector3 pos)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlyMovementSync::SetPosition(UnityEngine.Vector3)' called on client");
			return;
		}
		teleportPosition = pos;
		position = pos;
		base.transform.position = pos;
		speedhack.SetPosition(pos);
		TargetSetPosition(base.connectionToClient, pos);
	}

	[Command]
	public void CmdSetPosition(Vector3 pos)
	{
		teleportPosition = pos;
		position = pos;
		base.transform.position = pos;
		speedhack.SetPosition(pos);
		TargetSetPosition(base.connectionToClient, pos);
	}

	[Server]
	public void SetRotation(float rot)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlyMovementSync::SetRotation(System.Single)' called on client");
			return;
		}
		rotation = rot;
		myRotation = rot;
		TargetSetRotation(base.connectionToClient, rot);
	}

	[Server]
	public void SetAllowInput(bool b)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlyMovementSync::SetAllowInput(System.Boolean)' called on client");
		}
		else
		{
			allowInput = b;
		}
	}
}
