using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class TextSwitcherMixerBehaviour : PlayableBehaviour
{
	private Color m_DefaultColor;

	private int m_DefaultFontSize;

	private string m_DefaultText;

	private Text m_TrackBinding;

	private bool m_FirstFrameHappened;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		m_TrackBinding = playerData as Text;
		if (m_TrackBinding == null)
		{
			return;
		}
		if (!m_FirstFrameHappened)
		{
			m_DefaultColor = m_TrackBinding.color;
			m_DefaultFontSize = m_TrackBinding.fontSize;
			m_DefaultText = m_TrackBinding.text;
			m_FirstFrameHappened = true;
		}
		int inputCount = playable.GetInputCount();
		Color clear = Color.clear;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		int num4 = 0;
		for (int i = 0; i < inputCount; i++)
		{
			float inputWeight = playable.GetInputWeight(i);
			TextSwitcherBehaviour behaviour = ((ScriptPlayable<TextSwitcherBehaviour>)playable.GetInput(i)).GetBehaviour();
			clear += behaviour.color * inputWeight;
			num += (float)behaviour.fontSize * inputWeight;
			num2 += inputWeight;
			if (inputWeight > num3)
			{
				m_TrackBinding.text = behaviour.text;
				num3 = inputWeight;
			}
			if (!Mathf.Approximately(inputWeight, 0f))
			{
				num4++;
			}
		}
		m_TrackBinding.color = clear + m_DefaultColor * (1f - num2);
		m_TrackBinding.fontSize = Mathf.RoundToInt(num + (float)m_DefaultFontSize * (1f - num2));
		if (num4 != 1 && 1f - num2 > num3)
		{
			m_TrackBinding.text = m_DefaultText;
		}
	}

	public override void OnPlayableDestroy(Playable playable)
	{
		m_FirstFrameHappened = false;
		if (!(m_TrackBinding == null))
		{
			m_TrackBinding.color = m_DefaultColor;
			m_TrackBinding.fontSize = m_DefaultFontSize;
			m_TrackBinding.text = m_DefaultText;
		}
	}
}
