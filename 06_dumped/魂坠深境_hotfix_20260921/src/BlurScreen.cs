using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BlurScreen : CameraControlNode
{
	public AnimationCurve blurAmountCurve;

	public override Sequence ExecuteCor(ApplicationStateInfo applicationState)
	{
		FastPostProcessRenderFeature.FastPostProcessSettings settings = applicationState.renderingManager.overrideSettings.Value;
		settings.blurAmount = 0f;
		TweenerCore<float, float, FloatOptions> t = DOTween.To(() => settings.blurAmount, delegate(float v)
		{
			settings.blurAmount = v;
			applicationState.renderingManager.OverrideFastPostProcessSettings(settings);
		}, 1f, blurAmountCurve[blurAmountCurve.length - 1].time);
		if (blurAmountCurve != null)
		{
			t.SetEase(blurAmountCurve);
		}
		bool initBlur = settings.blur;
		Sequence sequence = DOTween.Sequence();
		sequence.AppendCallback(delegate
		{
			settings.blur = true;
		});
		sequence.Append(t);
		sequence.AppendCallback(delegate
		{
			settings.blur = initBlur;
			applicationState.renderingManager.OverrideFastPostProcessSettings(settings);
		});
		return sequence;
	}
}
