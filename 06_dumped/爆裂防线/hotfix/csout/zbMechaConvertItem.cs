using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

internal class zbMechaConvertItem : zbViewBaseItem
{
	private zbItem m_item0;

	private zbItem m_item1;

	private zbMechaCsvRecord m_record;

	private MechaDetail m_detail;

	public zbMechaConvertItem(VisualTreeAsset tAssets, zbMechaConvertWnd wndParent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAssets.Instantiate());
		m_item0 = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item0", (string)null);
		m_item0.OnEnable(wndParent);
		m_item1 = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item1", (string)null);
		m_item1.OnEnable(wndParent);
	}

	public void Init(MechaDetail _detail)
	{
		m_detail = _detail;
		m_record = zbSelf.Instance.mechaHandler.GetMechaRecord(m_detail.MechaId);
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, m_record.propId);
		m_item0.Refresh(new zbItemData(EResourceType.Prop, m_record.propId, resourceCount));
		m_item1.Refresh(new zbItemData(EResourceType.Prop, m_record.convertPropId, resourceCount));
	}
}
