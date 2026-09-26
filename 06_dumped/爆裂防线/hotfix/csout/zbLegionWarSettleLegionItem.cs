using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarSettleLegionItem : zbViewBaseItem
{
	private zbLegionWarSettleWnd m_wndParent;

	private VisualElement m_iconFlag;

	private Label m_lbLegionName;

	private Label m_lbPoint;

	private Button m_btnHead;

	private LegionWarLegionRankDetail m_detail;

	private VisualElement m_iconMvp;

	private VisualElement m_rankInfo;

	private VisualElement m_bg;

	public zbLegionWarSettleLegionItem(zbLegionWarSettleWnd wndParent)
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/legionWar/tb_legionWarSettleLegion.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_wndParent = wndParent;
		m_iconFlag = (VisualElement)(object)UQueryExtensions.Q<Label>((VisualElement)(object)this, "icon_flag", (string)null);
		m_lbLegionName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_legionName", (string)null);
		m_lbPoint = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_point", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_head", (string)null);
		m_rankInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "rankInfo", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_iconMvp = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_mvp", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnHead, "", (TrickleDown)0);
	}

	public void Refresh(LegionWarLegionRankDetail detail, ELegionWarForce legionWarForce)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		m_detail = detail;
		((TextElement)m_lbLegionName).text = m_detail.LegionFlagInfo.LegionName;
		m_iconFlag.style.backgroundImage = new StyleBackground(zbSelf.Instance.legionHandler.GetFlagIconById(m_detail.LegionFlagInfo.Icon));
		((TextElement)m_lbPoint).text = m_detail.RankData.Value.ToString();
		zbSelf.Instance.legionWarHandler.RefreshLegionRankInfo(m_rankInfo, detail);
		bool flag = m_detail.LegionFlagInfo.LegionId == zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.LegionId;
		m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag ? "ui/image/legionWar/yellow_bg.png" : "ui/image/legionWar/gray_bg.png"));
		LegionWarSettleLegionInfoAck legionWarSettleLegionInfoAck = zbSelf.Instance.legionWarHandler.legionWarSettleLegionInfoAck;
		bool flag2 = legionWarSettleLegionInfoAck.WinForceId == ELegionWarForce.Red && m_wndParent.CheckHasPlayerInfo();
		bool flag3 = legionWarSettleLegionInfoAck.WinForceId == ELegionWarForce.Blue && m_wndParent.CheckHasPlayerInfo();
		bool flag4 = (((legionWarForce == ELegionWarForce.Red) & flag2) && detail.RankData.Rank == 1) || (((legionWarForce == ELegionWarForce.Blue) & flag3) && detail.RankData.Rank == 1);
		m_iconMvp.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag4));
		if (flag4)
		{
			m_iconMvp.style.left = StyleLength.op_Implicit((StyleKeyword)((!flag2) ? 2 : 0));
			m_iconMvp.style.right = (flag2 ? StyleLength.op_Implicit((StyleKeyword)2) : StyleLength.op_Implicit(37f));
		}
	}

	private void OnHead()
	{
		zbWsMsgHandler.Instance.Legion_TipsInfo_Req(m_detail.LegionFlagInfo.LegionId);
	}
}
