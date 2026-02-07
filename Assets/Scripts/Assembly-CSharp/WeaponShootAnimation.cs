using UnityEngine;

public class WeaponShootAnimation : MonoBehaviour
{
	public float curPosition;

	public Vector3 maxRecoilPos;

	public Vector3 maxRecoilRot;

	public float backSpeed;

	private void LateUpdate()
	{
		curPosition = Mathf.Lerp(curPosition, 0f, Time.deltaTime * backSpeed * curPosition);
		base.transform.localPosition = Vector3.Lerp(Vector3.zero, maxRecoilPos, curPosition);
		base.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.Euler(maxRecoilRot), curPosition);
	}

	public void Recoil(float f)
	{
		curPosition = Mathf.Clamp01(curPosition + f);
	}
}
