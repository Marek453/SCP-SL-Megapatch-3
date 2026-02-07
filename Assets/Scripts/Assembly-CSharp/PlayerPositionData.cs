using RemoteAdmin;
using UnityEngine;

public struct PlayerPositionData
{
	public Vector3 position;

	public float rotation;

	public int playerID;

	public PlayerPositionData(Vector3 _pos, float _rotY, int _id)
	{
		position = _pos;
		rotation = _rotY;
		playerID = _id;
	}

	public PlayerPositionData(GameObject _player)
	{
		playerID = _player.GetComponent<QueryProcessor>().PlayerId;
		PlyMovementSync component = _player.GetComponent<PlyMovementSync>();
		position = component.position;
		rotation = component.rotation;
	}
}
