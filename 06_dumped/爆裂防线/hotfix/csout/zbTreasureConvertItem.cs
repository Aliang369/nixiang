using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

internal class zbTreasureConvertItem : zbViewBaseItem
{
	private zbItem m_item0;

	private zbItem m_item1;

	private zbTreasureCsvRecord m_record;

	private TreasureDetail m_detail;

	public zbTreasureConvertItem(VisualTreeAsset tAssets, zbTreasureConvertWnd wndParent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAssets.Instantiate());
		m_item0 = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item0", (string)null);
		m_item0.OnEnable(wndParent);
		m_item1 = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item1", (string)null);
		m_item1.OnEnable(wndParent);
	}

	public void Init(TreasureDetail _detail)
	{
		m_detail = _detail;
		m_record = zbDataConfig.Instance.treasureCsvTable.GetRecordByKey(m_detail.Id);
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, m_record.propId);
		m_item0.Refresh(new zbItemData(EResourceType.Prop, m_record.propId, resourceCount));
		zbTreasureQualityCsvRecord zbTreasureQualityCsvRecord2 = zbDataConfig.Instance.treasureQualityCsvTable.GetRecords().records.FirstOrDefault((zbTreasureQualityCsvRecord r) => r.quality == m_record.quality);
		if (zbTreasureQualityCsvRecord2 != null)
		{
			m_item1.Refresh(new zbItemData((EResourceType)zbTreasureQualityCsvRecord2.awardList[0], zbTreasureQualityCsvRecord2.awardList[1], zbTreasureQualityCsvRecord2.awardList[2] * resourceCount));
		}
	}
}
