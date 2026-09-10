using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class VignetteAction : GameAction
{
	private Sequence backTween;

	private bool delay;

	private bool finishFlag;

	private float intensity;

	private bool returnToDefault;

	private Color targetColor;

	private Sequence toTween;

	private float tweenTime;

	public VignetteAction(Color targetColor, float intensity, float tweenTime, bool returnToDefault = true, bool delay = false)
	{
		this.targetColor = targetColor;
		this.intensity = intensity;
		this.tweenTime = tweenTime;
		this.returnToDefault = returnToDefault;
		this.delay = delay;
	}

	protected override void Update()
	{
		FinishAction();
		if (toTween == null)
		{
			FastPostProcessRenderFeature.FastPostProcessSettings settings = gameState.applicationState.renderingManager.overrideSettings ?? gameState.applicationState.renderingManager.defaultSettings;
			settings.vignette = true;
			toTween = DOTween.Sequence();
			toTween.Append(DOTween.To(() => settings.vignetteColor, delegate(Color c)
			{
				settings.vignetteColor = c;
			}, targetColor, tweenTime));
			toTween.Join(DOTween.To(() => settings.vignetteAmount, delegate(float c)
			{
				settings.vignetteAmount = c;
			}, intensity, tweenTime));
			toTween.SetEase(Ease.InQuad);
			toTween.AppendCallback(delegate
			{
				if (returnToDefault)
				{
					backTween = DOTween.Sequence();
					backTween.timeScale = base.actionTimeScale;
					backTween.Append(DOTween.To(() => settings.vignetteColor, delegate(Color c)
					{
						settings.vignetteColor = c;
					}, ScriptableObjectSingleton<VFXDataPreset>.instance.defaultVignetteColor, tweenTime));
					backTween.Join(DOTween.To(() => settings.vignetteAmount, delegate(float c)
					{
						settings.vignetteAmount = c;
					}, ScriptableObjectSingleton<VFXDataPreset>.instance.defaultVignetteIntensity, tweenTime));
					backTween.SetEase(Ease.InQuad);
					backTween.AppendCallback(delegate
					{
						finishFlag = true;
						settings.vignette = false;
					});
					backTween.Play();
				}
				else
				{
					finishFlag = true;
					settings.vignette = false;
				}
			});
			toTween.Play();
		}
		if (!delay || finishFlag)
		{
			FinishAction();
		}
	}
}
