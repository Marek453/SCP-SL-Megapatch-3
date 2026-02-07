// WeaponManager
using System;
using System.Collections.Generic;
using System.Collections;
using MEC;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class WeaponManager : NetworkBehaviour
{
    [Serializable]
    public class Weapon
    {
        [Serializable]
        public class WeaponMod
        {
            [Serializable]
            public class WeaponModEffects
            {
                [Header("Sights only effects")]
                public PostProcessProfile customProfile;

                [Tooltip("FOV")]
                public float zoomFov = 70f;

                [Tooltip("RECOIL SCALE")]
                public float zoomRecoilReduction = 1f;

                [Tooltip("WALK SLOW")]
                public float zoomSlowdown = 1f;

                [Tooltip("SENSITIVITY")]
                public float zoomSensitivity = 1f;

                [Tooltip("RECOIL ANIMATION SCALE")]
                public float zoomRecoilAnimScale = 1f;

                public Vector3 zoomPositionOffset = Vector3.zero;

                [Header("Barrels only effects")]
                public AudioClip shootSound;

                public float damageMultiplier = 1f;

                public float audioSourceRangeScale = 1f;

                [Header("Ammo Counter Effects")]
                public Text counterText;

                public string counterTemplate;

                [Header("Mixed effects")]
                public float overallRecoilReduction = 1f;

                public bool isLaser;
            }

            public string name;

            public GameObject prefab_firstperson;

            public GameObject prefab_thirdperson;

            public WeaponModEffects effects;

            public Texture icon;

            public bool isActive;

            public void SetVisibility(bool b)
            {
                isActive = b;
                if (prefab_firstperson != null)
                {
                    prefab_firstperson.SetActive(b);
                }
                if (prefab_thirdperson != null)
                {
                    prefab_thirdperson.SetActive(b);
                }
            }
        }

        [Header("Misc properties")]
        public int inventoryID;

        public RecoilProperties recoil;

        public AnimationCurve damageOverDistance;

        public float shotsPerSecond;

        public bool allowFullauto;

        public Vector3 positionOffset;

        public GameObject holeEffect;

        public ParticleSystem husks;

        public float recoilAnimation = 0.5f;

        public float bobAnimationScale = 1f;

        [Header("Ammo & reloading")]
        public AudioClip reloadClip;

        public int maxAmmo;

        public int ammoType;

        public float reloadingTime;

        [Header("Zooming")]
        public bool allowZoom;

        public float zoomingTime;

        public float unfocusedSpread = 5f;

        [Header("Mods")]
        public WeaponMod[] mod_sights;

        public WeaponMod[] mod_barrels;

        public WeaponMod[] mod_others;

        public WeaponMod.WeaponModEffects allEffects;

        private int cur_sight;

        private int cur_barrel;

        private int cur_other;

        public void PlayMuzzleFlashes(bool firstperson)
        {
            GameObject gameObject = ((!firstperson) ? mod_barrels[cur_barrel].prefab_thirdperson : mod_barrels[cur_barrel].prefab_firstperson);
            if (gameObject != null)
            {
                ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem particleSystem in componentsInChildren)
                {
                    particleSystem.Play();
                }
            }
        }

        public void SetMods(int _s, int _b, int _o, bool savePlayerPrefs, bool _flashlight)
        {
            cur_sight = Mathf.Clamp(_s, 0, mod_sights.Length - 1);
            cur_barrel = Mathf.Clamp(_b, 0, mod_barrels.Length - 1);
            cur_other = Mathf.Clamp(_o, 0, mod_others.Length - 1);
            RefreshMods(savePlayerPrefs, _flashlight);
        }

        public void RefreshMods(bool saveToPlayerPrefs, bool _flashlight)
        {
            if (saveToPlayerPrefs)
            {
                PlayerPrefs.SetInt("Weapon_" + inventoryID + "_sight", cur_sight);
                PlayerPrefs.SetInt("Weapon_" + inventoryID + "_barrel", cur_barrel);
                PlayerPrefs.SetInt("Weapon_" + inventoryID + "_other", cur_other);
            }
            for (int i = 0; i < mod_sights.Length; i++)
            {
                mod_sights[i].SetVisibility(i == cur_sight);
            }
            for (int j = 0; j < mod_barrels.Length; j++)
            {
                mod_barrels[j].SetVisibility(j == cur_barrel);
            }
            for (int k = 0; k < mod_others.Length; k++)
            {
                mod_others[k].SetVisibility(k == cur_other);
                if (k != cur_other)
                {
                    continue;
                }
                if (mod_others[k].name.ToLower().Contains("flashlight"))
                {
                    if (mod_others[k].prefab_firstperson != null)
                    {
                        Light[] componentsInChildren = mod_others[k].prefab_firstperson.GetComponentsInChildren<Light>();
                        foreach (Light light in componentsInChildren)
                        {
                            light.enabled = _flashlight;
                        }
                    }
                    if (mod_others[k].prefab_thirdperson != null)
                    {
                        Light[] componentsInChildren2 = mod_others[k].prefab_thirdperson.GetComponentsInChildren<Light>();
                        foreach (Light light2 in componentsInChildren2)
                        {
                            light2.enabled = _flashlight;
                        }
                    }
                }
                else if (PlayerManager.localPlayer != null)
                {
                    PlayerManager.localPlayer.GetComponent<WeaponManager>().flashlightEnabled = true;
                }
            }
            allEffects = new WeaponMod.WeaponModEffects
            {
                customProfile = mod_sights[cur_sight].effects.customProfile,
                zoomRecoilReduction = mod_sights[cur_sight].effects.zoomRecoilReduction,
                zoomFov = mod_sights[cur_sight].effects.zoomFov,
                zoomSlowdown = mod_sights[cur_sight].effects.zoomSlowdown,
                zoomSensitivity = mod_sights[cur_sight].effects.zoomSensitivity,
                zoomPositionOffset = mod_sights[cur_sight].effects.zoomPositionOffset,
                shootSound = mod_barrels[cur_barrel].effects.shootSound,
                zoomRecoilAnimScale = mod_sights[cur_sight].effects.zoomRecoilAnimScale,
                damageMultiplier = mod_barrels[cur_barrel].effects.damageMultiplier,
                overallRecoilReduction = mod_sights[cur_sight].effects.overallRecoilReduction * mod_barrels[cur_barrel].effects.overallRecoilReduction * mod_others[cur_other].effects.overallRecoilReduction,
                isLaser = (mod_sights[cur_sight].effects.isLaser || mod_others[cur_other].effects.isLaser),
                audioSourceRangeScale = mod_barrels[cur_barrel].effects.audioSourceRangeScale,
                counterText = mod_others[cur_other].effects.counterText,
                counterTemplate = mod_others[cur_other].effects.counterTemplate
            };
        }

        public void ChangeMod(ModPrefab.ModType type, int value, bool saveToStats, bool _flashlight)
        {
            if (type == ModPrefab.ModType.Sight)
            {
                cur_sight = Mathf.Clamp(value, 0, mod_sights.Length - 1);
            }
            if (type == ModPrefab.ModType.Barrel)
            {
                cur_barrel = Mathf.Clamp(value, 0, mod_barrels.Length - 1);
            }
            if (type == ModPrefab.ModType.Other)
            {
                cur_other = Mathf.Clamp(value, 0, mod_others.Length - 1);
            }
            RefreshMods(saveToStats, _flashlight);
        }

        private string ConvertToStat(int value, bool lessTheBetter)
        {
            string empty = string.Empty;
            bool flag = false;
            if (value < 0)
            {
                flag = lessTheBetter;
                empty = "-" + Mathf.Abs(value) + "%";
            }
            else
            {
                flag = !lessTheBetter;
                empty = "+" + Mathf.Abs(value) + "%";
            }
            string text = ((!flag) ? "red" : "green");
            return "<color=" + text + ">" + empty + "</color>";
        }

        public string GetStats(ModPrefab.ModType type, int id)
        {
            string text = string.Empty;
            switch (type)
            {
                case ModPrefab.ModType.Barrel:
                    {
                        int num2 = Mathf.RoundToInt((mod_barrels[id].effects.damageMultiplier - 1f) * 100f);
                        int num3 = Mathf.RoundToInt((mod_barrels[id].effects.audioSourceRangeScale - 1f) * 100f);
                        int num4 = Mathf.RoundToInt((mod_barrels[id].effects.overallRecoilReduction - 1f) * 100f);
                        if (num2 != 0)
                        {
                            text = text + "Damage " + ConvertToStat(num2, false) + "\n";
                        }
                        if (num3 != 0)
                        {
                            text = text + "Shot loudness " + ConvertToStat(num3, true) + "\n";
                        }
                        if (num4 != 0)
                        {
                            text = text + "Recoil " + ConvertToStat(num4, true) + "\n";
                        }
                        break;
                    }
                case ModPrefab.ModType.Sight:
                    {
                        int num5 = Mathf.RoundToInt((mod_sights[id].effects.zoomRecoilReduction - 1f) * 100f);
                        bool flag = mod_sights[id].effects.customProfile != null;
                        float num6 = ((!flag) ? (Mathf.Round(70f / mod_sights[id].effects.zoomFov * 100f) / 100f) : 1f);
                        if (num5 != 0)
                        {
                            text = text + "Recoil while zooming " + ConvertToStat(num5, true) + "\n";
                        }
                        if (flag)
                        {
                            text += "<color=green>Telescopic-type sight</color>\n";
                        }
                        if (num6 != 1f)
                        {
                            string text2 = text;
                            text = text2 + "Zoom scale <color=green>" + num6 + "</color>";
                        }
                        break;
                    }
                default:
                    {
                        int num = Mathf.RoundToInt((mod_others[id].effects.overallRecoilReduction - 1f) * 100f);
                        if (num != 0)
                        {
                            text = text + "Overall recoil " + ConvertToStat(num, true) + "\n";
                        }
                        break;
                    }
            }
            if (string.IsNullOrEmpty(text))
            {
                text = "No effects";
            }
            return text;
        }

        public int GetMod(ModPrefab.ModType type)
        {
            switch (type)
            {
                case ModPrefab.ModType.Sight:
                    return cur_sight;
                case ModPrefab.ModType.Barrel:
                    return cur_barrel;
                case ModPrefab.ModType.Other:
                    return cur_other;
                default:
                    return 0;
            }
        }
    }

    [SyncVar]
    private bool friendlyFire;

    [SyncVar(hook = "HookCurWeapon")]
    public int curWeapon = -1;

    [SyncVar]
    private int sync_sight;

    [SyncVar]
    private int sync_barrel;

    [SyncVar]
    private int sync_other;

    [SyncVar]
    private bool sync_flashlight;

    private CharacterClassManager ccm;

    private BloodDrawer drawer;

    private Inventory inv;

    private AmmoBox abox;

    private WeaponShootAnimation weaponShootAnimation;

    private FirstPersonController fpc;

    private AnimationController animationController;

    private KeyCode kc_fire;

    private KeyCode kc_reload;

    private KeyCode kc_zoom;

    private float fireCooldown;

    private float reloadCooldown;

    private float zoomCooldown;

    public float normalFov = 70f;

    public Transform camera;

    public Transform weaponInventoryGroup;

    public Camera weaponModelCamera;

    public float fovAdjustingSpeed;

    public bool zoomed;

    public AnimationCurve viewBob;

    public float overallDamagerFactor = 1.65f;

    public LayerMask raycastMask;

    public LayerMask bloodMask;

    public HitboxIdentity[] hitboxes;

    public Weapon[] weapons;

    public bool flashlightEnabled = true;

    public bool forceSyncModsNextFrame;

    private int prevSyncWeapon;
    private void HookCurWeapon(int i)
    {
        curWeapon = i;
    }

    private void Start()
    {
        abox = GetComponent<AmmoBox>();
        fpc = GetComponent<FirstPersonController>();
        inv = GetComponent<Inventory>();
        animationController = GetComponent<AnimationController>();
        weaponShootAnimation = GetComponentInChildren<WeaponShootAnimation>();
        drawer = GetComponent<BloodDrawer>();
         friendlyFire = ConfigFile.ServerConfig.GetBool("friendly_fire");
        ccm = GetComponent<CharacterClassManager>();
        kc_fire = NewInput.GetKey("Shoot");
        kc_reload = NewInput.GetKey("Reload");
        kc_zoom = NewInput.GetKey("Zoom");
        if (base.isLocalPlayer)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].SetMods(PlayerPrefs.GetInt("Weapon_" + weapons[i].inventoryID + "_sight", 0), PlayerPrefs.GetInt("Weapon_" + weapons[i].inventoryID + "_barrel", 0), PlayerPrefs.GetInt("Weapon_" + weapons[i].inventoryID + "_other", 0), true, flashlightEnabled);
            }
        }
        else
        {
            UnityEngine.Object.Destroy(weaponModelCamera.gameObject);
        }
    }

    private void Update()
    {
        DeductCooldown();
        if (base.isLocalPlayer)
        {
            CheckForInput();
            UpdateFov();
            SetupCameras();
            RefreshPositions();
        }
    }

    private void LateUpdate()
    {
        SyncMods();
        if (base.isLocalPlayer && !Cursor.visible && Input.GetKeyDown(NewInput.GetKey("Toggle flashlight")) && curWeapon >= 0 && weapons[curWeapon].mod_others[weapons[curWeapon].GetMod(ModPrefab.ModType.Other)].name.ToLower().Contains("flashlight"))
        {
            flashlightEnabled = !flashlightEnabled;
        }
    }

    private void SyncMods()
    {
        if (curWeapon < 0)
        {
            return;
        }
        int mod = weapons[curWeapon].GetMod(ModPrefab.ModType.Sight);
        int mod2 = weapons[curWeapon].GetMod(ModPrefab.ModType.Barrel);
        int mod3 = weapons[curWeapon].GetMod(ModPrefab.ModType.Other);
        if (forceSyncModsNextFrame || prevSyncWeapon != curWeapon || sync_sight != mod || sync_barrel != mod2 || sync_other != mod3 || flashlightEnabled != sync_flashlight)
        {
            if (base.isLocalPlayer)
            {
                CmdSyncMods(mod, mod2, mod3, flashlightEnabled);
                weapons[curWeapon].RefreshMods(false, flashlightEnabled);
            }
            else
            {
                flashlightEnabled = sync_flashlight;
                weapons[curWeapon].SetMods(sync_sight, sync_barrel, sync_other, false, flashlightEnabled);
            }
            prevSyncWeapon = curWeapon;
        }
    }

    private void DeductCooldown()
    {
        if (fireCooldown >= 0f)
        {
            fireCooldown -= Time.deltaTime;
        }
        if (reloadCooldown >= 0f)
        {
            reloadCooldown -= Time.deltaTime;
        }
        if (zoomCooldown >= 0f)
        {
            zoomCooldown -= Time.deltaTime;
        }
    }

    [ClientCallback]
    private void UpdateFov()
    {
        if (NetworkClient.active)
        {
            float zoomFov = normalFov;
            bool flag = curWeapon >= 0 && weapons[curWeapon].allEffects.customProfile != null;
            if (curWeapon >= 0 && zoomed && !flag)
            {
                zoomFov = weapons[curWeapon].allEffects.zoomFov;
            }
            camera.GetComponent<Camera>().fieldOfView = ((!flag) ? Mathf.Lerp(camera.GetComponent<Camera>().fieldOfView, zoomFov, Time.deltaTime * fovAdjustingSpeed) : zoomFov);
        }
    }

    [ClientCallback]
    private void RefreshPositions()
    {
        if (NetworkClient.active && curWeapon >= 0)
        {
            Vector3 positionOffset = weapons[curWeapon].positionOffset;
            if (zoomed)
            {
                positionOffset += weapons[curWeapon].allEffects.zoomPositionOffset;
            }
            else
            {
                positionOffset += camera.transform.localPosition * (viewBob.Evaluate(new Vector3(fpc.m_MoveDir.x, 0f, fpc.m_MoveDir.z).magnitude) * weapons[curWeapon].bobAnimationScale);
            }
            weaponInventoryGroup.localPosition = Vector3.Lerp(weaponInventoryGroup.localPosition, positionOffset, Time.deltaTime * 8f);
        }
    }

    [ClientCallback]
    private void SetZoom(bool b)
    {
        if (!NetworkClient.active)
        {
            return;
        }
        bool flag = false;
        if (curWeapon >= 0 && weapons[curWeapon].allowZoom)
        {
            if (b != zoomed && fireCooldown <= 0f)
            {
                fireCooldown += weapons[curWeapon].zoomingTime;
                zoomCooldown = weapons[curWeapon].zoomingTime;
                zoomed = b;
                flag = true;
            }
        }
        else if (zoomed)
        {
            flag = true;
            zoomed = false;
        }
        if (curWeapon >= 0)
        {
            if (flag)
            {
                inv.availableItems[inv.curItem].firstpersonModel.GetComponent<Animator>().SetBool("Zoomed", zoomed);
                fpc.zoomSlowdown = ((!zoomed) ? 1f : weapons[curWeapon].allEffects.zoomSlowdown);
            }
        }
        else
        {
            fpc.zoomSlowdown = 1f;
        }
    }

    public int AmmoLeft()
    {
        if (curWeapon >= 0)
        {
            return (int)inv.items[inv.GetItemIndex()].durability;
        }
        return -1;
    }

    [ClientCallback]
    private void SetupCameras()
    {
        if (!NetworkClient.active)
        {
            return;
        }
        fpc.m_MouseLook.sensitivityMultiplier = 1f;
        if (ccm.curClass < 0)
        {
            return;
        }
        weaponModelCamera.nearClipPlane = 0.01f;
        PostProcessVolume component = weaponModelCamera.GetComponent<PostProcessVolume>();
        component.profile = ccm.klasy[ccm.curClass].postprocessingProfile;
        if (curWeapon >= 0)
        {
            if (weapons[curWeapon].allEffects.counterText != null)
            {
                weapons[curWeapon].allEffects.counterText.text = string.Format(weapons[curWeapon].allEffects.counterTemplate, AmmoLeft(), weapons[curWeapon].maxAmmo, abox.GetAmmo(weapons[curWeapon].ammoType)).Replace("\\n", Environment.NewLine);
            }
            if (zoomed && zoomCooldown <= 0f)
            {
                fpc.m_MouseLook.sensitivityMultiplier = weapons[curWeapon].allEffects.zoomSensitivity;
                if (weapons[curWeapon].allEffects.customProfile != null)
                {
                    component.profile = weapons[curWeapon].allEffects.customProfile;
                    camera.GetComponent<Camera>().fieldOfView = weapons[curWeapon].allEffects.zoomFov;
                    weaponModelCamera.nearClipPlane = 3.5f;
                }
            }
        }
        Inventory.targetCrosshairAlpha = ((!zoomed && (curWeapon < 0 || !weapons[curWeapon].allEffects.isLaser)) ? 1 : 0);
    }

    [ClientCallback]
    private void CheckForInput()
    {
        if (!NetworkClient.active)
        {
            return;
        }
        if (!Cursor.visible && Inventory.inventoryCooldown <= 0f && fireCooldown <= 0f && (reloadCooldown <= 0f || zoomed))
        {
            SetZoom(Input.GetKey(NewInput.GetKey("Zoom")));
        }
        if (curWeapon >= 0 && reloadCooldown <= 0f && !Cursor.visible && Inventory.inventoryCooldown <= 0f && fireCooldown <= 0f)
        {
            if ((!weapons[curWeapon].allowFullauto) ? Input.GetKeyDown(kc_fire) : Input.GetKey(kc_fire))
            {
                Shoot();
            }
            else if (Input.GetKey(kc_reload))
            {
                StartCoroutine(_Reload());
            }
        }
    }

    [ClientCallback]
    private void Shoot()
    {
        if (!NetworkClient.active || inv.items[inv.GetItemIndex()].durability == 0f)
        {
            return;
        }
        fireCooldown = 1f / weapons[curWeapon].shotsPerSecond;
        inv.availableItems[inv.curItem].firstpersonModel.GetComponent<Animator>().SetTrigger("Shoot");
        animationController.gunSource.PlayOneShot(weapons[curWeapon].allEffects.shootSound);
        camera.GetComponent<Camera>().fieldOfView -= weapons[curWeapon].recoilAnimation * weapons[curWeapon].recoil.fovKick;
        weapons[curWeapon].PlayMuzzleFlashes(true);
        weapons[curWeapon].husks.Play();
        Vector3 vector = camera.transform.forward;
        if (!zoomed)
        {
            vector = Quaternion.Euler(new Vector3(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1)) * (weapons[curWeapon].unfocusedSpread / 5f)) * vector;
        }
        Ray ray = new Ray(camera.transform.position, vector);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 500f, raycastMask))
        {
            HitboxIdentity component = hitInfo.collider.GetComponent<HitboxIdentity>();
            if (component != null)
            {
                DoRecoil();
                CmdShoot(component.GetComponentInParent<NetworkIdentity>().gameObject, component.id, ray.direction);
            }
            else
            {
                DoRecoil();
                CmdShoot(null, "hole", ray.direction);
            }
        }
        else
        {
            DoRecoil();
            CmdShoot(null, string.Empty, Vector3.zero);
        }
    }

    private void DoRecoil()
    {
        weaponShootAnimation.Recoil(weapons[curWeapon].recoilAnimation * ((!zoomed) ? 1f : weapons[curWeapon].allEffects.zoomRecoilAnimScale));
        Recoil.StaticDoRecoil(weapons[curWeapon].recoil, weapons[curWeapon].allEffects.overallRecoilReduction * ((!zoomed) ? 1f : weapons[curWeapon].allEffects.zoomRecoilReduction));
    }

    [Command]
    private void CmdSyncMods(int _s, int _b, int _o, bool _flashlight)
    {
        sync_sight = _s;
        sync_barrel = _b;
        sync_other = _o;
        sync_flashlight = _flashlight;
    }

    [Command]
    private void CmdShoot(GameObject target, string hitboxType, Vector3 dir)
    {
        if (curWeapon < 0 || ((!(reloadCooldown <= 0f) || !(fireCooldown <= 0f)) && !base.isLocalPlayer) || inv.curItem != weapons[curWeapon].inventoryID || inv.items[inv.GetItemIndex()].durability <= 0f)
        {
            return;
        }
        inv.items.ModifyDuration(inv.GetItemIndex(), inv.items[inv.GetItemIndex()].durability - 1f);
        fireCooldown = 1f / weapons[curWeapon].shotsPerSecond * 0.8f;
        CharacterClassManager characterClassManager = null;
        if (target != null)
        {
            characterClassManager = target.GetComponent<CharacterClassManager>();
        }
        float audioSourceRangeScale = weapons[curWeapon].allEffects.audioSourceRangeScale;
        audioSourceRangeScale = audioSourceRangeScale / 2f * 70f;
        GetComponent<Scp939_VisionController>().MakeNoise(Mathf.Clamp(audioSourceRangeScale, 5f, 100f));
        if (characterClassManager != null && GetShootPermission(characterClassManager))
        {
            float num = Vector3.Distance(camera.transform.position, target.transform.position);
            float num2 = weapons[curWeapon].damageOverDistance.Evaluate(num);
            switch (hitboxType.ToUpper())
            {
                case "HEAD":
                    num2 *= 4f;
                    break;
                case "LEG":
                    num2 /= 2f;
                    break;
                case "SCP106":
                    num2 /= 10f;
                    break;
            }
            num2 *= weapons[curWeapon].allEffects.damageMultiplier;
            num2 *= overallDamagerFactor;
            GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(num2, ccm.SteamId + " (" + ccm.GetComponent<NicknameSync>().myNick + ")", "Weapon:" + curWeapon, GetComponent<QueryProcessor>().PlayerId), characterClassManager.gameObject);
            RpcConfirmShot(true, curWeapon);
            PlaceDecal(true, new Ray(camera.position, dir), characterClassManager.curClass, num);
        }
        else
        {
            PlaceDecal(false, new Ray(camera.position, dir), curWeapon, 0f);
            RpcConfirmShot(false, curWeapon);
        }
    }

    [Command]
    private void CmdReload(bool animationOnly)
    {
        if (curWeapon < 0 || inv.curItem != weapons[curWeapon].inventoryID || !(inv.items[inv.GetItemIndex()].durability < (float)weapons[curWeapon].maxAmmo))
        {
            return;
        }
        if (animationOnly)
        {
            RpcReload(curWeapon);
            return;
        }
        int ammoType = weapons[curWeapon].ammoType;
        int num = abox.GetAmmo(ammoType);
        int num2 = (int)inv.items[inv.GetItemIndex()].durability;
        int maxAmmo = weapons[curWeapon].maxAmmo;
        while (num > 0 && num2 < maxAmmo)
        {
            num--;
            num2++;
        }
        inv.items.ModifyDuration(inv.GetItemIndex(), num2);
        abox.SetOneAmount(ammoType, num.ToString());
    }

    [ServerCallback]
    private void PlaceDecal(bool isBlood, Ray ray, int classId, float distanceAddition)
    {
        RaycastHit hitInfo;
        if (NetworkServer.active && Physics.Raycast(ray, out hitInfo, (!isBlood) ? 100f : (10f + distanceAddition), bloodMask) && classId >= 0)
        {
            RpcPlaceDecal(isBlood, (!isBlood) ? classId : ccm.klasy[classId].bloodType, hitInfo.point + hitInfo.normal * 0.01f, Quaternion.FromToRotation(Vector3.up, hitInfo.normal));
        }
    }

    [ClientRpc]
    private void RpcPlaceDecal(bool isBlood, int type, Vector3 pos, Quaternion rot)
    {
        if (isBlood)
        {
            drawer.DrawBlood(pos, rot, type);
            return;
        }
        GameObject gameObject;
        UnityEngine.Object.Destroy(gameObject = UnityEngine.Object.Instantiate(weapons[type].holeEffect), 4f);
        gameObject.transform.position = pos;
        gameObject.transform.rotation = rot;
        gameObject.transform.localScale = Vector3.one;
    }

    [ClientRpc]
    private void RpcConfirmShot(bool hitmarker, int weapon)
    {
        if (base.isLocalPlayer)
        {
            if (hitmarker)
            {
                Hitmarker.Hit();
            }
        }
        else if (animationController != null)
        {
            animationController.DoAnimation("Shoot");
            weapons[curWeapon].PlayMuzzleFlashes(false);
            animationController.gunSource.maxDistance = 80f * ((curWeapon < 0) ? 1f : weapons[curWeapon].allEffects.audioSourceRangeScale);
            animationController.gunSource.PlayOneShot(weapons[weapon].allEffects.shootSound);
        }
    }

    [ClientRpc]
    private void RpcReload(int weapon)
    {
        if (!base.isLocalPlayer && reloadCooldown <= 0f)
        {
            animationController.DoAnimation("Reload");
            StartCoroutine(_ReloadRpc(weapon));
        }
    }

    private IEnumerator _Reload()
    {
        if (!(inv.items[inv.GetItemIndex()].durability < (float)weapons[curWeapon].maxAmmo) || abox.GetAmmo(weapons[curWeapon].ammoType) <= 0 || zoomed)
        {
            yield break;
        }
        Animator a = inv.availableItems[inv.curItem].firstpersonModel.GetComponent<Animator>();
        int w = curWeapon;
        animationController.gunSource.PlayOneShot(weapons[curWeapon].reloadClip);
        reloadCooldown = weapons[w].reloadingTime;
        a.SetBool("Reloading", true);
       CmdReload(true);
        while (reloadCooldown > 0.4f)
        {
            if (w != curWeapon)
            {
                a.SetBool("Reloading", false);
                animationController.gunSource.Stop();
                reloadCooldown = 0f;
                yield break;
            }
            yield return 0f;
        }
        a.SetBool("Reloading", false);
        CmdReload(false);
    }

    private IEnumerator _ReloadRpc(int weapon)
    {
        reloadCooldown = weapons[weapon].reloadingTime;
        AudioSource s = animationController.gunSource;
        s.maxDistance = 15f;
        s.PlayOneShot(weapons[weapon].reloadClip);
        while (reloadCooldown > 0f)
        {
            if (curWeapon != weapon)
            {
                s.Stop();
                reloadCooldown = 0f;
            }
            yield return 0f;
        }
    }

    public bool GetShootPermission(Team target, bool forceFriendlyFire = false)
    {
        if (ccm.curClass == 2 || ccm.klasy[ccm.curClass].team == Team.SCP)
        {
            return false;
        }
        if (friendlyFire && !forceFriendlyFire)
        {
            return true;
        }
        Team team = ccm.klasy[ccm.curClass].team;
        if ((team == Team.MTF || team == Team.RSC) && (target == Team.MTF || target == Team.RSC))
        {
            return false;
        }
        if ((team == Team.CDP || team == Team.CHI) && (target == Team.CDP || target == Team.CHI))
        {
            return false;
        }
        if ((team == Team.SH || team == Team.SCP) && (target == Team.SH || target == Team.SCP))
        {
            return false;
        }
        return true;
    }

    public bool GetShootPermission(CharacterClassManager c, bool forceFriendlyFire = false)
    {
        return GetShootPermission(c.klasy[c.curClass].team, forceFriendlyFire);
    }
}
