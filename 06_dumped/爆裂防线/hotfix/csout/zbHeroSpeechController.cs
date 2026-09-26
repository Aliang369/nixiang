using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroSpeechController
{
	public float Duration = 5f;

	public float IntervalMin = 10f;

	public float IntervalMax = 20f;

	public bool AutoPlay = true;

	public float HeadOffset;

	public float? TailLeft;

	private readonly VisualElement m_layer;

	private readonly VisualTreeAsset m_template;

	private readonly Func<int, zbHeroCsvRecord> m_getHero;

	private readonly Func<int, Vector2?> m_getHeadPosition;

	private readonly float[] m_nextTimes;

	private bool m_firstSpeechPending;

	private int m_activeIndex = -1;

	private int m_activeHeroTid;

	private float m_remaining;

	private bool m_waitForLayout;

	private VisualElement m_root;

	private Label m_label;

	private VisualElement m_tail;

	private float m_left = float.NaN;

	private float m_top = float.NaN;

	private float m_tailLeft = float.NaN;

	public zbHeroSpeechController(VisualElement layer, VisualTreeAsset template, int count, Func<int, zbHeroCsvRecord> getHero, Func<int, Vector2?> getHeadPosition)
	{
		m_layer = layer ?? throw new ArgumentNullException("layer");
		m_template = template ?? throw new ArgumentNullException("template");
		m_getHero = getHero ?? throw new ArgumentNullException("getHero");
		m_getHeadPosition = getHeadPosition ?? throw new ArgumentNullException("getHeadPosition");
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		m_nextTimes = new float[count];
		Reset();
	}

	public void Reset()
	{
		Clear();
		m_firstSpeechPending = true;
		Array.Clear(m_nextTimes, 0, m_nextTimes.Length);
	}

	public void Clear()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		m_activeIndex = -1;
		if (m_root != null)
		{
			m_root.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_root.style.visibility = StyleEnum<Visibility>.op_Implicit((Visibility)1);
			((TextElement)m_label).text = string.Empty;
		}
	}

	public void Tick(float deltaTime)
	{
		if (m_layer.panel == null || deltaTime < 0f)
		{
			return;
		}
		if (m_activeIndex >= 0)
		{
			zbHeroCsvRecord zbHeroCsvRecord2 = m_getHero(m_activeIndex);
			m_remaining -= deltaTime;
			if (zbHeroCsvRecord2 == null || zbHeroCsvRecord2.id != m_activeHeroTid || m_remaining <= 0f)
			{
				Clear();
			}
			else if (m_waitForLayout)
			{
				m_waitForLayout = false;
			}
			else if (!PositionSpeech())
			{
				Clear();
			}
		}
		if (!AutoPlay || m_nextTimes.Length == 0)
		{
			return;
		}
		int num = (m_firstSpeechPending ? Random.Range(0, m_nextTimes.Length) : 0);
		for (int i = 0; i < m_nextTimes.Length; i++)
		{
			int num2 = (num + i) % m_nextTimes.Length;
			m_nextTimes[num2] -= deltaTime;
			if (num2 == m_activeIndex || (!m_firstSpeechPending && m_nextTimes[num2] > 0f))
			{
				continue;
			}
			m_nextTimes[num2] = Random.Range(IntervalMin, IntervalMax);
			if (TryShow(num2) && m_firstSpeechPending)
			{
				m_firstSpeechPending = false;
				for (int j = 0; j < m_nextTimes.Length; j++)
				{
					m_nextTimes[j] = Random.Range(IntervalMin, IntervalMax);
				}
				break;
			}
		}
	}

	public bool TryShow(int index)
	{
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		if (m_activeIndex >= 0 || m_layer.panel == null || index < 0 || index >= m_nextTimes.Length)
		{
			return false;
		}
		zbHeroCsvRecord zbHeroCsvRecord2 = m_getHero(index);
		if (zbHeroCsvRecord2 == null || zbHeroCsvRecord2.id <= 0 || !m_getHeadPosition(index).HasValue)
		{
			return false;
		}
		string text = null;
		int count = 0;
		ChooseWord(zbHeroCsvRecord2.shortWord1, ref text, ref count);
		ChooseWord(zbHeroCsvRecord2.shortWord2, ref text, ref count);
		ChooseWord(zbHeroCsvRecord2.shortWord3, ref text, ref count);
		ChooseWord(zbHeroCsvRecord2.shortWord4, ref text, ref count);
		ChooseWord(zbHeroCsvRecord2.shortWord5, ref text, ref count);
		if (text == null)
		{
			return false;
		}
		if (m_root == null)
		{
			m_root = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_template.Instantiate(), "speech_bubble", (string)null);
			m_label = UQueryExtensions.Q<Label>(m_root, "lb_speech", (string)null);
			m_tail = UQueryExtensions.Q<VisualElement>(m_root, "speech_tail", (string)null);
			m_root.style.visibility = StyleEnum<Visibility>.op_Implicit((Visibility)1);
			m_layer.Add(m_root);
		}
		m_root.style.visibility = StyleEnum<Visibility>.op_Implicit((Visibility)1);
		m_root.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((TextElement)m_label).text = text;
		zbFontReplacer.Instance.ReplaceFontInUI(m_root);
		m_activeIndex = index;
		m_activeHeroTid = zbHeroCsvRecord2.id;
		m_remaining = Duration;
		m_waitForLayout = true;
		return true;
	}

	private static void ChooseWord(string key, ref string text, ref int count)
	{
		if (!string.IsNullOrWhiteSpace(key))
		{
			string text2 = zbUtil.Language(key);
			if (!string.IsNullOrWhiteSpace(text2) && !(text2 == key) && Random.Range(0, ++count) == 0)
			{
				text = text2;
			}
		}
	}

	private bool PositionSpeech()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		Vector2? val = m_getHeadPosition(m_activeIndex);
		if (!val.HasValue)
		{
			return false;
		}
		Rect layout = m_root.layout;
		float height = ((Rect)(ref layout)).height;
		layout = m_root.layout;
		float width = ((Rect)(ref layout)).width;
		if (float.IsNaN(height) || float.IsNaN(width) || height <= 0f || width <= 0f)
		{
			return true;
		}
		Vector2 val2 = VisualElementExtensions.WorldToLocal(m_layer, val.Value);
		layout = m_layer.layout;
		float width2 = ((Rect)(ref layout)).width;
		if (float.IsNaN(val2.x) || float.IsNaN(val2.y) || float.IsNaN(width2) || width2 < width + 16f)
		{
			return false;
		}
		float num = Mathf.Clamp(val2.x - width / 2f, 8f, width2 - width - 8f);
		float num2 = val2.y - height - HeadOffset;
		if (num2 < 0f)
		{
			return false;
		}
		float num3 = TailLeft ?? Mathf.Clamp(val2.x - num - 6f, 12f, width - 24f);
		if (m_left != num)
		{
			m_root.style.left = StyleLength.op_Implicit(num);
			m_left = num;
		}
		if (m_top != num2)
		{
			m_root.style.top = StyleLength.op_Implicit(num2);
			m_top = num2;
		}
		if (m_tailLeft != num3)
		{
			m_tail.style.left = StyleLength.op_Implicit(num3);
			m_tailLeft = num3;
		}
		if (m_root.style.visibility != StyleEnum<Visibility>.op_Implicit((Visibility)0))
		{
			m_root.style.visibility = StyleEnum<Visibility>.op_Implicit((Visibility)0);
		}
		return true;
	}
}
