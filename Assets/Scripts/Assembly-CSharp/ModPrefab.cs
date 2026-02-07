using UnityEngine;

public class ModPrefab : MonoBehaviour
{
	public enum ModType
	{
		Sight = 0,
		Barrel = 1,
		Other = 2
	}

	public string label;

	public int weaponId;

	public ModType modType;

	public int modId;

	public bool firstperson;

	public new GameObject gameObject;

	private void Start()
	{
		WeaponManager componentInParent = GetComponentInParent<WeaponManager>();
		componentInParent.forceSyncModsNextFrame = true;
		if (modType == ModType.Sight)
		{
			if (firstperson)
			{
				componentInParent.weapons[weaponId].mod_sights[modId].prefab_firstperson = gameObject;
			}
			else
			{
				componentInParent.weapons[weaponId].mod_sights[modId].prefab_thirdperson = gameObject;
			}
		}
		if (modType == ModType.Barrel)
		{
			if (firstperson)
			{
				componentInParent.weapons[weaponId].mod_barrels[modId].prefab_firstperson = gameObject;
			}
			else
			{
				componentInParent.weapons[weaponId].mod_barrels[modId].prefab_thirdperson = gameObject;
			}
		}
		if (modType == ModType.Other)
		{
			if (firstperson)
			{
				componentInParent.weapons[weaponId].mod_others[modId].prefab_firstperson = gameObject;
			}
			else
			{
				componentInParent.weapons[weaponId].mod_others[modId].prefab_thirdperson = gameObject;
			}
		}
	}
}
