using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Networking;

namespace AntiFaker
{
	public class AntiFakeCommands : NetworkBehaviour
	{
		private static List<Transform> allowedTeleportPositions = new List<Transform>();

		private static AntiFakeCommands host;

		private Scp173PlayerScript scp173;

		private Scp096PlayerScript scp096;

		private PlyMovementSync pms;

		private CharacterClassManager ccm;

		private float distanceTraveled;

		private Vector3 prevPos = Vector3.zero;

		private float maxDistance;

		[Header("Noclip Protection")]
		private bool noclip_protection;

		public LayerMask mask;

		private void Start()
		{
			noclip_protection = ConfigFile.ServerConfig.GetBool("noclip_protection", noclip_protection);
			scp173 = GetComponent<Scp173PlayerScript>();
			scp096 = GetComponent<Scp096PlayerScript>();
			if (!TutorialManager.status)
			{
				if (base.isLocalPlayer && base.isServer)
				{
					allowedTeleportPositions.Clear();
					AddTypeToList("Spawnpoint");
					host = this;
				}
				ccm = GetComponent<CharacterClassManager>();
				pms = GetComponent<PlyMovementSync>();
				Timing.RunCoroutine(_AntiSpeedhack(), Segment.Update);
			}
		}

		public bool CheckMovement(Vector3 pos)
		{
			if (TutorialManager.status || (base.isLocalPlayer && base.isServer) || ccm.curClass == -1 || ccm.curClass == 2)
			{
				prevPos = pos;
				return true;
			}
			distanceTraveled += Vector2.Distance(new Vector2(prevPos.x, prevPos.z), new Vector2(pos.x, pos.z));
			if (ccm.curClass == 0)
			{
				maxDistance = ((!scp173.CanMove()) ? 3f : (scp173.boost_teleportDistance.Evaluate(GetComponent<PlayerStats>().GetHealthPercent()) * 2f));
			}
			else if (ccm.curClass > 0)
			{
				maxDistance = ccm.klasy[ccm.curClass].runSpeed;
			}
			if (ccm.curClass == 9 && scp096.enraged == Scp096PlayerScript.RageState.Enraged)
			{
				maxDistance *= 4.9f;
			}
			if (distanceTraveled < maxDistance * 1.3f)
			{
				RaycastHit hitInfo;
				if (noclip_protection && Physics.Linecast(prevPos, pos, out hitInfo, mask))
				{
					bool flag = true;
					Door componentInParent = hitInfo.collider.GetComponentInParent<Door>();
					if (componentInParent != null)
					{
						if (ccm.curClass == 3)
						{
							flag = false;
						}
						else if (componentInParent.curCooldown > 0.7f)
						{
							flag = false;
						}
						else if (ccm.curClass == 9 && componentInParent.destroyedPrefab != null && GetComponent<Scp096PlayerScript>().enraged == Scp096PlayerScript.RageState.Enraged)
						{
							flag = false;
						}
					}
					if (flag)
					{
						return false;
					}
				}
				prevPos = pos;
				return true;
			}
			return false;
		}

		private IEnumerator<float> _AntiSpeedhack()
		{
			while (true)
			{
				distanceTraveled = 0f;
				yield return Timing.WaitForSeconds(1f);
			}
		}

		public bool SpeedhackJustification(Vector3 pos)
		{
			int curClass = ccm.curClass;
			if (Vector3.Distance(pos, ccm.deathPosition) < 10f || pos.y > 2000f || pos.y < -1500f)
			{
				return true;
			}
			foreach (Transform allowedTeleportPosition in allowedTeleportPositions)
			{
				if (Vector3.Distance(pos, allowedTeleportPosition.position) < 10f)
				{
					if (allowedTeleportPosition.tag == "SP_CDP" && curClass != 1)
					{
						return false;
					}
					if (allowedTeleportPosition.tag == "SP_173" && curClass != 0)
					{
						return false;
					}
					if (allowedTeleportPosition.tag == "SP_106" && curClass != 3)
					{
						return false;
					}
					if (allowedTeleportPosition.tag == "SP_049" && curClass != 5)
					{
						return false;
					}
					if (allowedTeleportPosition.tag == "SP_MTF" && ccm.klasy[curClass].team != Team.MTF)
					{
						return false;
					}
					if (allowedTeleportPosition.tag == "SP_RSC" && curClass != 6)
					{
						return false;
					}
					if (allowedTeleportPosition.tag == "SP_CI" && curClass != 8)
					{
						return false;
					}
					return true;
				}
			}
			if (curClass == 3 && Vector3.Distance(pos, GameObject.Find("SCP106_PORTAL").transform.position) < 10f)
			{
				return true;
			}
			return false;
		}

		public void FindAllowedTeleportPositions()
		{
			if (!TutorialManager.status)
			{
				AddTypeToList("SP_CDP");
				AddTypeToList("SP_173");
				AddTypeToList("SP_106");
				AddTypeToList("SP_049");
				AddTypeToList("SP_MTF");
				AddTypeToList("SP_RSC");
				AddTypeToList("SP_079");
				AddTypeToList("SCP_096");
				AddTypeToList("PD_EXIT");
				AddTypeToList("SP_CI");
				AddTypeToList("LiftTarget");
			}
		}

		private void AddTypeToList(string type)
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag(type);
			GameObject[] array2 = array;
			foreach (GameObject gameObject in array2)
			{
				allowedTeleportPositions.Add(gameObject.transform);
			}
		}

		public void SetPosition(Vector3 pos)
		{
			prevPos = pos;
			distanceTraveled = 0f;
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
}
