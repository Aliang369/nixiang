using UnityEngine.UIElements;

public class zbSeasonGuardInfoItem : zbViewBaseItem
{
	private zbSeasonGuardInfoWnd m_wndParent;

	private VisualElement m_iconFlag;

	private VisualElement m_head;

	private VisualElement m_frame;

	private Label m_lbLevel;

	private Label m_lbName;

	private Label m_lbPower;

	private Label m_lbCount;

	public zbSeasonGuardInfoItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/season/tb_seasonGuardInfo.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbSeasonGuardInfoWnd wndParent)
	{
		m_wndParent = wndParent;
		m_iconFlag = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_flag", (string)null);
		m_head = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "head", (string)null);
		m_frame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frame", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbPower = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_power", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init()
	{
	}
}
