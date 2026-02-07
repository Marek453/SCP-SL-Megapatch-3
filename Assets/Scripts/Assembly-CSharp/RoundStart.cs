using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RoundStart : NetworkBehaviour
{
	[SyncVar(hook = "SetInfo")]
	public string info = string.Empty;

	public static RoundStart singleton;

	public GameObject window;

	public GameObject forceButton;

	public TextMeshProUGUI playersNumber;

	public Image loadingbar;

	public string Networkinfo
	{
		get
		{
			return info;
		}
		[param: In]
		set
		{
			ref string fieldValue = ref info;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetInfo(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	public void SetInfo(string i)
	{
		Networkinfo = i;
	}

	private void Awake()
	{
		singleton = this;
	}

	private void Update()
	{
		window.SetActive(info != string.Empty && info != "started");
		float result = 0f;
		float.TryParse(info, out result);
		result -= 1f;
		result /= 19f;
		loadingbar.fillAmount = Mathf.Lerp(loadingbar.fillAmount, result, Time.deltaTime);
		playersNumber.text = PlayerManager.singleton.players.Length.ToString();
	}

	private void Start()
	{
		GetComponent<RectTransform>().localPosition = Vector3.zero;
	}

	public void ShowButton()
	{
		forceButton.SetActive(true);
	}

	public void UseButton()
	{
		forceButton.SetActive(false);
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
			if (component.isLocalPlayer && gameObject.name == "Host")
			{
				component.ForceRoundStart();
			}
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(info);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(info);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			info = reader.ReadString();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetInfo(reader.ReadString());
		}
	}
}
