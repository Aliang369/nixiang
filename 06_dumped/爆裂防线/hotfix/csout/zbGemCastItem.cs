using UnityEngine.UIElements;

public class zbGemCastItem : zbViewBaseItem
{
	private Label m_lbMod;

	public zbGemCastItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/leader/tb_gemCast.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_lbMod = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_mod", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(int modId)
	{
		zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(modId);
		if (recordByKey != null)
		{
			((TextElement)m_lbMod).text = recordByKey.desc;
		}
	}
}
