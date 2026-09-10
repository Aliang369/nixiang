using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenColorFadeAction : GameAction
{
	public Camera camera;

	public AnimationCurve curve;

	private readonly bool delay;

	public Color targetColor;

	public float time;

	public ScreenColorFadeAction(Camera camera, Color targetColor, AnimationCurve curve, float time, bool delay)
	{
		this.camera = camera;
		this.targetColor = targetColor;
		this.curve = curve;
		this.time = time;
		this.delay = delay;
	}

	public IEnumerator LerpCameraChangeColor(Color targetColor, float time, AnimationCurve curve, ColorAdjustments ca)
	{
		Color initColor = ca.colorFilter.value;
		float timer = 0f;
		while (!(timer > time))
		{
			float t = curve.Evaluate(timer / time);
			ca.colorFilter.Interp(initColor, targetColor, t);
			timer += Time.deltaTime * base.actionTimeScale;
			yield return new WaitForEndOfFrame();
		}
		if (delay)
		{
			FinishAction();
		}
	}

	protected override void Update()
	{
		Volume component = camera.GetComponent<Volume>();
		if (component == null)
		{
			FinishAction();
			return;
		}
		if (component.profile.TryGet<ColorAdjustments>(out ColorAdjustments component2))
		{
			component.StartCoroutine(LerpCameraChangeColor(targetColor, time, curve, component2));
		}
		if (!delay)
		{
			FinishAction();
		}
	}
}
