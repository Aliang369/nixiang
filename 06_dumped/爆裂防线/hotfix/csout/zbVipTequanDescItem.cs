using UnityEngine.UIElements;

public class zbVipTequanDescItem : zbViewBaseItem
{
	private Label m_desc;

	private VisualElement m_new;

	public zbVipTequanDescItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/vip/tb_viptqdesc.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(zbViptdescCsvRecord rec, int idx)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		m_desc = UQueryExtensions.Q<Label>((VisualElement)(object)this, (string)null, (string)null);
		m_new = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "new", (string)null);
		((TextElement)m_desc).text = $"{idx}. {rec.desc}";
		m_new.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(rec.isnew != 1));
	}
}
