using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

internal class zbFootholdConvertItem : zbViewBaseItem
{
	private zbItem m_item0;

	private zbItem m_item1;

	private FootholdDetail m_footholdDetail;

	private zbFootholdCsvRecord m_record;

	public zbFootholdConvertItem(zbFootholdConvertWnd wndParent, VisualTreeAsset tAssets)
	{
		((VisualElement)this).Add((VisualElement)(object)tAssets.Instantiate());
		m_item0 = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item0", (string)null);
		m_item0.OnEnable(wndParent);
		m_item1 = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item1", (string)null);
		m_item1.OnEnable(wndParent);
	}

	public void Init(FootholdDetail footholdDetail)
	{
		m_footholdDetail = footholdDetail;
		m_record = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(m_footholdDetail.FootholdId);
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, m_record.propId);
		zbItemData data = new zbItemData(EResourceType.Prop, m_record.propId, (int)resourceCount);
		m_item0.Refresh(data);
		zbItemData data2 = new zbItemData(EResourceType.Prop, m_record.convertPropId, resourceCount);
		m_item1.Refresh(data2);
	}
}
