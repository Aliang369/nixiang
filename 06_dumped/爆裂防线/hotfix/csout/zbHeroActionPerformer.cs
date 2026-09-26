using System;
using System.Collections;
using UnityEngine;

public class zbHeroActionPerformer
{
	private MonoBehaviour runner;

	private Coroutine actionCoroutine;

	private bool isRunning;

	private float actionTime;

	private float progressValue;

	private Vector3 progressPosition;

	private Action onComplete;

	public bool IsRunning => isRunning;

	public float Progress => progressValue;

	public Vector3 ProgressPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return progressPosition;
		}
	}

	public zbHeroActionPerformer(MonoBehaviour runner)
	{
		this.runner = runner;
	}

	public void StartAction(float time, Vector3 progressPos, Action onComplete)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (isRunning)
		{
			Stop();
		}
		actionTime = time;
		progressPosition = progressPos;
		this.onComplete = onComplete;
		progressValue = 0f;
		isRunning = true;
		actionCoroutine = runner.StartCoroutine(ActionCoroutine());
	}

	public void Stop()
	{
		if (isRunning && actionCoroutine != null)
		{
			runner.StopCoroutine(actionCoroutine);
			actionCoroutine = null;
			isRunning = false;
		}
	}

	private IEnumerator ActionCoroutine()
	{
		float timer = 0f;
		while (timer < actionTime)
		{
			timer += Time.unscaledDeltaTime;
			progressValue = Mathf.Clamp01(timer / actionTime) * 100f;
			yield return null;
		}
		isRunning = false;
		actionCoroutine = null;
		onComplete?.Invoke();
	}
}
