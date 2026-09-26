using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMineWarRecordItem : zbViewBaseItem
{
	private VisualElement m_bg;

	private VisualElement m_playerInfo;

	private Label m_lbName;

	private Label m_lbPower;

	private Label m_lbResult;

	private VisualElement m_spriteFightType;

	private Label m_lbTime;

	private Button m_btnHead;

	private Button m_btnRecord;

	private zbItem m_item;

	private MineWarRecordDetail m_detail;

	public zbMineWarRecordItem(VisualTreeAsset tAsset, zbMineWarRecordWnd wndParent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_playerInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "playerInfo", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbPower = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_power", (string)null);
		m_lbResult = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_result", (string)null);
		m_spriteFightType = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_fightType", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_time", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_head", (string)null);
		m_btnRecord = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_record", (string)null);
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)m_btnRecord, "item", (string)null);
		m_item.OnEnable(wndParent);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnHead, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRecord, (Action)OnRecord, "", (TrickleDown)0);
	}

	public void Init(MineWarRecordDetail detail, CrossMineRecordDetail detailCross)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		if (detail != null)
		{
			m_detail = detail;
			if (m_detail.SettleAward != null && m_detail.SettleAward.Count > 0)
			{
				((VisualElement)m_btnRecord).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_item.Refresh(new zbItemData(m_detail.SettleAward.Type, m_detail.SettleAward.Tid, m_detail.SettleAward.Count), isNameVis: false, isCountVis: true, isTouchVis: false);
			}
			else
			{
				((VisualElement)m_btnRecord).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
		else
		{
			((VisualElement)m_btnRecord).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_detail.UUID = detailCross.UUID;
			m_detail.FightType = detailCross.FightType;
			m_detail.ResultScore = detailCross.ResultScore;
			m_detail.RecordTm = detailCross.RecordTm;
			m_detail.RoleBaseInfo = detailCross.RoleBaseInfo;
			m_detail.PointId = detailCross.PointId;
		}
		InitPlayerInfo();
		if (m_detail.FightType == EFightType.Fight)
		{
			m_spriteFightType.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/arena/arena_record_fight.png"));
		}
		else
		{
			m_spriteFightType.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/arena/arena_record_befight.png"));
		}
		if (m_detail.ResultScore == EBattleResultScore.Fail)
		{
			m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/arena/arena_record_cell0.png"));
			((VisualElement)m_lbResult).style.color = StyleColor.op_Implicit(new Color(0.8156863f, 74f / 85f, 1f));
			((TextElement)m_lbResult).text = "LOSS";
		}
		else
		{
			m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/arena/arena_record_cell1.png"));
			((VisualElement)m_lbResult).style.color = StyleColor.op_Implicit(new Color(1f, 49f / 51f, 0.61960787f));
			((TextElement)m_lbResult).text = "WIN";
		}
		((TextElement)m_lbPower).text = zbUtil.ConvertToTBMK(m_detail.RoleBaseInfo.TeamAttack);
		((TextElement)m_lbTime).text = zbUtil.FormatUnixTimeToUtcString(m_detail.RecordTm);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	private void InitPlayerInfo()
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		if (m_detail.RoleBaseInfo.ServerId != zbSelf.Instance.GetServerId())
		{
			((TextElement)m_lbName).text = zbUtil.SetServerString(m_detail.RoleBaseInfo.ServerId) + "-" + m_detail.RoleBaseInfo.RoleName;
		}
		else
		{
			((TextElement)m_lbName).text = m_detail.RoleBaseInfo.RoleName;
		}
		string icon = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(m_detail.RoleBaseInfo.HeadId).icon;
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(icon);
		UQueryExtensions.Q<VisualElement>(m_playerInfo, "header", (string)null).style.backgroundImage = new StyleBackground(val);
		string icon2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(m_detail.RoleBaseInfo.HeadFrameId).icon;
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(icon2);
		UQueryExtensions.Q<VisualElement>(m_playerInfo, "frame", (string)null).style.backgroundImage = new StyleBackground(val2);
		((TextElement)UQueryExtensions.Q<Label>(m_playerInfo, "lb_level", (string)null)).text = m_detail.RoleBaseInfo.Level.ToString();
	}

	private void OnHead()
	{
		if (m_detail.RoleBaseInfo.RoleId < 100000)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10882"));
		}
		else
		{
			zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_detail.RoleBaseInfo.RoleId);
		}
	}

	private void OnRecord()
	{
		zbWsMsgHandler.Instance.MineWar_RecordSettle_Req();
	}
}
