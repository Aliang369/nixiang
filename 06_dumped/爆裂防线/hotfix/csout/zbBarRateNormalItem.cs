using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBarRateNormalItem : zbViewBaseItem
{
	private Label m_lbDesc;

	private Label m_lbSubDesc;

	private Label m_lbRate;

	public zbBarRateNormalItem(VisualTreeAsset tAsset)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		Init();
	}

	public void Init()
	{
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		m_lbSubDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_subDesc", (string)null);
		m_lbRate = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_rate", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbBarProbabilityCsvRecord record)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbDesc).text = record.desc;
		((TextElement)m_lbRate).text = $"{record.probability * 100f:0.##}%";
		Color color = zbResourceHelper.GetColor((EQuality)record.quality);
		((VisualElement)m_lbDesc).style.color = StyleColor.op_Implicit(color);
		((VisualElement)m_lbSubDesc).style.color = StyleColor.op_Implicit(color);
		((VisualElement)m_lbRate).style.color = StyleColor.op_Implicit(color);
	}
}
