using UnityEngine;
using UnityEngine.Playables;

public class TransformTweenMixerBehaviour : PlayableBehaviour
{
	private bool m_FirstFrameHappened;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		Transform transform = playerData as Transform;
		if (transform == null)
		{
			return;
		}
		Vector3 position = transform.position;
		Quaternion rotation = transform.rotation;
		int inputCount = playable.GetInputCount();
		float num = 0f;
		float num2 = 0f;
		Vector3 zero = Vector3.zero;
		Quaternion quaternion = new Quaternion(0f, 0f, 0f, 0f);
		for (int i = 0; i < inputCount; i++)
		{
			ScriptPlayable<TransformTweenBehaviour> playable2 = (ScriptPlayable<TransformTweenBehaviour>)playable.GetInput(i);
			TransformTweenBehaviour behaviour = playable2.GetBehaviour();
			if (behaviour.endLocation == null)
			{
				continue;
			}
			float inputWeight = playable.GetInputWeight(i);
			if (!m_FirstFrameHappened && !behaviour.startLocation)
			{
				behaviour.startingPosition = position;
				behaviour.startingRotation = rotation;
			}
			float time = (float)(playable2.GetTime() / playable2.GetDuration());
			float t = behaviour.EvaluateCurrentCurve(time);
			if (behaviour.tweenPosition)
			{
				num += inputWeight;
				zero += Vector3.Lerp(behaviour.startingPosition, behaviour.endLocation.position, t) * inputWeight;
			}
			if (behaviour.tweenRotation)
			{
				num2 += inputWeight;
				Quaternion rotation2 = Quaternion.Lerp(behaviour.startingRotation, behaviour.endLocation.rotation, t);
				rotation2 = NormalizeQuaternion(rotation2);
				if (Quaternion.Dot(quaternion, rotation2) < 0f)
				{
					rotation2 = ScaleQuaternion(rotation2, -1f);
				}
				rotation2 = ScaleQuaternion(rotation2, inputWeight);
				quaternion = AddQuaternions(quaternion, rotation2);
			}
		}
		zero += position * (1f - num);
		Quaternion second = ScaleQuaternion(rotation, 1f - num2);
		quaternion = AddQuaternions(quaternion, second);
		transform.position = zero;
		transform.rotation = quaternion;
		m_FirstFrameHappened = true;
	}

	public override void OnPlayableDestroy(Playable playable)
	{
		m_FirstFrameHappened = false;
	}

	private static Quaternion AddQuaternions(Quaternion first, Quaternion second)
	{
		first.w += second.w;
		first.x += second.x;
		first.y += second.y;
		first.z += second.z;
		return first;
	}

	private static Quaternion ScaleQuaternion(Quaternion rotation, float multiplier)
	{
		rotation.w *= multiplier;
		rotation.x *= multiplier;
		rotation.y *= multiplier;
		rotation.z *= multiplier;
		return rotation;
	}

	private static float QuaternionMagnitude(Quaternion rotation)
	{
		return Mathf.Sqrt(Quaternion.Dot(rotation, rotation));
	}

	private static Quaternion NormalizeQuaternion(Quaternion rotation)
	{
		float num = QuaternionMagnitude(rotation);
		if (num > 0f)
		{
			return ScaleQuaternion(rotation, 1f / num);
		}
		Debug.LogWarning("Cannot normalize a quaternion with zero magnitude.");
		return Quaternion.identity;
	}
}
