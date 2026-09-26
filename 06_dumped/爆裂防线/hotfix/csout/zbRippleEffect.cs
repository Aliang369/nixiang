using System;
using UnityEngine;

public class zbRippleEffect : MonoBehaviour
{
	public float duration = 1.5f;

	public float minScale = 0.5f;

	public float maxScale = 0.8f;

	public float maxAlpha = 0.5f;

	private SpriteRenderer sr;

	private float timer;

	private void Start()
	{
		sr = ((Component)this).GetComponent<SpriteRenderer>();
		if ((Object)(object)sr == (Object)null)
		{
			sr = ((Component)this).gameObject.AddComponent<SpriteRenderer>();
		}
		timer = Random.Range(0f, duration);
	}

	private void Update()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		timer += Time.deltaTime;
		if (timer > duration)
		{
			timer -= duration;
		}
		float num = timer / duration;
		float num2 = Mathf.Lerp(minScale, maxScale, num);
		((Component)this).transform.localScale = new Vector3(num2, num2, 1f);
		float a = Mathf.Sin(num * (float)Math.PI) * maxAlpha;
		Color color = sr.color;
		color.a = a;
		sr.color = color;
	}
}
