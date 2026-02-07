using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class RagdollManager : NetworkBehaviour
{
	public LayerMask inspectionMask;

	private Transform cam;

	private CharacterClassManager ccm;

	private TextMeshProUGUI txt;

	public void SpawnRagdoll(Vector3 pos, Quaternion rot, int classID, PlayerStats.HitInfo ragdollInfo, bool allowRecall, string ownerID, string ownerNick)
	{
		Class @class = ccm.klasy[classID];
		if (@class.model_ragdoll != null)
		{
			GameObject gameObject = Object.Instantiate(@class.model_ragdoll, pos + @class.ragdoll_offset.position, Quaternion.Euler(rot.eulerAngles + @class.ragdoll_offset.rotation));
			NetworkServer.Spawn(gameObject);
			gameObject.GetComponent<Ragdoll>().SetOwner(new Ragdoll.Info(ownerID, ownerNick, ragdollInfo, classID));
			gameObject.GetComponent<Ragdoll>().SetRecall(allowRecall);
		}
		if (ragdollInfo.tool.Contains("SCP") || ragdollInfo.tool == "POCKET")
		{
			RegisterScpFrag();
		}
	}

	private void Start()
	{
		txt = GameObject.Find("BodyInspection").GetComponentInChildren<TextMeshProUGUI>();
		cam = GetComponent<Scp049PlayerScript>().plyCam.transform;
		ccm = GetComponent<CharacterClassManager>();
	}

	public void Update()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		string text = string.Empty;
		RaycastHit hitInfo;
		if (Physics.Raycast(new Ray(cam.position, cam.forward), out hitInfo, 3f, inspectionMask))
		{
			Ragdoll componentInParent = hitInfo.transform.GetComponentInParent<Ragdoll>();
			if (componentInParent != null)
			{
				text = TranslationReader.Get("Death_Causes", 12);
				text = text.Replace("[user]", componentInParent.owner.steamClientName);
				text = text.Replace("[cause]", GetCause(componentInParent.owner.deathCause, false));
				text = text.Replace("[class]", "<color=" + GetColor(ccm.klasy[componentInParent.owner.charclass].classColor) + ">" + ccm.klasy[componentInParent.owner.charclass].fullName + "</color>");
			}
		}
		txt.text = text;
	}

	public string GetColor(Color c)
	{
		Color32 color = new Color32((byte)(c.r * 255f), (byte)(c.g * 255f), (byte)(c.b * 255f), byte.MaxValue);
		return "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
	}

	public void RegisterScpFrag()
	{
		RoundSummary.kills_by_scp++;
	}

	public static string GetCause(PlayerStats.HitInfo info, bool ragdoll)
	{
		string result = TranslationReader.Get("Death_Causes", 11);
		int result2 = -1;
		if (info.tool == "NUKE")
		{
			result = TranslationReader.Get("Death_Causes", 0);
		}
		else if (info.tool == "FALLDOWN")
		{
			result = TranslationReader.Get("Death_Causes", 1);
		}
		else if (info.tool == "LURE")
		{
			result = TranslationReader.Get("Death_Causes", 2);
		}
		else if (info.tool == "POCKET")
		{
			result = TranslationReader.Get("Death_Causes", 3);
		}
		else if (info.tool == "CONTAIN")
		{
			result = TranslationReader.Get("Death_Causes", 4);
		}
		else if (info.tool == "TESLA")
		{
			result = TranslationReader.Get("Death_Causes", 5);
		}
		else if (info.tool == "WALL")
		{
			result = TranslationReader.Get("Death_Causes", 6);
		}
		else if (info.tool == "DECONT")
		{
			result = TranslationReader.Get("Death_Causes", 15);
		}
		else if (info.tool == "FRAG")
		{
			result = TranslationReader.Get("Death_Causes", 16);
		}
		else if (info.tool.Length > 7 && info.tool.Substring(0, 7) == "Weapon:" && int.TryParse(info.tool.Remove(0, 7), out result2) && result2 != -1)
		{
			GameObject gameObject = GameObject.Find("Host");
			AmmoBox component = gameObject.GetComponent<AmmoBox>();
			WeaponManager component2 = gameObject.GetComponent<WeaponManager>();
			result = TranslationReader.Get("Death_Causes", 7).Replace("[ammotype]", component.types[component2.weapons[result2].ammoType].label);
		}
		else if (info.tool.Length > 4 && info.tool.Substring(0, 4) == "SCP:" && int.TryParse(info.tool.Remove(0, 4), out result2))
		{
			switch (result2)
			{
			case 173:
				result = TranslationReader.Get("Death_Causes", 8);
				break;
			case 106:
				result = TranslationReader.Get("Death_Causes", 9);
				break;
			case 96:
				result = TranslationReader.Get("Death_Causes", 13);
				break;
			case 49:
			case 492:
				result = TranslationReader.Get("Death_Causes", 10);
				break;
			case 939:
				result = TranslationReader.Get("Death_Causes", 14);
				break;
			}
		}
		return result;
	}

	private void UNetVersion()
	{
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
