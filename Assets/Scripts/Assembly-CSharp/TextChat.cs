using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TextChat : NetworkBehaviour
{
	public int messageDuration;

	private static Transform lply;

	public GameObject textMessagePrefab;

	private Transform attachParent;

	public bool enabledChat;

	private List<GameObject> msgs = new List<GameObject>();

	private static int kCmdCmdSendChat;

	private static int kRpcRpcSendChat;

	private void Start()
	{
		if (base.isLocalPlayer)
		{
			lply = base.transform;
		}
	}

	private void Update()
	{
		if (!base.isLocalPlayer || !enabledChat)
		{
			return;
		}
		for (int i = 0; i < msgs.Count; i++)
		{
			if (msgs[i] == null)
			{
				msgs.RemoveAt(i);
				break;
			}
			msgs[i].GetComponent<TextMessage>().position = msgs.Count - i - 1;
		}
		if (Input.GetKeyDown(KeyCode.Return))
		{
			SendChat("(づ｡◕\u203f\u203f◕｡)づ" + Random.Range(0, 4654), GetComponent<NicknameSync>().myNick, base.transform.position);
		}
	}

	private void SendChat(string msg, string nick, Vector3 position)
	{
		CallCmdSendChat(msg, nick, position);
	}

	[Command(channel = 2)]
	private void CmdSendChat(string msg, string nick, Vector3 pos)
	{
		CallRpcSendChat(msg, nick, pos);
	}

	[ClientRpc(channel = 2)]
	private void RpcSendChat(string msg, string nick, Vector3 pos)
	{
		if (Vector3.Distance(lply.position, pos) < 15f)
		{
			AddMsg(msg, nick);
		}
	}

	private void AddMsg(string msg, string nick)
	{
		while (msg.Contains("<"))
		{
			msg = msg.Replace("<", "＜");
		}
		while (msg.Contains(">"))
		{
			msg = msg.Replace(">", "＞");
		}
		string text = "<b>" + nick + "</b>: " + msg;
		GameObject gameObject = Object.Instantiate(textMessagePrefab);
		gameObject.transform.SetParent(attachParent);
		msgs.Add(gameObject);
		gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
		gameObject.transform.localScale = Vector3.one;
		gameObject.GetComponent<Text>().text = text;
		gameObject.GetComponent<TextMessage>().remainingLife = messageDuration;
		Object.Destroy(gameObject, messageDuration);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSendChat(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendChat called on client.");
		}
		else
		{
			((TextChat)obj).CmdSendChat(reader.ReadString(), reader.ReadString(), reader.ReadVector3());
		}
	}

	public void CallCmdSendChat(string msg, string nick, Vector3 pos)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSendChat called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSendChat(msg, nick, pos);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSendChat);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(msg);
		networkWriter.Write(nick);
		networkWriter.Write(pos);
		SendCommandInternal(networkWriter, 2, "CmdSendChat");
	}

	protected static void InvokeRpcRpcSendChat(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSendChat called on server.");
		}
		else
		{
			((TextChat)obj).RpcSendChat(reader.ReadString(), reader.ReadString(), reader.ReadVector3());
		}
	}

	public void CallRpcSendChat(string msg, string nick, Vector3 pos)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSendChat called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSendChat);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(msg);
		networkWriter.Write(nick);
		networkWriter.Write(pos);
		SendRPCInternal(networkWriter, 2, "RpcSendChat");
	}

	static TextChat()
	{
		kCmdCmdSendChat = -683434843;
		NetworkBehaviour.RegisterCommandDelegate(typeof(TextChat), kCmdCmdSendChat, InvokeCmdCmdSendChat);
		kRpcRpcSendChat = -734819717;
		NetworkBehaviour.RegisterRpcDelegate(typeof(TextChat), kRpcRpcSendChat, InvokeRpcRpcSendChat);
		NetworkCRC.RegisterBehaviour("TextChat", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}
}
