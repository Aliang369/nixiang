using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ShowMapFogPanel : UIBase
{
	[SerializeField]
	private Image enterFogImage;

	[SerializeField]
	private Image enterDarkenImage;

	[NonSerialized]
	private float fadeFactor;

	[NonSerialized]
	private Sequence sequence;

	protected override void Init()
	{
		enterFogImage.material = new Material(enterFogImage.material);
	}

	public IEnumerator ShowFog()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIBase.applicationState.settingManager.GetUIScale();
		DungeonMapDisplay.Config dungeonMapDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig;
		sequence.AppendCallback(delegate
		{
			SetEnterFogFadeFactor(1f);
			Color color = enterDarkenImage.color;
			color.a = 0f;
			enterDarkenImage.color = color;
			enterFogImage.gameObject.SetActive(value: true);
			enterDarkenImage.gameObject.SetActive(value: true);
		});
		sequence.Append(DOTween.To(() => fadeFactor, SetEnterFogFadeFactor, 0f, dungeonMapDisplayConfig.fogFadeTime));
		sequence.Insert(0f, DOTweenModuleUI.DOFade(enterDarkenImage, 1f, dungeonMapDisplayConfig.darkenTime));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
		yield return sequence.WaitForKill();
	}

	public IEnumerator HideFog()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = UIBase.applicationState.settingManager.GetUIScale();
		DungeonMapDisplay.Config dungeonMapDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig;
		sequence.AppendCallback(delegate
		{
			SetEnterFogFadeFactor(0f);
			Color color = enterDarkenImage.color;
			color.a = 1f;
			enterDarkenImage.color = color;
			enterFogImage.gameObject.SetActive(value: true);
			enterDarkenImage.gameObject.SetActive(value: true);
		});
		sequence.Append(DOTween.To(() => fadeFactor, SetEnterFogFadeFactor, 1f, dungeonMapDisplayConfig.fogFadeTime));
		sequence.Insert(0f, DOTweenModuleUI.DOFade(enterDarkenImage, 1f, 0f));
		sequence.Insert(dungeonMapDisplayConfig.darkenTime - dungeonMapDisplayConfig.fogFadeTime, DOTweenModuleUI.DOFade(enterDarkenImage, 0f, dungeonMapDisplayConfig.darkenTime));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			enterFogImage.gameObject.SetActive(value: false);
		});
		Sequence obj2 = sequence;
		obj2.onKill = (TweenCallback)Delegate.Combine(obj2.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
		yield return sequence?.WaitForKill();
	}

	private void SetEnterFogFadeFactor(float f)
	{
		fadeFactor = f;
		enterFogImage.materialForRendering.SetFloat(ShaderFields._FadeFactor, f);
	}
}
