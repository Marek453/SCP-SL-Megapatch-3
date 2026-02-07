using UnityEngine;

public class CCTV_Camera : MonoBehaviour
{
	public Transform cameraTarget;

	public Transform camera079;

	public Transform spriteHolder;

	public MeshRenderer spriteRenderer;

	public Material mat;

	public AnimationCurve transparencyOverDistance;

	public AnimationCurve scaleOverDistance;

	public string liftID;

	private void Start()
	{
		spriteRenderer.material = new Material(mat);
	}

	public void UpdateLOD()
	{
		float time = Vector3.Distance(camera079.position, base.transform.position);
		spriteRenderer.material.color = new Color(spriteRenderer.material.color.r, spriteRenderer.material.color.g, spriteRenderer.material.color.b, transparencyOverDistance.Evaluate(time));
		spriteHolder.transform.localScale = Vector3.one * scaleOverDistance.Evaluate(time);
		spriteHolder.transform.LookAt(camera079);
		GetComponent<SphereCollider>().radius = Mathf.Clamp(spriteHolder.transform.localScale.x / 2f, 0.13f, 10f);
	}
}
