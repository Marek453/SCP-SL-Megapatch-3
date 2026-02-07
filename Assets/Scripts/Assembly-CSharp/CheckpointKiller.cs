using UnityEngine;

public class CheckpointKiller : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		CharacterClassManager component = other.GetComponent<CharacterClassManager>();
		if (component != null && component.isLocalPlayer)
		{
			component.CmdSuicide(new PlayerStats.HitInfo(99999f, "WORLD", "WALL", 0));
		}
	}
}
