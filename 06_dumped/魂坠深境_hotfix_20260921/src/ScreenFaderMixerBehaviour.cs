using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ScreenFaderMixerBehaviour : PlayableBehaviour
{
	private Color m_DefaultColor;

	private Image m_TrackBinding;

	private bool m_FirstFrameHappened;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		m_TrackBinding = playerData as Image;
		if (m_TrackBinding == null)
		{
			return;
		}
		if (!m_FirstFrameHappened)
		{
			m_DefaultColor = m_TrackBinding.color;
			m_FirstFrameHappened = true;
		}
		int inputCount = playable.GetInputCount();
		Color clear = Color.clear;
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		for (int i = 0; i < inputCount; i++)
		{
			float inputWeight = playable.GetInputWeight(i);
			ScreenFaderBehaviour behaviour = ((ScriptPlayable<ScreenFaderBehaviour>)playable.GetInput(i)).GetBehaviour();
			clear += behaviour.color * inputWeight;
			num += inputWeight;
			if (inputWeight > num2)
			{
				num2 = inputWeight;
			}
			if (!Mathf.Approximately(inputWeight, 0f))
			{
				num3++;
			}
		}
		m_TrackBinding.color = clear + m_DefaultColor * (1f - num);
	}

	public override void OnPlayableDestroy(Playable playable)
	{
		m_FirstFrameHappened = false;
		if (!(m_TrackBinding == null))
		{
			m_TrackBinding.color = m_DefaultColor;
		}
	}
}
