using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MEC;
using RemoteAdmin;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class Scp106PlayerScript : NetworkBehaviour
{
	[Header("Player Properties")]
	public Transform plyCam;

	public bool iAm106;

	public bool sameClass;

	[SyncVar]
	private float ultimatePoints;

	public float teleportSpeed;

	public GameObject containAnnouncePrefab;

	public GameObject screamsPrefab;

	[Header("Portal")]
	[SyncVar(hook = "SetPortalPosition")]
	public Vector3 portalPosition;

	public GameObject portalPrefab;

	private Vector3 previousPortalPosition;

	private CharacterClassManager ccm;

	private FirstPersonController fpc;

	private GameObject popup106;

	private TextMeshProUGUI highlightedAbilityText;

	private Text pointsText;

	private string highlightedString;

	public int highlightID;

	private Image cooldownImg;

	private static BlastDoor blastDoor;

	private float attackCooldown;

	public bool goingViaThePortal;

	private bool isCollidingDoorOpen;

	private Door doorCurrentlyIn;

	private Offset modelOffset;

	private bool isHighlightingPoints;

	public LayerMask teleportPlacementMask;

	private void Start()
	{
		if (blastDoor == null)
		{
			blastDoor = UnityEngine.Object.FindObjectOfType<BlastDoor>();
		}
		cooldownImg = GameObject.Find("Cooldown106").GetComponent<Image>();
		ccm = GetComponent<CharacterClassManager>();
		fpc = GetComponent<FirstPersonController>();
		InvokeRepeating("ExitDoor", 1f, 2f);
		if (base.isLocalPlayer && NetworkServer.active)
		{
			InvokeRepeating("HumanPocketLoss", 1f, 1f);
		}
		modelOffset = ccm.klasy[3].model_offset;
	}

	private void Update()
	{
		CheckForInventoryInput();
		CheckForShootInput();
		AnimateHighlightedText();
		UpdatePointText();
		DoorCollisionCheck();
	}

	[Server]
	private void HumanPocketLoss()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Scp106PlayerScript::HumanPocketLoss()' called on client");
			return;
		}
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			if (gameObject.transform.position.y < -1500f && gameObject.GetComponent<CharacterClassManager>().IsHuman())
			{
				gameObject.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(1f, "WORLD", "POCKET", GetComponent<QueryProcessor>().PlayerId), gameObject);
			}
		}
	}

	private void CheckForShootInput()
	{
		if (base.isLocalPlayer && iAm106)
		{
			cooldownImg.fillAmount = Mathf.Clamp01((!(attackCooldown <= 0f)) ? (1f - attackCooldown * 2f) : 0f);
			if (attackCooldown > 0f)
			{
				attackCooldown -= Time.deltaTime;
			}
			if (Input.GetKeyDown(NewInput.GetKey("Shoot")) && attackCooldown <= 0f && Inventory.inventoryCooldown <= 0f)
			{
				attackCooldown = 0.5f;
				Shoot();
			}
		}
	}

	private void Shoot()
	{
		RaycastHit hitInfo;
		if (Physics.Raycast(plyCam.transform.position, plyCam.transform.forward, out hitInfo, 1.5f))
		{
			CharacterClassManager component = hitInfo.transform.GetComponent<CharacterClassManager>();
			if (component != null && component.klasy[component.curClass].team != 0)
			{
				CmdMovePlayer(hitInfo.transform.gameObject, ServerTime.time);
				Hitmarker.Hit(1.5f);
			}
		}
	}

	private void UpdatePointText()
	{
		if (pointsText == null)
		{
			pointsText = UnityEngine.Object.FindObjectOfType<ScpInterfaces>().Scp106_ability_points;
			return;
		}
		if (base.isServer)
		{
			ultimatePoints = ultimatePoints + Time.deltaTime * 6.66f * teleportSpeed;
			ultimatePoints = Mathf.Clamp(ultimatePoints, 0f, 100f);
		}
		pointsText.text = TranslationReader.Get("Legancy_Interfaces", 11);
	}

	private bool BuyAbility(int cost)
	{
		if ((float)cost <= ultimatePoints)
		{
			if (base.isServer)
			{
				ultimatePoints = ultimatePoints - (float)cost;
			}
			return true;
		}
		return false;
	}

	private void AnimateHighlightedText()
	{
		if (highlightedAbilityText == null)
		{
			highlightedAbilityText = UnityEngine.Object.FindObjectOfType<ScpInterfaces>().Scp106_ability_highlight;
			return;
		}
		highlightedString = string.Empty;
		if (highlightID == 1)
		{
			highlightedString = TranslationReader.Get("Legancy_Interfaces", 12);
		}
		if (highlightID == 2)
		{
			highlightedString = TranslationReader.Get("Legancy_Interfaces", 13);
		}
		if (highlightedString != highlightedAbilityText.text)
		{
			if (highlightedAbilityText.canvasRenderer.GetAlpha() > 0f)
			{
				highlightedAbilityText.canvasRenderer.SetAlpha(highlightedAbilityText.canvasRenderer.GetAlpha() - Time.deltaTime * 4f);
			}
			else
			{
				highlightedAbilityText.text = highlightedString;
			}
		}
		else if (highlightedAbilityText.canvasRenderer.GetAlpha() < 1f && highlightedString != string.Empty)
		{
			highlightedAbilityText.canvasRenderer.SetAlpha(highlightedAbilityText.canvasRenderer.GetAlpha() + Time.deltaTime * 4f);
		}
	}

	private void CheckForInventoryInput()
	{
		if (base.isLocalPlayer)
		{
			if (popup106 == null)
			{
				popup106 = UnityEngine.Object.FindObjectOfType<ScpInterfaces>().Scp106_eq;
				return;
			}
			bool flag = (CursorManager.scp106 = iAm106 & Input.GetKey(NewInput.GetKey("Inventory")));
			popup106.SetActive(flag);
			fpc.m_MouseLook.scp106_eq = flag;
		}
	}

	public void Init(int classID, Class c)
	{
		iAm106 = classID == 3;
		sameClass = c.team == Team.SCP || c.team == Team.SH;
	}

	public void SetDoors()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		Door[] array = UnityEngine.Object.FindObjectsOfType<Door>();
		Door[] array2 = array;
		foreach (Door door in array2)
		{
			Collider[] componentsInChildren = door.GetComponentsInChildren<Collider>();
			foreach (Collider collider in componentsInChildren)
			{
				if (collider.tag != "DoorButton")
				{
					try
					{
						collider.isTrigger = iAm106;
					}
					catch
					{
					}
				}
			}
		}
	}

	[Server]
	public void Contain()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Scp106PlayerScript::Contain()' called on client");
			return;
		}
		ultimatePoints = 0f;
		StartCoroutine(_ClientContainAnimation());
	}

	public void DeletePortal()
	{
		if (portalPosition.y < 900f)
		{
			portalPrefab = null;
			portalPosition = Vector3.zero;
		}
	}

	public void UseTeleport()
	{
		if (!(portalPrefab == null))
		{
			if (BuyAbility(100) && portalPosition != Vector3.zero)
			{
				CmdUsePortal();
			}
			else
			{
				StartCoroutine(_HighlightPointsText());
			}
		}
	}

	private void SetPortalPosition(Vector3 pos)
	{
		portalPosition = pos;
		StartCoroutine(_DoPortalSetupAnimation());
	}

	public void CreatePortalInCurrentPosition()
	{
		if (BuyAbility(100))
		{
			if (base.isLocalPlayer)
			{
				CmdMakePortal();
			}
		}
		else
		{
			StartCoroutine(_HighlightPointsText());
		}
	}

	private IEnumerator _ClientContainAnimation()
	{
		UnityEngine.Object.Instantiate(screamsPrefab);
		for (int i = 0; i < 750; i++)
		{
			yield return 0f;
		}
		if (base.isLocalPlayer)
		{
			goingViaThePortal = true;
			VignetteAndChromaticAberration vaca = GetComponentInChildren<VignetteAndChromaticAberration>();
			Recoil recoil = GetComponentInChildren<Recoil>();
			fpc.noclip = true;
			for (float j = 1f; j <= 175f; j += 1f)
			{
				recoil.positionOffset = -1.6f * (vaca.intensity = j / 175f);
				yield return 0f;
			}
            yield return new WaitForSeconds(2f);
			fpc.noclip = false;
			goingViaThePortal = false;
            yield return new WaitForSeconds(5f);
			vaca.intensity = 0.036f;
			recoil.positionOffset = 0f;
		}
		else
		{
			GetComponent<AnimationController>().animator.SetTrigger("Teleporting");
		}
	}

	[ClientRpc]
	private void RpcContainAnimation()
	{
		StartCoroutine(_ClientContainAnimation());
	}

	private void LateUpdate()
	{
		Animator animator = GetComponent<AnimationController>().animator;
		if (animator != null && iAm106 && !base.isLocalPlayer)
		{
			AnimationFloatValue component = ccm.myModel.GetComponent<AnimationFloatValue>();
			Offset offset = modelOffset;
			offset.position -= component.v3_value * component.f_value;
			animator.transform.localPosition = offset.position;
			animator.transform.localRotation = Quaternion.Euler(offset.rotation);
		}
	}

	[Server]
	private void Kill()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Scp106PlayerScript::Kill()' called on client");
			return;
		}
		GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo
		{
			amount = 999799f
		}, base.gameObject);
	}

	private IEnumerator _HighlightPointsText()
	{
		if (!isHighlightingPoints)
		{
			isHighlightingPoints = true;
			while ((double)pointsText.color.g > 0.05)
			{
				pointsText.color = Color.Lerp(pointsText.color, Color.red, 0.19999999f);
				yield return 0f;
			}
			while ((double)pointsText.color.g < 0.95)
			{
				pointsText.color = Color.Lerp(pointsText.color, Color.white, 0.19999999f);
				yield return 0f;
			}
			isHighlightingPoints = false;
		}
	}

	private IEnumerator _DoPortalSetupAnimation()
	{
		while (portalPrefab == null)
		{
			portalPrefab = GameObject.Find("SCP106_PORTAL");
			yield return 0f;
		}
		Animator portalAnim = portalPrefab.GetComponent<Animator>();
		portalAnim.SetBool("activated", false);
		yield return new WaitForSeconds(1f);
		portalPrefab.transform.position = portalPosition;
		portalAnim.SetBool("activated", true);
	}

	[ClientRpc]
	public void RpcTeleportAnimation()
	{
		Timing.RunCoroutine(_ClientTeleportAnimation(),Segment.FixedUpdate);
	}

	[Command]
	void CmdTp()
	{
		RpcTp();
	}

	[ClientRpc]
	void RpcTp()
	{
		
	}

	private IEnumerator<float> _ClientTeleportAnimation()
	{
		if (!(portalPrefab != null))
		{
			yield break;
		}
		if (base.isLocalPlayer)
		{
			goingViaThePortal = true;
			VignetteAndChromaticAberration vaca = GetComponentInChildren<VignetteAndChromaticAberration>();
			Recoil recoil = GetComponentInChildren<Recoil>();
			fpc.noclip = true;
			for (float k = 1f; k <= 175f; k += 1f)
			{
				recoil.positionOffset = -1.6f * (vaca.intensity = k / 175f);
				yield return 0f;
			}
			for (float j = 1f; j <= 25f; j += 1f)
			{
				yield return 0f;
			}
			base.GetComponent<PlyMovementSync>().CmdSetPosition(portalPosition+ Vector3.up + Vector3.up);
            for (float i = 1f; i <= 150f; i += 1f)
			{
				recoil.positionOffset = -1.6f * (vaca.intensity = 1f - i / 150f);
				yield return 0f;
			}
			vaca.intensity = 0.036f;
			recoil.positionOffset = 0f;
			fpc.noclip = false;
			goingViaThePortal = false;
		}
		else
		{
			GetComponent<AnimationController>().animator.SetTrigger("Teleporting");
		}
	}

	[Command(channel = 4)]
	private void CmdMakePortal()
	{
		UnityEngine.Debug.DrawRay(base.transform.position, -base.transform.up, Color.red, 10f);
		RaycastHit hitInfo;
		if (iAm106 && !goingViaThePortal && Physics.Raycast(new Ray(base.transform.position, -base.transform.up), out hitInfo, 10f, teleportPlacementMask))
		{
			SetPortalPosition(hitInfo.point - Vector3.up);
		}
	}

	[Command]
	public void CmdUsePortal()
	{
		if (iAm106 && portalPosition != Vector3.zero && !goingViaThePortal)
		{
            RpcTeleportAnimation();
		}
	}

	[Command]
	private void CmdMovePlayer(GameObject ply, int t)
	{
		if (ServerTime.CheckSynchronization(t) && iAm106 && Vector3.Distance(GetComponent<PlyMovementSync>().position, ply.transform.position) < 3f && ply.GetComponent<CharacterClassManager>().IsHuman())
		{
			GetComponent<CharacterClassManager>().CallRpcPlaceBlood(ply.transform.position, 1, 4f);
			if (blastDoor.isClosed)
			{
				GetComponent<CharacterClassManager>().CallRpcPlaceBlood(ply.transform.position, 1, 4f);
				GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(500f, "SCP:106", "SCP:106", GetComponent<QueryProcessor>().PlayerId), ply);
			}
			else
			{
				GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(40f, "SCP:106", "SCP:106", GetComponent<QueryProcessor>().PlayerId), ply);
				ply.GetComponent<PlyMovementSync>().SetPosition(Vector3.down * 1997f);
			}
		}
	}

	[ClientRpc]
	public void RpcAnnounceContaining()
	{
		//UnityEngine.Object.Instantiate(containAnnouncePrefab);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!base.isLocalPlayer || ccm.curClass != 3)
		{
			return;
		}
		Door componentInParent = other.GetComponentInParent<Door>();
		if (componentInParent != null)
		{
			doorCurrentlyIn = componentInParent;
			isCollidingDoorOpen = false;
			fpc.m_WalkSpeed = 1f;
			fpc.m_RunSpeed = 1f;
			if (componentInParent.isOpen && componentInParent.curCooldown <= 0f)
			{
				fpc.m_WalkSpeed = ccm.klasy[ccm.curClass].walkSpeed;
				fpc.m_RunSpeed = ccm.klasy[ccm.curClass].runSpeed;
				isCollidingDoorOpen = true;
			}
		}
	}

	private void ExitDoor()
	{
		if (base.isLocalPlayer && ccm.curClass == 3)
		{
			fpc.m_WalkSpeed = ccm.klasy[ccm.curClass].walkSpeed;
			fpc.m_RunSpeed = ccm.klasy[ccm.curClass].runSpeed;
			doorCurrentlyIn = null;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		ExitDoor();
	}

	private void DoorCollisionCheck()
	{
		if (doorCurrentlyIn != null && doorCurrentlyIn.destroyed)
		{
			ExitDoor();
		}
		else if (!isCollidingDoorOpen && doorCurrentlyIn != null && doorCurrentlyIn.isOpen && doorCurrentlyIn.curCooldown <= 0f && !isCollidingDoorOpen)
		{
			fpc.m_WalkSpeed = ccm.klasy[ccm.curClass].walkSpeed;
			fpc.m_RunSpeed = ccm.klasy[ccm.curClass].runSpeed;
			isCollidingDoorOpen = true;
		}
		else if (isCollidingDoorOpen && doorCurrentlyIn != null && !doorCurrentlyIn.isOpen)
		{
			isCollidingDoorOpen = false;
			fpc.m_WalkSpeed = 1f;
			fpc.m_RunSpeed = 1f;
		}
	}
}
