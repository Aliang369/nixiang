using UnityEngine.UIElements;

public class zbJourneyOneBarRateItem : zbViewBaseItem
{
	private zbJourneyOneBarRateWnd m_wndParent;

	private zbItem m_item = new zbItem();

	private Label m_lbRate;

	public zbJourneyOneBarRateItem(zbJourneyOneBarRateWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/journey/journeyOne/tb_journeyOneBarRate.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_lbRate = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_rate", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbRateData data)
	{
		m_item.Refresh(data.itemData);
		((TextElement)m_lbRate).text = (data.rate * 100f).ToString("F2") + "%";
	}
}
