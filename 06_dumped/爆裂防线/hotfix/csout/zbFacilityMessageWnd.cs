using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbFacilityMessageWnd : zbBaseWnd
{
	private Label m_lbTitle;

	private Label m_lbDesc;

	private VisualElement m_spriteCost;

	private Label m_lbCost;

	private VisualElement m_spriteAward;

	private Label m_lbAward;

	private zbItem m_item;

	private Button m_btnConfirm;

	private Button m_btnCancel;

	private bool m_needTili;

	private Action<bool> m_callback;

	private long m_explorerExpire;

	private AwardDetail m_award;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_spriteCost = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_cost", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		m_spriteAward = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_award", (string)null);
		m_lbAward = UQueryExtensions.Q<Label>(m_RootElement, "lb_award", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnConfirm, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(string title, string message, bool needTili, Action<bool> callback, long explorerExpire, AwardDetail award)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbTitle).text = title;
		((TextElement)m_lbDesc).text = message;
		m_needTili = needTili;
		m_callback = callback;
		m_explorerExpire = explorerExpire;
		m_award = award;
		m_spriteCost.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!m_needTili));
		m_spriteAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_award == null));
		if (m_award != null)
		{
			((VisualElement)m_lbDesc).style.top = StyleLength.op_Implicit(80f);
			m_item.Refresh(new zbItemData(award.Type, award.Tid, award.Count));
		}
		else
		{
			((VisualElement)m_lbDesc).style.top = StyleLength.op_Implicit(135f);
		}
		((TextElement)m_lbCost).text = zbSelf.Instance.facilityHandler.tiliCost.ToString();
		RefreshLeftTime();
	}

	private void RefreshLeftTime()
	{
		if (m_explorerExpire > 0)
		{
			long num = m_explorerExpire - zbSelf.Instance.timeHandler.GetServerTime();
			if (num > 0)
			{
				((TextElement)m_lbDesc).text = string.Format(zbUtil.Language("lang.11278"), "<color=red>" + zbUtil.FormatTimeMMSS(num) + "</color>");
			}
			else
			{
				OnConfirm(null);
			}
		}
	}

	private void OnConfirm(ClickEvent evt)
	{
		if (m_needTili && !zbSelf.Instance.moneyHandler.HasEnoughMoney(4, zbSelf.Instance.facilityHandler.tiliCost))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 4L, zbSelf.Instance.facilityHandler.tiliCost);
			return;
		}
		Hide();
		m_callback?.Invoke(obj: true);
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshLeftTime();
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
		m_callback?.Invoke(obj: false);
	}
}
