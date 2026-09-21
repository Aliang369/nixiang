using UnityEngine;
using UnityEngine.Playables;

public class LightControlMixerBehaviour : PlayableBehaviour
{
	private Color m_DefaultColor;

	private float m_DefaultIntensity;

	private float m_DefaultBounceIntensity;

	private float m_DefaultRange;

	private Light m_TrackBinding;

	private bool m_FirstFrameHappened;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		m_TrackBinding = playerData as Light;
		if (m_TrackBinding == null)
		{
			return;
		}
		if (!m_FirstFrameHappened)
		{
			m_DefaultColor = m_TrackBinding.color;
			m_DefaultIntensity = m_TrackBinding.intensity;
			m_DefaultBounceIntensity = m_TrackBinding.bounceIntensity;
			m_DefaultRange = m_TrackBinding.range;
			m_FirstFrameHappened = true;
		}
		int inputCount = playable.GetInputCount();
		Color clear = Color.clear;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		int num6 = 0;
		for (int i = 0; i < inputCount; i++)
		{
			float inputWeight = playable.GetInputWeight(i);
			LightControlBehaviour behaviour = ((ScriptPlayable<LightControlBehaviour>)playable.GetInput(i)).GetBehaviour();
			clear += behaviour.color * inputWeight;
			num += behaviour.intensity * inputWeight;
			num2 += behaviour.bounceIntensity * inputWeight;
			num3 += behaviour.range * inputWeight;
			num4 += inputWeight;
			if (inputWeight > num5)
			{
				num5 = inputWeight;
			}
			if (!Mathf.Approximately(inputWeight, 0f))
			{
				num6++;
			}
		}
		m_TrackBinding.color = clear + m_DefaultColor * (1f - num4);
		m_TrackBinding.intensity = num + m_DefaultIntensity * (1f - num4);
		m_TrackBinding.bounceIntensity = num2 + m_DefaultBounceIntensity * (1f - num4);
		m_TrackBinding.range = num3 + m_DefaultRange * (1f - num4);
	}

	public override void OnPlayableDestroy(Playable playable)
	{
		m_FirstFrameHappened = false;
		if (!(m_TrackBinding == null))
		{
			m_TrackBinding.color = m_DefaultColor;
			m_TrackBinding.intensity = m_DefaultIntensity;
			m_TrackBinding.bounceIntensity = m_DefaultBounceIntensity;
			m_TrackBinding.range = m_DefaultRange;
		}
	}
}
