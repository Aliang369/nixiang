using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTreasurePoolTitleItem : zbViewBaseItem
{
	private Label m_lbTitle;

	public zbTreasurePoolTitleItem(EBarType barType)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/treasure/tb_treasurePoolTitle.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_lbTitle = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_title", (string)null);
		Color color = zbResourceHelper.GetColor(EQuality.Blue);
		string text = zbUtil.Language("lang.10950");
		if (barType == EBarType.TreasurePlus)
		{
			color = zbResourceHelper.GetColor(EQuality.Red);
			text = zbUtil.Language("lang.10951");
		}
		((VisualElement)m_lbTitle).style.color = StyleColor.op_Implicit(color);
		((TextElement)m_lbTitle).text = text;
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}
}
