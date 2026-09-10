using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class TrigramsLineDisplay : UIBase
{
	[SerializeField]
	private Image image;

	[NonSerialized]
	private Sequence sequence;

	[NonSerialized]
	private int targetIndex;

	[NonSerialized]
	private int targetMax;

	protected override void Init()
	{
		image.material = new Material(image.material);
	}

	protected override void UpdateDisplay()
	{
		if (RefreshIndex())
		{
			UIBase.gameState.StartCoroutine(PlayTween_Shift);
		}
	}

	public void UpdateImmediately()
	{
		if (RefreshIndex())
		{
			SetFactor(GetTargetFactor());
		}
	}

	private bool RefreshIndex()
	{
		TrigramsManager trigramsManager = UIBase.gameState.trigramsManager;
		int num = trigramsManager.maxPower + 1;
		ProtectedInt yangPower = trigramsManager.yangPower;
		if (targetMax == num && targetIndex == (int)yangPower)
		{
			return false;
		}
		targetIndex = yangPower;
		targetMax = num;
		return true;
	}

	private void SetFactor(float value)
	{
		image.materialForRendering.SetFloat(ShaderFields._Factor, value);
	}

	private float GetFactor()
	{
		return image.materialForRendering.GetFloat(ShaderFields._Factor);
	}

	private IEnumerator PlayTween_Shift()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		float targetFactor = GetTargetFactor();
		TweenerCore<float, float, FloatOptions> t = DOTween.To(() => GetFactor(), delegate(float x)
		{
			SetFactor(x);
		}, targetFactor, 0.5f);
		t.SetOptions(snapping: false).SetTarget(targetFactor);
		sequence.Append(t);
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	private float GetTargetFactor()
	{
		int num = 60;
		if (targetMax == 0)
		{
			return 1f;
		}
		if (targetIndex == 0)
		{
			return 0f;
		}
		if (targetIndex == targetMax - 1)
		{
			return 1f;
		}
		float width = base.transform.parent.RectTransform().GetWidth();
		if (width < (float)((targetMax + 1) * num))
		{
			float num2 = width / (float)(targetMax + 1);
			return (float)(targetIndex + 1) * num2 / width;
		}
		int num3 = (targetMax + 1) * num;
		int num4 = num;
		return ((float)((targetIndex + 1) * num4) - 0.5f * (float)num3 + 0.5f * width) / width;
	}
}
