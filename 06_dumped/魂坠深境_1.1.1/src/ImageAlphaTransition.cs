using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageAlphaTransition : MonoBehaviour
{
	public Image targetImage;

	public float duration = 0.4f;

	public AnimationCurve alphaCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

	private Coroutine currentCoroutine;

	public void OnButtonClick()
	{
		if (currentCoroutine != null)
		{
			StopCoroutine(currentCoroutine);
		}
		currentCoroutine = StartCoroutine(ChangeAlpha());
	}

	private IEnumerator ChangeAlpha()
	{
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			float time = elapsedTime / duration;
			float a = alphaCurve.Evaluate(time);
			Color color = targetImage.color;
			color.a = a;
			targetImage.color = color;
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		float a2 = alphaCurve.Evaluate(1f);
		Color color2 = targetImage.color;
		color2.a = a2;
		targetImage.color = color2;
		currentCoroutine = null;
	}
}
