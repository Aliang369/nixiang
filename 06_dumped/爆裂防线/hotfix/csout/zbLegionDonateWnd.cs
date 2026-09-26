using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionDonateWnd : zbBaseWnd
{
	private Button m_btnBack;

	private Button m_btnGold;

	private Button m_btnDiamond;

	private Label m_lbTitle;

	private Label m_lbGoldTitle;

	private Label m_lbDiamondTitle;

	private Label m_targetReward;

	private Label m_lbStageBar;

	private Label m_iconFlag;

	private Label m_lbName;

	private Label m_lbId;

	private Label m_lbIdVal;

	private Label m_lbLv;

	private Label m_lbLvVal;

	private Label m_lbPro;

	private Label m_lbProVal;

	private Label m_lbProText;

	private Label m_lbMember;

	private Label m_lbMemberCur;

	private Label m_lbMemberNext;

	private Label m_lbDonate;

	private Label m_lbDonateCur;

	private Label m_lbDonateNext;

	private Label m_lbEnergy;

	private Label m_lbEnergyCur;

	private Label m_lbEnergyNext;

	private Label m_lbCount;

	private VisualElement m_notifyGold;

	private VisualElement m_notifyDiamond;

	private Label[] m_lbDummys = (Label[])(object)new Label[4];

	private Label[] m_lbTargets = (Label[])(object)new Label[5];

	private zbItem[] m_targetAwards = new zbItem[5];

	private JObject m_legionBase;

	protected override void OnEnable()
	{
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnGold = UQueryExtensions.Q<Button>(m_RootElement, "btn_gold", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "pnl_btndiamond", (string)null);
		m_btnDiamond = UQueryExtensions.Q<Button>(val, "btn_diamond", (string)null);
		m_lbGoldTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_goldTitle", (string)null);
		m_lbDiamondTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_diamondTitle", (string)null);
		m_lbStageBar = UQueryExtensions.Q<Label>(m_RootElement, "lb_stageBar", (string)null);
		m_iconFlag = UQueryExtensions.Q<Label>(m_RootElement, "icon_flag", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbId = UQueryExtensions.Q<Label>(m_RootElement, "lb_id", (string)null);
		m_lbIdVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_idVal", (string)null);
		m_lbLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_lv", (string)null);
		m_lbLvVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_lvVal", (string)null);
		m_lbPro = UQueryExtensions.Q<Label>(m_RootElement, "lb_pro", (string)null);
		m_lbProVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_proVal", (string)null);
		m_lbProText = UQueryExtensions.Q<Label>(m_RootElement, "lb_proText", (string)null);
		m_lbMember = UQueryExtensions.Q<Label>(m_RootElement, "lb_member", (string)null);
		m_lbMemberCur = UQueryExtensions.Q<Label>(m_RootElement, "lb_memberCur", (string)null);
		m_lbMemberNext = UQueryExtensions.Q<Label>(m_RootElement, "lb_memberNext", (string)null);
		m_lbDonate = UQueryExtensions.Q<Label>(m_RootElement, "lb_donate", (string)null);
		m_lbDonateCur = UQueryExtensions.Q<Label>(m_RootElement, "lb_donateCur", (string)null);
		m_lbDonateNext = UQueryExtensions.Q<Label>(m_RootElement, "lb_donateNext", (string)null);
		m_lbEnergy = UQueryExtensions.Q<Label>(m_RootElement, "lb_energy", (string)null);
		m_lbEnergyCur = UQueryExtensions.Q<Label>(m_RootElement, "lb_energyCur", (string)null);
		m_lbEnergyNext = UQueryExtensions.Q<Label>(m_RootElement, "lb_energyNext", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "title", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_dailyCount", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_lbDummys[i] = UQueryExtensions.Q<Label>(m_RootElement, "dummy" + i, (string)null);
		}
		for (int j = 0; j < 4; j++)
		{
			m_lbTargets[j] = UQueryExtensions.Q<Label>(m_RootElement, "lb_target" + j, (string)null);
			m_targetAwards[j] = UQueryExtensions.Q<zbItem>(m_RootElement, "targetAward" + j, (string)null);
			m_targetAwards[j].OnEnable(this);
		}
		UQueryExtensions.Q<zbItem>((VisualElement)(object)m_lbDummys[1], "zbItem", (string)null).OnEnable(this);
		UQueryExtensions.Q<zbItem>((VisualElement)(object)m_lbDummys[3], "zbItem", (string)null).OnEnable(this);
		m_legionBase = zbDataConfig.Instance.legionBaseJsonTable.GetRecord();
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGold, (Action<ClickEvent>)OnClickGold, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDiamond, (Action<ClickEvent>)OnClickDiamond, "", (TrickleDown)0);
		m_notifyGold = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGold, (Vector2?)new Vector2(3f, 3f), 1f, true);
		m_notifyDiamond = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnDiamond, (Vector2?)new Vector2(3f, 3f), 1f, true);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Legion_Donate, OnUpdateEventLegionDonate);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
	}

	protected override void UpdateAtShow()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateAtShow();
		m_notifyGold.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.legionHandler.HasDonateBtnRedPoint()));
		m_notifyDiamond.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.legionHandler.HasDonateBtnRedPoint()));
	}

	public override void Refresh()
	{
		Init();
	}

	public bool CheckCanDonate()
	{
		bool num = zbServerData.sGameData.LegionSelfInfo.Donate.DailyCount < Extensions.Value<int>((IEnumerable<JToken>)m_legionBase["donate"][(object)"dailyCount"]);
		if (!num)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10205"));
		}
		return num;
	}

	public void OnClickGold(ClickEvent evt)
	{
		if (CheckCanDonate())
		{
			zbWsMsgHandler.Instance.LegionAux_Donate_Req(EMoneyType.Gold);
		}
	}

	public void OnClickDiamond(ClickEvent evt)
	{
		if (CheckCanDonate())
		{
			zbWsMsgHandler.Instance.LegionAux_Donate_Req(EMoneyType.Diamond);
		}
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void Init()
	{
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		LegionInfo legionInfo = zbSelf.Instance.legionHandler.legionInfo;
		_ = legionInfo.LegionSetInfo;
		((TextElement)m_lbName).text = legionInfo.LegionBaseInfo.LegionName;
		((TextElement)m_lbIdVal).text = legionInfo.LegionBaseInfo.LegionId.ToString();
		int level = legionInfo.LegionBaseInfo.Level;
		((TextElement)m_lbLvVal).text = $"Lv.{level}";
		zbLegionLevelCsvRecord recordByKey = zbDataConfig.Instance.legionLevelCsvTable.GetRecordByKey(level);
		zbLegionLevelCsvRecord recordByKey2 = zbDataConfig.Instance.legionLevelCsvTable.GetRecordByKey(level + 1);
		bool flag = recordByKey2 != null;
		int legionExp = legionInfo.LegionSetInfo.LegionExp;
		if (flag)
		{
			int exp = recordByKey2.exp;
			((TextElement)m_lbProText).text = $"{legionExp} / {exp}";
			((VisualElement)m_lbProVal).style.width = new StyleLength(new Length((float)legionExp / (float)exp * 100f, (LengthUnit)1));
		}
		else
		{
			((TextElement)m_lbProText).text = $"{legionExp} / Max";
			((VisualElement)m_lbProVal).style.width = new StyleLength(new Length(100f, (LengthUnit)1));
		}
		((TextElement)m_lbMemberCur).text = recordByKey.member.ToString();
		((TextElement)m_lbMemberNext).text = (flag ? recordByKey2.member.ToString() : "Max");
		((TextElement)m_lbDonateCur).text = recordByKey.donateMax.ToString();
		((TextElement)m_lbDonateNext).text = (flag ? recordByKey2.donateMax.ToString() : "Max");
		((TextElement)m_lbEnergyCur).text = recordByKey.tili.ToString();
		((TextElement)m_lbEnergyNext).text = (flag ? recordByKey2.tili.ToString() : "Max");
		((VisualElement)m_iconFlag).style.backgroundImage = new StyleBackground(zbSelf.Instance.legionHandler.GetFlagIconById(legionInfo.LegionBaseInfo.Icon));
		((TextElement)m_lbCount).text = zbUtil.Language("lang.10048") + ":" + zbSelf.Instance.legionHandler.GetTodayRemainingCount();
		InitByConfig();
	}

	private void OnUpdateEventLegionDonate(object data)
	{
		Init();
	}

	private void OnTargetAward(ClickEvent evt, int target, bool canReceive)
	{
		if (canReceive)
		{
			zbWsMsgHandler.Instance.LegionAux_DonateTarget_Req(target);
		}
	}

	private void InitByConfig()
	{
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
		JToken val = m_legionBase["donate"][(object)"gold"];
		JToken val2 = m_legionBase["donate"][(object)"diamond"];
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnGold, (string)null, (string)null)).text = Extensions.Value<int>((IEnumerable<JToken>)val[(object)"cost"]).ToString();
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnDiamond, (string)null, (string)null)).text = Extensions.Value<int>((IEnumerable<JToken>)val2[(object)"cost"]).ToString();
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_lbDummys[0], "lb_count", (string)null)).text = zbUtil.Language("lang.11362") + string.Format("<color=#2DF421>+{0}</color>", Extensions.Value<int>((IEnumerable<JToken>)val[(object)"exp"]));
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_lbDummys[1], "lb_count", (string)null)).text = zbUtil.Language("lang.money.name.14") + string.Format("<color=#2DF421>+{0}</color>", Extensions.Value<int>((IEnumerable<JToken>)val[(object)"award"][(object)2]));
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_lbDummys[2], "lb_count", (string)null)).text = zbUtil.Language("lang.11362") + string.Format("<color=#2DF421>+{0}</color>", Extensions.Value<int>((IEnumerable<JToken>)val2[(object)"exp"]));
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_lbDummys[3], "lb_count", (string)null)).text = zbUtil.Language("lang.money.name.14") + string.Format("<color=#2DF421>+{0}</color>", Extensions.Value<int>((IEnumerable<JToken>)val2[(object)"award"][(object)2]));
		UQueryExtensions.Q<zbItem>((VisualElement)(object)m_lbDummys[1], "zbItem", (string)null).Refresh(new zbItemData((EResourceType)Extensions.Value<int>((IEnumerable<JToken>)val[(object)"award"][(object)0]), Extensions.Value<int>((IEnumerable<JToken>)val[(object)"award"][(object)1]), Extensions.Value<int>((IEnumerable<JToken>)val[(object)"award"][(object)2])), isNameVis: false, isCountVis: false);
		UQueryExtensions.Q<zbItem>((VisualElement)(object)m_lbDummys[3], "zbItem", (string)null).Refresh(new zbItemData((EResourceType)Extensions.Value<int>((IEnumerable<JToken>)val2[(object)"award"][(object)0]), Extensions.Value<int>((IEnumerable<JToken>)val2[(object)"award"][(object)1]), Extensions.Value<int>((IEnumerable<JToken>)val2[(object)"award"][(object)2])), isNameVis: false, isCountVis: false);
		zbLegionDonateTargetCsvTableData records = zbDataConfig.Instance.legionDonateTargetCsvTable.GetRecords();
		LegionSelfDonate donate = zbServerData.sGameData.LegionSelfInfo.Donate;
		for (int i = 0; i < 4; i++)
		{
			zbLegionDonateTargetCsvRecord zbLegionDonateTargetCsvRecord2 = records.records[i];
			((TextElement)m_lbTargets[i]).text = zbLegionDonateTargetCsvRecord2.target.ToString();
			bool canReceive = donate.WeekDonate >= zbLegionDonateTargetCsvRecord2.target;
			bool flag = donate.WeekTarget.Contains(zbLegionDonateTargetCsvRecord2.target);
			m_targetAwards[i].Refresh(new zbItemData((EResourceType)zbLegionDonateTargetCsvRecord2.awardList[0][0], zbLegionDonateTargetCsvRecord2.awardList[0][1], zbLegionDonateTargetCsvRecord2.awardList[0][2]));
			int num = i;
			if (flag || !canReceive)
			{
				m_targetAwards[i].SetTouchCallback(null);
			}
			else
			{
				int targetVal;
				switch (num)
				{
				case 0:
					targetVal = 250;
					break;
				case 1:
					targetVal = 500;
					break;
				case 2:
					targetVal = 750;
					break;
				case 3:
					targetVal = 1000;
					break;
				default:
					targetVal = 0;
					break;
				}
				m_targetAwards[i].SetTouchCallback(delegate(ClickEvent evt)
				{
					OnTargetAward(evt, targetVal, canReceive);
				});
			}
			if (canReceive && !flag)
			{
				m_targetAwards[i].SetSpineVisible(vis: true);
			}
			m_targetAwards[i].SetGettedVisible(flag);
			m_targetAwards[i].SetSpineVisible(canReceive && !flag);
		}
		float num2 = donate.WeekDonate;
		float num3 = records.records[records.records.Length - 1].target;
		float num4 = num2 / num3 * 100f;
		((VisualElement)m_lbStageBar).style.width = new StyleLength(new Length((num4 > 100f) ? 100f : num4, (LengthUnit)1));
	}
}
