using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbEndlessHordeFightForwardWnd : zbBaseWnd
{
	[NonSerialized]
	private VisualElement num_bg;

	[NonSerialized]
	private Button m_btnRefresh;

	private VisualElement icon_funcCard;

	[NonSerialized]
	private Label m_lbRefresh;

	[NonSerialized]
	private zbModPanel[] m_ModPanels = new zbModPanel[3];

	[NonSerialized]
	public List<int> selectMod = new List<int>();

	private VisualElement m_content;

	private int selectNum;

	private int selectMax;

	private zbCombatTeam m_masterTeam;

	private int curRefreshNum;

	private bool m_canSelect = true;

	[NonSerialized]
	private VisualElement[] m_numBg = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbNum = (Label[])(object)new Label[5];

	private TemplateContainer[] m_monsters = (TemplateContainer[])(object)new TemplateContainer[3];

	private Sprite m_monsterBg;

	private List<int> monsterIdList = new List<int>();

	private Label m_lbTitle;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_content = UQueryExtensions.Q<VisualElement>(m_RootElement, "content", (string)null);
		num_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "numBg", (string)null);
		m_btnRefresh = UQueryExtensions.Q<Button>(m_RootElement, "btn_refresh", (string)null);
		icon_funcCard = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_funcCard", (string)null);
		m_lbRefresh = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnRefresh, "lb_refresh", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRefresh, (Action)OnRefresh, "", (TrickleDown)0);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_titleName", (string)null);
		for (int i = 0; i < 3; i++)
		{
			zbModPanel zbModPanel2 = new zbModPanel(UQueryExtensions.Q<VisualElement>(m_RootElement, "mod" + i, (string)null));
			m_ModPanels[i] = zbModPanel2;
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_ModPanels[i].btnSelect, (Action<ClickEvent>)delegate
			{
				OnSelect(tempIndex);
			}, "", (TrickleDown)0);
			m_monsters[i] = UQueryExtensions.Q<TemplateContainer>(m_RootElement, "monster" + i, (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_monsters[i], (Action)delegate
			{
				OnClick(tempIndex);
			}, "", (TrickleDown)0);
		}
		for (int num = 0; num < 5; num++)
		{
			m_numBg[num] = UQueryExtensions.Q<VisualElement>(m_RootElement, "num_" + num, (string)null);
			m_lbNum[num] = UQueryExtensions.Q<Label>(m_RootElement, "lb_num" + num, (string)null);
		}
		AddDark();
	}

	private void InitMonster()
	{
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		BattleInfo battleInfo = zbCombatSystem.Instance.battleInfo;
		monsterIdList.Clear();
		foreach (BattleMonsterDetail monster in battleInfo.MonsterInfo.MonsterList)
		{
			zbMonsterCsvRecord recordByKey = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monster.MonsterId);
			if (recordByKey != null && recordByKey.type == 1 && monster.Summon == EBoolState.False)
			{
				monsterIdList.Add(monster.MonsterId);
			}
		}
		for (int i = 0; i < 3; i++)
		{
			bool flag = i < monsterIdList.Count;
			UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_monsters[i], "seal", (string)null).visible = !flag;
			UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_monsters[i], "monster", (string)null).visible = flag;
			if (flag)
			{
				SpineVisualElement val = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_monsters[i], "spine_monster", (string)null);
				zbMonsterCsvRecord recordByKey2 = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monsterIdList[i]);
				Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(zbDataConfig.Instance.monsterAvatarCsvTable.GetRecordByKey(recordByKey2.avatarId).shadow);
				VisualElement obj = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_monsters[i], "yingzi", (string)null);
				obj.style.scale = new StyleScale(new Vector2(recordByKey2.bodyScale, recordByKey2.bodyScale));
				IStyle style = UQueryExtensions.Q<VisualElement>(obj, "yingzi", (string)null).style;
				Rect rect = val2.rect;
				style.width = StyleLength.op_Implicit(((Rect)(ref rect)).width);
				IStyle style2 = UQueryExtensions.Q<VisualElement>(obj, "yingzi", (string)null).style;
				rect = val2.rect;
				style2.height = StyleLength.op_Implicit(((Rect)(ref rect)).height);
				UQueryExtensions.Q<VisualElement>(obj, "yingzi", (string)null).style.backgroundImage = new StyleBackground(val2);
				IStyle style3 = obj.style;
				rect = val2.rect;
				float height = ((Rect)(ref rect)).height;
				rect = val2.rect;
				style3.top = StyleLength.op_Implicit((height > ((Rect)(ref rect)).width) ? 130f : 188f);
				((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_monsters[i], "lb_name", (string)null)).text = recordByKey2.name.ToString();
				((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_monsters[i], "lb_name", (string)null)).style.color = new StyleColor(Color32.op_Implicit(new Color32((byte)66, (byte)67, (byte)87, byte.MaxValue)));
				string resName = zbDataConfig.Instance.monsterAvatarCsvTable.GetRecordByKey(recordByKey2.avatarId).fightBone + "_SkeletonData.asset";
				SkeletonDataAsset skeletonDataAsset = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(resName);
				if (val != null)
				{
					val.skeletonDataAsset = skeletonDataAsset;
					val.Initialize(true);
					val.unscaledTime = true;
					val.AnimationState.SetAnimation(0, "move", true);
				}
			}
		}
		int distance = zbSelf.Instance.endlessHordeHandler.GetDistance(zbServerData.sGameData.EndlessHordeInfo.Level);
		((TextElement)m_lbTitle).text = ((float)distance / 1000f).ToString("0.#") + "km";
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
		if (list.Count == 0)
		{
			return list2;
		}
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
		ChangeNum();
		List<zbModBattleRandCsvRecord> list = new List<zbModBattleRandCsvRecord>();
		RandomModId(list);
		selectMod = Pick3IdsWithWeight(list);
		RefreshItems();
		zbCombatSystem.Instance.combatRecorder.NeedFightForwardSave(selectNum, selectMax, curRefreshNum, selectMod);
		CaclBtnRefresh();
	}

	public void CaclBtnRefresh()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (curRefreshNum == 0 && zbCombatSystem.Instance.refreshSelectNum > 0)
		{
			((VisualElement)m_btnRefresh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (zbCombatSystem.Instance.refreshSelectNum > 1)
			{
				icon_funcCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((TextElement)m_lbRefresh).text = "";
			}
			else
			{
				icon_funcCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
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

	public void RefreshCustom(int _selectMax)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		InitMonster();
		selectNum = 0;
		curRefreshNum = 0;
		selectMax = _selectMax;
		m_masterTeam = zbCombatSystem.Instance.combatData.masterTeam;
		num_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(selectMax <= 1));
		zbCombatWnd obj = zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd;
		obj.SetAlertRandgeVisible(null, vis: false);
		obj.SetVisibleUI(flag: false);
		zbCombatSystem.Instance.Pause();
		zbAudioMgr.Instance.PauseAllAudio();
		RefreshDetail();
		SetElementVisible(visible: true);
		if (zbSelf.Instance.combatHandler.IsZuobi())
		{
			zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10667"), zbUtil.Language("lang.10668") + "_6", OnZuobiConfirm);
			zbCombatSystem.Instance.combatRecorder.ClearRecord();
		}
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
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		InitMonster();
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
		zbCombatSystem.Instance.combatRecorder.NeedFightForwardSave(selectNum, selectMax, curRefreshNum, selectMod);
		CaclBtnRefresh();
		SetElementVisible(visible: true);
	}

	public void RefreshItems()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			if (selectMod.Count > i)
			{
				int key = selectMod[i];
				zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(key);
				m_ModPanels[i].RefreshDetail(m_masterTeam, recordByKey);
				((VisualElement)UQueryExtensions.Q<TemplateContainer>(m_RootElement, "mod" + i, (string)null)).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			else
			{
				((VisualElement)UQueryExtensions.Q<TemplateContainer>(m_RootElement, "mod" + i, (string)null)).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
	}

	public void OnSelect(int index)
	{
		if (m_canSelect)
		{
			int modId = selectMod[index];
			m_masterTeam.UseSelectMode(modId);
			zbCombatWnd zbCombatWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd;
			if ((Object)(object)zbCombatWnd2 != (Object)null)
			{
				zbCombatWnd2.refreshSkill();
			}
			selectMod.Clear();
			selectNum++;
			m_canSelect = false;
			if (selectNum < selectMax)
			{
				RefreshDetail();
				return;
			}
			zbCombatSystem.Instance.combatRecorder.Save();
			OnHideSelf();
			zbUIMgr.Instance.ShowBossUp();
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
		zbCombatSystem.Instance.Resume();
		zbAudioMgr.Instance.ResumeAllAudio();
	}

	public void OnRefresh()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (curRefreshNum == 0 && zbCombatSystem.Instance.refreshSelectNum > 0)
		{
			((VisualElement)m_btnRefresh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (zbCombatSystem.Instance.refreshSelectNum > 1 || zbCombatSystem.Instance.refreshSelectNum >= 1)
			{
				curRefreshNum++;
				zbCombatSystem.Instance.refreshSelectNum--;
				RefreshDetail();
			}
		}
	}

	private void SetElementVisible(bool visible)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		num_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!visible || selectMax <= 1) ? 1 : 0));
		m_content.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!visible));
		(zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd).SetVisibleUI(!visible);
	}

	protected override void OnHide()
	{
		base.OnHide();
	}

	public bool CanRefresh()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return ((VisualElement)m_btnRefresh).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private void OnClick(int index)
	{
		if (monsterIdList.Count() > index)
		{
			zbMonsterCsvRecord recordByKey = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monsterIdList[index]);
			if (recordByKey != null)
			{
				zbUIMgr.Instance.ShowPreviewMonsterDetail(recordByKey);
			}
		}
	}
}
