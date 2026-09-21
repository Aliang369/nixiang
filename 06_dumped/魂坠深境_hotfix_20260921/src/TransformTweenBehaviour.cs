using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class TransformTweenBehaviour : PlayableBehaviour
{
	public enum TweenType
	{
		Linear,
		Deceleration,
		Harmonic,
		Custom
	}

	public Transform startLocation;

	public Transform endLocation;

	public bool tweenPosition = true;

	public bool tweenRotation = true;

	public TweenType tweenType;

	public AnimationCurve customCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public Vector3 startingPosition;

	public Quaternion startingRotation = Quaternion.identity;

	private AnimationCurve m_LinearCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private AnimationCurve m_DecelerationCurve = new AnimationCurve(new Keyframe(0f, 0f, -(float)Math.PI / 2f, (float)Math.PI / 2f), new Keyframe(1f, 1f, 0f, 0f));

	private AnimationCurve m_HarmonicCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private const float k_RightAngleInRads = (float)Math.PI / 2f;

	public override void PrepareFrame(Playable playable, FrameData info)
	{
		if ((bool)startLocation)
		{
			startingPosition = startLocation.position;
			startingRotation = startLocation.rotation;
		}
	}

	public float EvaluateCurrentCurve(float time)
	{
		if (tweenType == TweenType.Custom && !IsCustomCurveNormalised())
		{
			Debug.LogError("Custom Curve is not normalised.  Curve must start at 0,0 and end at 1,1.");
			return 0f;
		}
		return tweenType switch
		{
			TweenType.Linear => m_LinearCurve.Evaluate(time), 
			TweenType.Deceleration => m_DecelerationCurve.Evaluate(time), 
			TweenType.Harmonic => m_HarmonicCurve.Evaluate(time), 
			_ => customCurve.Evaluate(time), 
		};
	}

	private bool IsCustomCurveNormalised()
	{
		if (!Mathf.Approximately(customCurve[0].time, 0f))
		{
			return false;
		}
		if (!Mathf.Approximately(customCurve[0].value, 0f))
		{
			return false;
		}
		if (!Mathf.Approximately(customCurve[customCurve.length - 1].time, 1f))
		{
			return false;
		}
		return Mathf.Approximately(customCurve[customCurve.length - 1].value, 1f);
	}
}
