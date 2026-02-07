using AmplifyBloom;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityStandardAssets.ImageEffects;

public class WeaponCamera : MonoBehaviour
{
	private VignetteAndChromaticAberration vaca;

	private VignetteAndChromaticAberration myvaca;

	private PostProcessVolume ppbeh;

	private AmplifyBloomEffect bloom;
	private UnityEngine.Rendering.PostProcessing.Bloom Bloom;

	private void Start()
	{
		bloom = GetComponent<AmplifyBloomEffect>();
		ppbeh = GetComponent<PostProcessVolume>();
		myvaca = GetComponent<VignetteAndChromaticAberration>();
		vaca = GetComponentInParent<VignetteAndChromaticAberration>();
	}

	private void Update()
	{
		myvaca = vaca;
	}
}
