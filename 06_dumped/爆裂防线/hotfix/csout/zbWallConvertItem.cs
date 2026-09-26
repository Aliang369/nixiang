using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

internal class zbWallConvertItem : zbViewBaseItem
{
	private zbItem m_item0;

	private zbItem m_item1;

	private zbWallCsvRecord m_record;

	private WallDetail m_detail;

	public zbWallConvertItem(VisualTreeAsset tAssets, zbWallConvertWnd wndParent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAssets.Instantiate());
		m_item0 = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item0", (string)null);
		m_item0.OnEnable(wndParent);
		m_item1 = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item1", (string)null);
		m_item1.OnEnable(wndParent);
	}

	public void Init(WallDetail _detail)
	{
		m_detail = _detail;
		m_record = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(m_detail.WallId);
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, m_record.propId);
		m_item0.Refresh(new zbItemData(EResourceType.Prop, m_record.propId, resourceCount));
		m_item1.Refresh(new zbItemData(EResourceType.Prop, m_record.convertPropId, resourceCount));
	}
}
