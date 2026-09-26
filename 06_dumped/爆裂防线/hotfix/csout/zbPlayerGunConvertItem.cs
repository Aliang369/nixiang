using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

internal class zbPlayerGunConvertItem : zbViewBaseItem
{
	private zbPlayerGunConvertWnd m_wndRoot;

	private zbPlayerGunCsvRecord m_record;

	private zbItem m_item0;

	private zbItem m_item1;

	private PlayerGunDetail m_detail;

	public zbPlayerGunConvertItem(VisualTreeAsset tAssets, zbPlayerGunConvertWnd wndRoot)
	{
		((VisualElement)this).Add((VisualElement)(object)tAssets.Instantiate());
		m_wndRoot = wndRoot;
		m_item0 = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item0", (string)null);
		m_item0.OnEnable(wndRoot);
		m_item1 = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item1", (string)null);
		m_item1.OnEnable(wndRoot);
	}

	public void Init(PlayerGunDetail _detail)
	{
		m_detail = _detail;
		m_record = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(m_detail.GunId);
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, m_record.propId);
		zbItemData data = new zbItemData(EResourceType.Prop, m_record.propId, (int)resourceCount);
		m_item0.Refresh(data);
		zbItemData data2 = new zbItemData(EResourceType.Prop, m_record.convertPropId, resourceCount);
		m_item1.Refresh(data2);
	}
}
