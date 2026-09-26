using Proto.Enums;
using UnityEngine.UIElements;

public class zbTreasureUpgradeItem : zbViewBaseItem
{
	private zbItem m_item;

	private Label m_lbName;

	private Label m_lbLastLevel;

	private Label m_lbLevel;

	public zbTreasureUpgradeItem(VisualTreeAsset tAsset, zbTreasureUpgradeWnd wndParent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(wndParent);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbLastLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_lastLevel", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(TreasureLevelChange data)
	{
		zbTreasureCsvRecord recordByKey = zbDataConfig.Instance.treasureCsvTable.GetRecordByKey(data.Id);
		if (recordByKey != null)
		{
			m_item.Refresh(new zbItemData(EResourceType.Prop, recordByKey.propId, 0L), isNameVis: false, isCountVis: false);
			((TextElement)m_lbName).text = recordByKey.name;
			((TextElement)m_lbLastLevel).text = $"Lv.{data.OldLevel}";
			((TextElement)m_lbLevel).text = $"Lv.{data.NewLevel}";
		}
	}
}
