using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbModSelectWnd : zbBaseWnd
{
	[NonSerialized]
	private VisualElement node_bg;

	[NonSerialized]
	private VisualElement m_lbSkill;

	[NonSerialized]
	private VisualElement num_bg;

	[NonSerialized]
	private Button m_btnRefresh;

	[NonSerialized]
	private Label m_lbRefresh;

	[NonSerialized]
	private VisualElement m_btnEye;

	[NonSerialized]
	private Sprite m_iconVisible;

	[NonSerialized]
	private Sprite m_iconInvisible;

	[NonSerialized]
	private zbModPanel[] m_ModPanels = new zbModPanel[3];

	private Button m_btnModList;

	[NonSerialized]
	public List<int> selectMod = new List<int>();

	private VisualElement m_content;

	private VisualElement m_spFinger;

	private int selectNum;

	private int selectMax;

	private zbCombatTeam m_masterTeam;

	private int curRefreshNum;

	private bool m_canSelect = true;

	[NonSerialized]
	private VisualElement[] m_numBg = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbNum = (Label[])(object)new Label[5];

	private zbPlayerAvatar[] m_spineAvatar = new zbPlayerAvatar[3];

	private SkeletonAnimation[] m_heroAvatar = (SkeletonAnimation[])(object)new SkeletonAnimation[3];

	protected GameObject m_BaseObject;

	private Button m_btnAuto;

	private VisualElement m_spriteOk;

	private int autoLeftTime;

	private VisualElement m_spriteLeft;

	private Label m_lbAutoTime;

	private Label m_lbLeftTime;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_iconVisible = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_visible.png");
		m_iconInvisible = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_Invisible.png");
		m_content = UQueryExtensions.Q<VisualElement>(m_RootElement, "content", (string)null);
		num_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "numBg", (string)null);
		node_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "nodeBg", (string)null);
		m_lbSkill = (VisualElement)(object)UQueryExtensions.Q<Label>(m_RootElement, "lb_skill", (string)null);
		m_btnRefresh = UQueryExtensions.Q<Button>(m_RootElement, "btn_refresh", (string)null);
		m_btnEye = UQueryExtensions.Q<VisualElement>(m_RootElement, "btn_visible", (string)null);
		m_btnModList = UQueryExtensions.Q<Button>(m_RootElement, "btn_modList", (string)null);
		m_lbRefresh = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnRefresh, "lb_refresh", (string)null);
		m_lbLeftTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_leftTime", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRefresh, (Action)OnRefresh, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<PointerDownEvent>(m_btnEye, (Action<PointerDownEvent>)OnEyeDown, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<PointerUpEvent>(m_btnEye, (Action<PointerUpEvent>)OnEyeUp, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnModList, (Action)OnModList, "", (TrickleDown)0);
		m_spriteLeft = UQueryExtensions.Q<VisualElement>(m_RootElement, "lb_left", (string)null);
		m_lbAutoTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_leftNum", (string)null);
		m_btnAuto = UQueryExtensions.Q<Button>(m_RootElement, "btn_auto", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_ok", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAuto, (Action)OnAuto, "", (TrickleDown)0);
		m_BaseObject = ((Component)((Component)this).transform.Find("baseObject")).gameObject;
		for (int i = 0; i < 3; i++)
		{
			zbModPanel zbModPanel2 = new zbModPanel(UQueryExtensions.Q<VisualElement>(m_RootElement, "mod" + i, (string)null));
			m_ModPanels[i] = zbModPanel2;
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_ModPanels[i].btnSelect, (Action<ClickEvent>)delegate
			{
				OnSelect(tempIndex);
			}, "", (TrickleDown)0);
			m_spineAvatar[i] = ((Component)m_BaseObject.transform.Find("GameObject" + i).Find("playerAvatar" + i)).GetComponent<zbPlayerAvatar>();
			m_heroAvatar[i] = ((Component)m_BaseObject.transform.Find("GameObject" + i).Find("heroAvatar")).GetComponent<SkeletonAnimation>();
		}
		for (int num = 0; num < 5; num++)
		{
			m_numBg[num] = UQueryExtensions.Q<VisualElement>(m_RootElement, "num_" + num, (string)null);
			m_lbNum[num] = UQueryExtensions.Q<Label>(m_RootElement, "lb_num" + num, (string)null);
		}
	}

	public void ChangeNum()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		if (selectMax <= 5 || selectNum < 3)
		{
			for (int i = 0; i < 5; i++)
			{
				((TextElement)m_lbNum[i]).text = (i + 1).ToString();
				if (i >= selectMax)
				{
					m_numBg[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				}
				else
				{
					m_numBg[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				}
				Sprite val = ((i != selectNum) ? zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/num_un.png") : zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/num_se.png"));
				m_numBg[i].style.backgroundImage = new StyleBackground(val);
			}
		}
		else if (selectNum > selectMax - 3)
		{
			for (int j = 0; j < 5; j++)
			{
				((TextElement)m_lbNum[j]).text = (selectMax - 5 + j + 1).ToString();
				m_numBg[j].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				Sprite val2 = ((selectMax - 5 + j != selectNum) ? zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/num_un.png") : zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/num_se.png"));
				m_numBg[j].style.backgroundImage = new StyleBackground(val2);
			}
		}
		else
		{
			for (int k = 0; k < 5; k++)
			{
				((TextElement)m_lbNum[k]).text = (selectNum - 2 + k + 1).ToString();
				m_numBg[k].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				Sprite val3 = ((selectNum - 2 + k != selectNum) ? zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/num_un.png") : zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/num_se.png"));
				m_numBg[k].style.backgroundImage = new StyleBackground(val3);
			}
		}
		zbUtil.CenterNodeItems(m_numBg, 44f, 30f);
	}

	private List<int> Pick3IdsWithWeight(List<zbModBattleRandCsvRecord> list)
	{
		List<int> list2 = new List<int>();
		while (list2.Count < 3)
		{
			int? num = WeightedPickExclude(list, list2);
			if (!num.HasValue)
			{
				break;
			}
			list2.Add(num.Value);
		}
		while (list2.Count < 3)
		{
			zbModBattleRandCsvRecord zbModBattleRandCsvRecord2 = WeightedPick(list);
			list2.Add(zbModBattleRandCsvRecord2.modId);
		}
		return list2;
	}

	private zbModBattleRandCsvRecord WeightedPick(List<zbModBattleRandCsvRecord> list)
	{
		int num = 0;
		foreach (zbModBattleRandCsvRecord item in list)
		{
			num += item.weight;
		}
		if (num == 0)
		{
			throw new Exception("权重总和为 0");
		}
		int num2 = Rand.Range(0, num);
		int num3 = 0;
		foreach (zbModBattleRandCsvRecord item2 in list)
		{
			num3 += item2.weight;
			if (num2 < num3)
			{
				return item2;
			}
		}
		return list[0];
	}

	private int? WeightedPickExclude(List<zbModBattleRandCsvRecord> list, List<int> excludeIds)
	{
		int num = 0;
		foreach (zbModBattleRandCsvRecord item in list)
		{
			if (!excludeIds.Contains(item.modId))
			{
				num = ((!selectMod.Contains(item.modId)) ? (num + item.weight) : (num + 1));
			}
		}
		if (num == 0)
		{
			return null;
		}
		int num2 = Rand.Range(0, num);
		int num3 = 0;
		foreach (zbModBattleRandCsvRecord item2 in list)
		{
			if (!excludeIds.Contains(item2.modId))
			{
				num3 = ((!selectMod.Contains(item2.modId)) ? (num3 + item2.weight) : (num3 + 1));
				if (num2 < num3)
				{
					return item2.modId;
				}
			}
		}
		return null;
	}

	public void RefreshDetail()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_lbLeftTime).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbCombatSystem.Instance.GetBattleType() != EBattleFuncType.LegionWar));
		ChangeNum();
		List<zbModBattleRandCsvRecord> list = new List<zbModBattleRandCsvRecord>();
		RandomModId(list);
		selectMod = Pick3IdsWithWeight(list);
		RefreshItems();
		zbCombatSystem.Instance.combatRecorder.NeedModSelectSave(selectNum, selectMax, curRefreshNum, selectMod);
		CaclBtnRefresh();
		RefreshAuto();
	}

	public void CaclBtnRefresh()
	{
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if ((zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.MainQuest || zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.EliteQuest) && zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId <= 1)
		{
			if (curRefreshNum == 0 && zbCombatSystem.Instance.refreshSelectNum > 1)
			{
				((VisualElement)m_btnRefresh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)m_lbRefresh).text = zbUtil.Language("lang.10340") + 1;
			}
			else
			{
				((VisualElement)m_btnRefresh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
		else if (curRefreshNum == 0 && zbCombatSystem.Instance.refreshSelectNum > 0)
		{
			((VisualElement)m_btnRefresh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (zbCombatSystem.Instance.refreshSelectNum > 1)
			{
				((TextElement)m_lbRefresh).text = "";
			}
			else if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.FightModRefresh, zbSelf.Instance.GetVipLevel()) <= 0)
			{
				((TextElement)m_lbRefresh).text = zbUtil.Language("lang.10339");
			}
			else
			{
				((TextElement)m_lbRefresh).text = zbUtil.Language("lang.10340") + zbCombatSystem.Instance.refreshSelectNum;
			}
		}
		else
		{
			((VisualElement)m_btnRefresh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	public void RandomModId(List<zbModBattleRandCsvRecord> randomModIdList)
	{
		zbModBattleRandCsvTableData records = zbDataConfig.Instance.modBattleRandCsvTable.GetRecords();
		zbCombatExp combatExp = zbCombatSystem.Instance.combatData.combatExp;
		zbModBattleRandCsvRecord[] records2 = records.records;
		foreach (zbModBattleRandCsvRecord zbModBattleRandCsvRecord2 in records2)
		{
			if (zbModBattleRandCsvRecord2.randId != zbCombatSystem.Instance.battleInfo.BattleBaseInfo.ModRandId || m_masterTeam.modMgr.IsModIdSelMax(zbModBattleRandCsvRecord2.modId, zbModBattleRandCsvRecord2.maxNum) || (int)combatExp.curLevel + combatExp.initLevel < zbModBattleRandCsvRecord2.battleLv)
			{
				continue;
			}
			bool flag = true;
			int[] requireMod = zbModBattleRandCsvRecord2.requireMod;
			foreach (int modId in requireMod)
			{
				if (!m_masterTeam.modMgr.HasMod(modId))
				{
					flag = false;
					break;
				}
			}
			requireMod = zbModBattleRandCsvRecord2.conflictMod;
			foreach (int modId2 in requireMod)
			{
				if (m_masterTeam.modMgr.HasMod(modId2))
				{
					flag = false;
					break;
				}
			}
			requireMod = zbModBattleRandCsvRecord2.requireSkillType;
			foreach (int skillType in requireMod)
			{
				if (!m_masterTeam.HasSkillType((ESkillType)skillType))
				{
					flag = false;
					break;
				}
			}
			requireMod = zbModBattleRandCsvRecord2.conflictSkillId;
			foreach (int skillId in requireMod)
			{
				if (m_masterTeam.modMgr.HasSkillId(skillId))
				{
					flag = false;
					break;
				}
			}
			zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(zbModBattleRandCsvRecord2.modId);
			if (m_masterTeam.heroList.Count >= 5 && recordByKey.modIndex == EModIndexDefine.Index1000)
			{
				flag = false;
			}
			else if (flag)
			{
				randomModIdList.Add(zbModBattleRandCsvRecord2);
				m_canSelect = true;
			}
		}
	}

	public void RefreshCustom(zbCombatTeam team, int _selectMax)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		selectNum = 0;
		curRefreshNum = 0;
		selectMax = _selectMax;
		m_masterTeam = team;
		num_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(selectMax <= 1));
		zbCombatWnd obj = zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd;
		obj.SetAlertRandgeVisible(null, vis: false);
		obj.SetVisibleUI(flag: false);
		zbCombatSystem.Instance.Pause();
		zbAudioMgr.Instance.PauseAllAudio();
		autoLeftTime = 0;
		RefreshDetail();
		SetElementVisible(visible: true);
		if (zbSelf.Instance.guideHandler.IsFirstLevelMod())
		{
			CalFirstMod23291();
			zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)m_ModPanels[1].btnSelect, OnSelectB);
			zbUIMgr.Instance.ShowGuideWeak(1002, null, 1040f);
		}
		if (zbSelf.Instance.combatHandler.IsZuobi())
		{
			zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10667"), zbUtil.Language("lang.10668") + "_6", OnZuobiConfirm);
			zbCombatSystem.Instance.combatRecorder.ClearRecord();
		}
	}

	private void CalFirstMod23291()
	{
		if (selectMod[0] == 23291 || selectMod[1] == 23291 || selectMod[2] == 23291)
		{
			if (selectMod[0] == 23291)
			{
				selectMod[0] = selectMod[1];
			}
			else if (selectMod[2] == 23291)
			{
				selectMod[2] = selectMod[1];
			}
		}
		selectMod[1] = 23291;
		RefreshItems();
	}

	private void OnZuobiConfirm(bool isOK)
	{
		Hide();
		zbAudioMgr.Instance.StopAllAudio();
		zbUIMgr.Instance.DestroyWnd(EWndId.Combat);
		zbUIMgr.Instance.ShowMainLevel();
		zbCombatSystem.Instance.combatRecorder.ClearRecord();
	}

	public void RefreshRestoreData(zbCombatTeam team, int _selectNum, int _selectMax, int _curRefreshNum, List<int> _selectMod)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		selectNum = _selectNum;
		selectMax = _selectMax;
		m_masterTeam = team;
		curRefreshNum = _curRefreshNum;
		selectMod = new List<int>(_selectMod);
		num_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(selectMax <= 1));
		zbCombatWnd obj = zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd;
		obj.SetAlertRandgeVisible(null, vis: false);
		obj.SetVisibleUI(flag: false);
		zbCombatSystem.Instance.Pause();
		zbAudioMgr.Instance.PauseAllAudio();
		ChangeNum();
		RefreshItems();
		RefreshAuto();
		((VisualElement)m_lbLeftTime).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbCombatSystem.Instance.GetBattleType() != EBattleFuncType.LegionWar));
		zbCombatSystem.Instance.combatRecorder.NeedModSelectSave(selectNum, selectMax, curRefreshNum, selectMod);
		CaclBtnRefresh();
		SetElementVisible(visible: true);
	}

	public void RefreshItems()
	{
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		m_BaseObject.SetActive(true);
		for (int i = 0; i < 3; i++)
		{
			if (selectMod.Count > i)
			{
				int key = selectMod[i];
				zbModCsvRecord modRecord = zbDataConfig.Instance.modCsvTable.GetRecordByKey(key);
				m_ModPanels[i].RefreshDetail(m_masterTeam, modRecord);
				((VisualElement)UQueryExtensions.Q<TemplateContainer>(m_RootElement, "mod" + i, (string)null)).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((Component)m_spineAvatar[i]).gameObject.SetActive(modRecord.skillType[0] == 1);
				((Component)m_heroAvatar[i]).gameObject.SetActive(modRecord.skillType[0] != 1);
				if (modRecord.skillType[0] != 1)
				{
					BattleSkillDetail battleSkillDetail = ((IEnumerable<BattleSkillDetail>)m_masterTeam.battleTeamInfo.SkillList).First((BattleSkillDetail s) => s.SkillType == (ESkillType)modRecord.skillType[0]);
					zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(battleSkillDetail.HeroTid);
					zbHeroAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
					zbUtil.RefreshSpine(m_heroAvatar[i], recordByKey2.frontBone + "_SkeletonData.asset", "idle", loop: true);
				}
				else
				{
					m_spineAvatar[i].RefreshCustom(zbSelf.Instance.GetAvatarId(), zbSelf.Instance.GetGunId());
				}
			}
			else
			{
				((VisualElement)UQueryExtensions.Q<TemplateContainer>(m_RootElement, "mod" + i, (string)null)).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((Component)m_spineAvatar[i]).gameObject.SetActive(false);
				((Component)m_heroAvatar[i]).gameObject.SetActive(false);
			}
		}
	}

	public void OnSelectB()
	{
		OnSelect(1);
	}

	public void OnSelect(int index)
	{
		if (m_canSelect)
		{
			int num = selectMod[index];
			List<int> list = new List<int>();
			list.Add(num);
			List<int> list2 = new List<int>();
			list2.Add(index);
			zbUIMgr.Instance.ShowModSelectAction(list, list2);
			m_masterTeam.UseSelectMode(num);
			zbCombatWnd zbCombatWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd;
			if ((Object)(object)zbCombatWnd2 != (Object)null)
			{
				zbCombatWnd2.refreshSkill();
			}
			selectMod.Clear();
			selectNum++;
			m_canSelect = false;
			curRefreshNum = 0;
			if (selectNum < selectMax)
			{
				RefreshDetail();
				return;
			}
			zbCombatSystem.Instance.combatRecorder.Save();
			OnHideSelf();
		}
	}

	private void OnHideSelf()
	{
		if (zbCombatSystem.Instance.combatData.helperTeam != null)
		{
			foreach (BattleResultModDetail select in zbCombatSystem.Instance.combatData.helperTeam.battleTeamInfo.SelectList)
			{
				if (select.Level == (int)zbCombatSystem.Instance.combatData.combatExp.curLevel)
				{
					zbCombatSystem.Instance.combatData.helperTeam.UseSelectMode(select.ModId);
				}
			}
		}
		Hide();
		(zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd).SetVisibleUI(flag: true);
		if (!zbUIMgr.Instance.IsShow(EWndId.ModWheel))
		{
			zbCombatSystem.Instance.Resume();
			zbAudioMgr.Instance.ResumeAllAudio();
		}
	}

	private void OnModList()
	{
		if (IsAutoing())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11222"));
			return;
		}
		SkillDetail[] heroList = m_masterTeam.heroList.Select((zbHero s) => new SkillDetail
		{
			HeroTid = s.skillDetail.HeroTid,
			SkillType = s.skillDetail.SkillType,
			SkillId = s.skillDetail.SkillId
		}).ToArray();
		List<int> modList = m_masterTeam.modMgr.modList.Select((zbModBase r) => r.modRecord.id).ToList();
		zbUIMgr.Instance.ShowSkillFightModWnd(0, heroList, modList, isResumeFight: false);
	}

	public void OnRefresh()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.MainQuest && zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId <= 1)
		{
			if (curRefreshNum != 0 || zbCombatSystem.Instance.refreshSelectNum <= 1)
			{
				return;
			}
		}
		else
		{
			if (curRefreshNum != 0 || zbCombatSystem.Instance.refreshSelectNum <= 0)
			{
				return;
			}
			((VisualElement)m_btnRefresh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (zbCombatSystem.Instance.refreshSelectNum <= 1)
			{
				if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.FightModRefresh, zbSelf.Instance.GetVipLevel()) <= 0)
				{
					if (!IsAutoing())
					{
						zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10339"));
					}
					return;
				}
				if (zbCombatSystem.Instance.refreshSelectNum < 1)
				{
					return;
				}
			}
		}
		curRefreshNum++;
		zbCombatSystem.Instance.refreshSelectNum--;
		RefreshDetail();
	}

	private void OnEyeDown(PointerDownEvent evt)
	{
		if (IsAutoing())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11222"));
			return;
		}
		PointerCaptureHelper.CapturePointer((IEventHandler)(object)m_btnEye, ((PointerEventBase<PointerDownEvent>)(object)evt).pointerId);
		SetElementVisible(visible: false);
	}

	private void OnEyeUp(PointerUpEvent evt)
	{
		if (IsAutoing())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11222"));
			return;
		}
		PointerCaptureHelper.ReleasePointer((IEventHandler)(object)m_btnEye, ((PointerEventBase<PointerUpEvent>)(object)evt).pointerId);
		SetElementVisible(visible: true);
	}

	private void SetElementVisible(bool visible)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		num_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!visible || selectMax <= 1) ? 1 : 0));
		m_content.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!visible));
		m_btnEye.style.backgroundImage = new StyleBackground(visible ? m_iconVisible : m_iconInvisible);
		zbCombatWnd obj = zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd;
		m_BaseObject.SetActive(visible);
		obj.SetVisibleUI(!visible);
	}

	protected override void OnHide()
	{
		base.OnHide();
		m_BaseObject.SetActive(false);
	}

	public bool CanRefresh()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return ((VisualElement)m_btnRefresh).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		if (zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.LegionWar)
		{
			((TextElement)m_lbLeftTime).text = zbUtil.FormatTimeMMSS(zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
		if (IsAutoing())
		{
			((TextElement)m_lbAutoTime).text = (10 - autoLeftTime).ToString();
			if (autoLeftTime >= 10)
			{
				autoLeftTime = 0;
				int index = Rand.Range(0, 3);
				OnSelect(index);
			}
			autoLeftTime = Math.Min(autoLeftTime + 1, 10);
		}
	}

	private bool IsAutoing()
	{
		return zbSelf.Instance.combatHandler.IsAutoSelectMod(zbCombatSystem.Instance.GetBattleType());
	}

	private void RefreshAuto()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		EBattleFuncType battleType = zbCombatSystem.Instance.GetBattleType();
		bool flag = zbSelf.Instance.combatHandler.IsCanAutoSelectMod(battleType);
		bool flag2 = IsAutoing();
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		m_spriteLeft.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(flag & flag2)));
		((VisualElement)m_btnAuto).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_lbSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		autoLeftTime = 0;
		UpdatePerSecond();
	}

	private void OnAuto()
	{
		EBattleFuncType battleType = zbCombatSystem.Instance.GetBattleType();
		if (zbSelf.Instance.combatHandler.IsCanAutoSelectMod(battleType))
		{
			if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.AutoFight, zbSelf.Instance.GetVipLevel()) <= 0)
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11492"), zbSelf.Instance.vipHandler.CheckVipFuncUnlock(EVipFuncType.AutoFight)) + "【" + zbUtil.Language("lang.11494") + "】");
				return;
			}
			zbSelf.Instance.combatHandler.SetAutoSelectMod(battleType);
			RefreshAuto();
		}
	}
}
