using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public static class MShortcutExtensions
{
	public static TweenerCore<Quaternion, Vector3, QuaternionOptions> DOLocalRotate(this Transform target, Vector3 endValue, float duration, RotateMode mode = RotateMode.Fast)
	{
		TweenerCore<Quaternion, Vector3, QuaternionOptions> tweenerCore = DOTween.To(() => target.localRotation, delegate(Quaternion x)
		{
			target.localRotation = x;
		}, endValue, duration);
		tweenerCore.SetTarget(target);
		tweenerCore.plugOptions.rotateMode = mode;
		return tweenerCore;
	}

	public static TweenerCore<float, float, FloatOptions> DOShaderFactor(this Material mat, int factorNameID, float endValue, float duration)
	{
		return DOTween.To(() => mat.GetFloat(factorNameID), delegate(float x)
		{
			mat.SetFloat(factorNameID, x);
		}, endValue, duration);
	}

	public static TweenerCore<float, float, FloatOptions> DOShaderFactor(this Material mat, int factorNameID, AnimationCurve curve, float duration)
	{
		float elapsedTime = 0f;
		return DOTween.To(() => mat.GetFloat(factorNameID), delegate(float x)
		{
			elapsedTime = x;
			mat.SetFloat(factorNameID, curve.Evaluate(x / duration));
		}, duration, duration);
	}

	public static TweenerCore<float, float, FloatOptions> DOShaderColor(this Material mat, int factorNameID, AnimationCurve curve, Color color, float duration)
	{
		float elapsedTime = 0f;
		return DOTween.To(() => 0f, delegate(float x)
		{
			elapsedTime = x;
			float num = curve.Evaluate(x / duration);
			Color color2 = color;
			color2 *= num;
			color2.a = 1f;
			mat.SetVector(factorNameID, color2);
		}, duration, duration);
	}

	public static TweenerCore<float, float, FloatOptions> DOShaderColorAlpha(this Material mat, int factorNameID, float endValue, float duration)
	{
		return DOTween.To(() => mat.GetColor(factorNameID).a, delegate(float x)
		{
			mat.SetColor(factorNameID, mat.GetColor(factorNameID).SetA(x));
		}, endValue, duration);
	}

	public static TweenerCore<float, float, FloatOptions> DOTo(this Float f, float endValue, float duration)
	{
		return DOTween.To(() => f, delegate(float x)
		{
			f.Value = x;
		}, endValue, duration);
	}
}
