using Proto.Enums;
using UnityEngine.UIElements;

public class zbRoomAttrListItem : zbViewBaseItem
{
	private EAttrTeamType m_record;

	private Label m_lbKey;

	private Label m_lbValue;

	public zbRoomAttrListItem(VisualTreeAsset tAsset)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_lbKey = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_key", (string)null);
		m_lbValue = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_value", (string)null);
	}

	public void Init(EAttrTeamType record)
	{
		m_record = record;
		Refresh(record, 0f);
	}

	public void Refresh(EAttrTeamType record, float value)
	{
		zbTeamAttrCsvRecord recordByKey = zbDataConfig.Instance.teamAttrCsvTable.GetRecordByKey((int)record);
		if (recordByKey != null)
		{
			((TextElement)m_lbKey).text = recordByKey.name;
			((TextElement)m_lbValue).text = zbAttrHelper.FormatAttrValue(value, recordByKey.showPercent > 0);
		}
		else
		{
			((TextElement)m_lbKey).text = record.ToString();
			((TextElement)m_lbValue).text = value.ToString("0.00");
		}
	}
}
