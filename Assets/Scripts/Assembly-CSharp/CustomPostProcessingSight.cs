using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CustomPostProcessingSight : MonoBehaviour
{
	[HideInInspector]
	public WeaponManager wm;

	[HideInInspector]
	public PostProcessVolume ppb;

	public GameObject canvas;

	public PostProcessProfile targetProfile;

	public static CustomPostProcessingSight singleton;

	public static bool raycast_bool;

	public static RaycastHit raycast_hit;

	public int GetAmmoLeft()
	{
		return wm.AmmoLeft();
	}

	public bool IsHumanHit()
	{
		return raycast_hit.collider.GetComponentInParent<CharacterClassManager>() == null;
	}
}
