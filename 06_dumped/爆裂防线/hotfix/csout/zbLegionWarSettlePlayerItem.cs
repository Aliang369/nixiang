using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarSettlePlayerItem : zbViewBaseItem
{
	private zbLegionWarSettleWnd m_wndParent;

	private VisualElement m_headFrame;

	private VisualElement m_head;

	private Button m_btnHead;

	private LegionWarPlayerRankDetail m_detail;

	private Label m_lbPlayerName;

	private Label m_lbPoint;

	private VisualElement m_rankInfo;

	private VisualElement m_iconMvp;

	private VisualElement m_bg;

	public zbLegionWarSettlePlayerItem(zbLegionWarSettleWnd wndParent)
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/legionWar/tb_legionWarSettlePlayer.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_wndParent = wndParent;
		m_headFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "headFrame", (string)null);
		m_head = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "head", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_head", (string)null);
		m_lbPlayerName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_playerName", (string)null);
		m_lbPoint = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_point", (string)null);
		m_rankInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "rankInfo", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_iconMvp = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_mvp", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnHead, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(LegionWarPlayerRankDetail detail, ELegionWarForce legionWarForce)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		m_detail = detail;
		string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(m_detail.RoleBaseInfo.HeadId);
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(playerHeadTex);
		if ((Object)(object)val != (Object)null)
		{
			m_head.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			m_head.MarkDirtyRepaint();
		}
		string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(m_detail.RoleBaseInfo.HeadFrameId);
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(playerHeadFrameTex);
		if ((Object)(object)val2 != (Object)null)
		{
			m_headFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_headFrame.MarkDirtyRepaint();
		}
		((TextElement)m_lbPlayerName).text = m_detail.RoleBaseInfo.RoleName;
		((TextElement)m_lbPoint).text = m_detail.RankData.Value.ToString();
		bool flag = m_detail.RoleBaseInfo.RoleId == zbSelf.Instance.GetRoleId();
		m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag ? "ui/image/legionWar/yellow_bg.png" : "ui/image/legionWar/gray_bg.png"));
		LegionWarSettlePlayerInfoAck legionWarSettlePlayerInfoAck = zbSelf.Instance.legionWarHandler.legionWarSettlePlayerInfoAck;
		bool flag2 = legionWarSettlePlayerInfoAck.WinForceId == ELegionWarForce.Red && m_wndParent.CheckHasPlayerInfo();
		bool flag3 = legionWarSettlePlayerInfoAck.WinForceId == ELegionWarForce.Blue && m_wndParent.CheckHasPlayerInfo();
		bool flag4 = (((legionWarForce == ELegionWarForce.Red) & flag2) && detail.RankData.Rank == 1) || (((legionWarForce == ELegionWarForce.Blue) & flag3) && detail.RankData.Rank == 1);
		m_iconMvp.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag4));
		if (flag4)
		{
			m_iconMvp.style.left = StyleLength.op_Implicit((StyleKeyword)((!flag2) ? 2 : 0));
			m_iconMvp.style.right = (flag2 ? StyleLength.op_Implicit((StyleKeyword)2) : StyleLength.op_Implicit(37f));
		}
		zbSelf.Instance.legionWarHandler.RefreshPlayerRankInfo(m_rankInfo, detail);
	}

	private void OnHead()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_detail.RoleBaseInfo.RoleId);
	}
}
