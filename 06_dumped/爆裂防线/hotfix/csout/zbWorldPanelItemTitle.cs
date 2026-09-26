using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldPanelItemTitle : zbViewBaseItem
{
	private Label m_lbTitle;

	public zbWorldPanelItemTitle(string iconPath, string title)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/world/tb_worldTitle.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_lbTitle = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_title", (string)null);
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(iconPath);
		((VisualElement)m_lbTitle).style.backgroundImage = new StyleBackground(val2);
		((TextElement)m_lbTitle).text = title;
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}
}
