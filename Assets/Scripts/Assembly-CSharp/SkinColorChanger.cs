using UnityEngine;

public class SkinColorChanger : MonoBehaviour
{
	public Material ci;

	public Material mtf;

	public Material classd;

	public Material scientist;

	public Material guard;

	private int lastClass = -1;

	private void OnEnable()
	{
		Renderer component = GetComponent<SkinnedMeshRenderer>();
		CharacterClassManager componentInParent = GetComponentInParent<CharacterClassManager>();
		if (lastClass == componentInParent.curClass)
		{
			return;
		}
		lastClass = componentInParent.curClass;
		if (componentInParent.klasy[componentInParent.curClass].team == Team.MTF)
		{
			if (componentInParent.curClass == 15)
			{
				component.sharedMaterial = guard;
			}
			else
			{
				component.sharedMaterial = mtf;
			}
		}
		else if (componentInParent.klasy[componentInParent.curClass].team == Team.CHI)
		{
			component.sharedMaterial = ci;
		}
		else if (componentInParent.klasy[componentInParent.curClass].team == Team.RSC)
		{
			component.sharedMaterial = scientist;
		}
		else
		{
			component.sharedMaterial = classd;
		}
	}
}
