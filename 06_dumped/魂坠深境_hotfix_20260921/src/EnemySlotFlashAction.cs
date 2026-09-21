using System.Collections;
using UnityEngine;

public class EnemySlotFlashAction : GameAction
{
	public AnimationCurve curve;

	public float flashTime;

	public bool forceStop;

	private bool started;

	public Material targetMaterial;

	private float timeSinceStart;

	public EnemySlotFlashAction(Material targetMaterial, AnimationCurve curve, float flashTime)
	{
		this.targetMaterial = targetMaterial;
		this.curve = curve;
		this.flashTime = flashTime;
		forceStop = false;
		timeSinceStart = 0f;
	}

	protected override void Update()
	{
		if (!started)
		{
			SceneInfoSingleton.instance.StartCoroutine(Start());
		}
	}

	private IEnumerator Start()
	{
		while (timeSinceStart < flashTime)
		{
			timeSinceStart += Time.deltaTime * base.actionTimeScale;
			if (flashTime <= 0f || timeSinceStart > flashTime)
			{
				Finish();
			}
			float num = curve.Evaluate(timeSinceStart / flashTime);
			Color value = new Color(num, num, num);
			targetMaterial.SetColor("_AdditionalColor", value);
			yield return null;
		}
	}

	public void Finish()
	{
		targetMaterial.SetColor("_AdditionalColor", Color.black);
		FinishAction();
	}
}
