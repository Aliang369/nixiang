using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbCrossPeakItem : VisualElement
{
	private VisualElement m_veRank;

	private Label m_lbRank;

	private Label m_lbName;

	private VisualElement m_Diamond1;

	private VisualElement m_Diamond2;

	private Label m_lbDiamond1;

	private Label m_lbDiamond2;

	private VisualElement m_bg;

	private CrossPeakRankDetail m_rankDetail;

	private Label m_lbPower;

	private Label m_lbSelf;

	private Label m_lbFight;

	private Button m_btnHead;

	private Button m_btnFight;

	private Button m_btnSweep;

	private Button m_btnVipFight;

	private zbCrossPeakWnd m_parent;

	private int rank;

	private int rankSelf;

	public VisualElement m_spinePlaceholder { get; private set; }

	public VisualElement m_spinePlaceholderTop { get; private set; }

	public zbCrossPeakItem(VisualTreeAsset tAsset, zbCrossPeakWnd parent, int index)
	{
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)this)._002Ector();
		m_parent = parent;
		rank = index + 1;
		TemplateContainer val = tAsset.Instantiate();
		((VisualElement)this).Add((VisualElement)(object)val);
		m_lbRank = UQueryExtensions.Q<Label>((VisualElement)(object)val, "lb_rank", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)val, "lb_name", (string)null);
		m_veRank = UQueryExtensions.Q<VisualElement>((VisualElement)(object)val, "m_rank", (string)null);
		m_lbPower = UQueryExtensions.Q<Label>((VisualElement)(object)val, "lb_power", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)val, "bg", (string)null);
		m_Diamond1 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)val, "btn_diamond1", (string)null);
		m_Diamond2 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)val, "btn_diamond2", (string)null);
		m_lbDiamond1 = UQueryExtensions.Q<Label>((VisualElement)(object)val, "lb_diamond1", (string)null);
		m_lbDiamond2 = UQueryExtensions.Q<Label>((VisualElement)(object)val, "lb_diamond2", (string)null);
		m_lbSelf = UQueryExtensions.Q<Label>((VisualElement)(object)val, "lb_self", (string)null);
		m_lbFight = UQueryExtensions.Q<Label>((VisualElement)(object)val, "lb_fight", (string)null);
		m_spinePlaceholder = UQueryExtensions.Q<VisualElement>((VisualElement)(object)val, "spinePlaceholder", (string)null);
		m_spinePlaceholderTop = UQueryExtensions.Q<VisualElement>((VisualElement)(object)val, "spinePlaceholderTop", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>((VisualElement)(object)val, "btn_head", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>((VisualElement)(object)val, "btn_confirm", (string)null);
		m_btnSweep = UQueryExtensions.Q<Button>((VisualElement)(object)val, "btn_saodang", (string)null);
		m_btnVipFight = UQueryExtensions.Q<Button>((VisualElement)(object)val, "btn_vipfight", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)val);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnClickHead, "", (TrickleDown)0);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action)OnClickFight, "", (TrickleDown)0);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSweep, (Action)OnClickSweep, "", (TrickleDown)0);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnVipFight, (Action)OnClickFight, "", (TrickleDown)0);
		bool flag = rank == 1;
		m_veRank.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		((VisualElement)this).style.marginTop = StyleLength.op_Implicit((float)(flag ? 280 : 0));
		m_bg.style.alignSelf = StyleEnum<Align>.op_Implicit((Align)(flag ? 2 : ((rank % 2 != 0) ? 1 : 3)));
		if (rank % 2 == 0)
		{
			m_Diamond1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_Diamond2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			m_Diamond1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_Diamond2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
	}

	private void OnClickHead()
	{
		if (!zbUtil.IsNpc(m_rankDetail.RoleBaseInfo.RoleId))
		{
			zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_rankDetail.RoleBaseInfo.RoleId);
		}
	}

	private void OnSwitchConfirm(bool isOk)
	{
		if (isOk)
		{
			zbWsMsgHandler.Instance.CrossPeak_SaoDang_Req((int)m_rankDetail.RoleBaseInfo.RoleId, m_rankDetail.Rank);
		}
	}

	private void OnClickSweep()
	{
		zbUIMgr.Instance.ShowMessageBox("Tip", zbUtil.Language("lang.11085"), OnSwitchConfirm);
	}

	private void OnClickFight()
	{
		zbPeakMatchCsvRecord recordByKey = zbDataConfig.Instance.peakMatchCsvTable.GetRecordByKey(rankSelf);
		if (recordByKey == null)
		{
			return;
		}
		if (rank < recordByKey.fightRange)
		{
			if (rank < recordByKey.vipFightRange)
			{
				return;
			}
			if (!zbSelf.Instance.crossLadderHandler.ValidateHasCrossLadderCard())
			{
				zbUIMgr.Instance.ShowCrossLadderHelpCardWnd();
				return;
			}
		}
		zbUIMgr.Instance.ShowArenaDetail(EBattleFuncType.PeakArena, m_rankDetail.RoleBaseInfo, null, null, m_rankDetail.Rank);
	}

	public void Init(CrossPeakRankDetail rankDetail)
	{
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		rankSelf = zbSelf.Instance.crossPeakHandler.crossPeakRankInfo.Rank;
		int num = zbSelf.Instance.crossPeakHandler.crossPeakRankInfo.BestRank;
		if (num == 0)
		{
			num = 51;
		}
		if (rankSelf == 0)
		{
			rankSelf = 51;
		}
		m_rankDetail = rankDetail;
		int num2 = 0;
		foreach (KeyValuePair<int, zbPeakRankAwardCsvRecord> record in zbDataConfig.Instance.peakRankAwardCsvTable.GetRecords().records)
		{
			if (record.Value.rank >= rank && record.Value.rank < num)
			{
				num2 += record.Value.bestAward[2];
			}
		}
		((TextElement)m_lbDiamond1).text = num2.ToString();
		((TextElement)m_lbDiamond2).text = num2.ToString();
		if (rank >= num)
		{
			m_Diamond1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_Diamond2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		bool visible = rankDetail.RoleBaseInfo != null;
		((VisualElement)m_lbName).visible = visible;
		((TextElement)m_lbName).text = rankDetail.RoleBaseInfo.RoleName;
		((TextElement)m_lbPower).text = zbUtil.ConvertToTBMK(rankDetail.RoleBaseInfo.TeamAttack);
		((TextElement)m_lbRank).text = rank.ToString();
		((VisualElement)m_btnFight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnSweep).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_lbSelf).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnVipFight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (rank == rankSelf)
		{
			((VisualElement)m_lbSelf).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		((VisualElement)m_btnFight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (rank > rankSelf)
		{
			((VisualElement)m_btnSweep).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			if (rank >= rankSelf)
			{
				return;
			}
			zbPeakMatchCsvRecord recordByKey = zbDataConfig.Instance.peakMatchCsvTable.GetRecordByKey(rankSelf);
			if (recordByKey != null)
			{
				if (rank >= recordByKey.fightRange)
				{
					((VisualElement)m_btnFight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				}
				else if (rank >= recordByKey.vipFightRange)
				{
					((VisualElement)m_btnVipFight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				}
			}
		}
	}
}
