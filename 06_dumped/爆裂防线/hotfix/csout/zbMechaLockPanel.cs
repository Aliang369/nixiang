using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaLockPanel
{
	private zbMechaWnd m_wndParent;

	private VisualElement m_RootElement;

	private Label m_lbCost;

	private zbItem m_item = new zbItem();

	private Label m_lbDesc;

	public Button m_btnActive;

	private VisualElement m_notify;

	public void OnEnable(zbMechaWnd wndParent, VisualElement root)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_lbCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnActive = UQueryExtensions.Q<Button>(m_RootElement, "btn_active", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnActive, (Action)OnActive, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnActive, (Vector2?)new Vector2(-3f, -5f), 1f, true);
	}

	public void Refresh()
	{
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		m_item.Refresh(new zbItemData(EResourceType.Prop, m_wndParent.m_record.propId, 1L), isNameVis: false, isCountVis: false);
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, m_wndParent.m_record.propId);
		((TextElement)m_lbCost).text = zbUtil.GetNeedHaveRichString(resourceCount, 1L);
		if (1 > resourceCount)
		{
			m_item.SetTouchCallback(delegate
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_wndParent.m_record.propId, 1L);
			});
		}
		else
		{
			m_item.SetTouchCallback(delegate
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_wndParent.m_record.propId, 0L);
			});
		}
		((TextElement)m_lbDesc).text = m_wndParent.m_record.desc;
		m_wndParent.RefreshMechaSkills();
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.mechaHandler.HasMechaActiveNotify(m_wndParent.m_record.id)));
	}

	public void OnActive()
	{
		if (zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, m_wndParent.m_record.propId) < 1)
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_wndParent.m_record.propId, 1L);
		}
		else
		{
			zbWsMsgHandler.Instance.Mecha_Active_Req(m_wndParent.m_record.id);
		}
	}
}
