using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBarRateSpecialItemTitle : zbViewBaseItem
{
	private Label m_lbDesc;

	private Label m_lbSubDesc;

	private Label m_lbRate;

	public zbBarRateSpecialItemTitle(zbBarProbabilityCsvRecord record)
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/vip/tb_barRateSpecialTitle.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		m_lbSubDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_subDesc", (string)null);
		m_lbRate = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_rate", (string)null);
		Refresh(record);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	private void Refresh(zbBarProbabilityCsvRecord record)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbDesc).text = record.desc;
		((TextElement)m_lbRate).text = record.probability * 100f + "%";
		Color color = zbResourceHelper.GetColor((EQuality)record.quality);
		((VisualElement)m_lbDesc).style.color = StyleColor.op_Implicit(color);
		((VisualElement)m_lbSubDesc).style.color = StyleColor.op_Implicit(color);
		((VisualElement)m_lbRate).style.color = StyleColor.op_Implicit(color);
	}
}
