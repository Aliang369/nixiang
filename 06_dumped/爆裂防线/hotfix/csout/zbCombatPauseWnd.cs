using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCombatPauseWnd : zbTabWnd
{
	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[3];

	private Button[] m_tabButtons = (Button[])(object)new Button[3];

	private TemplateContainer[] m_monsters = (TemplateContainer[])(object)new TemplateContainer[3];

	private zbCombatBalanceAward m_panelAward;

	private zbCombatBalanceDamage m_panelDamage;

	private zbCombatBalanceSet m_panelSet;

	private Button m_btnExit;

	private Button m_btnResume;

	private Sprite m_monsterBg;

	private List<int> monsterIdList = new List<int>();

	private Label m_lbLeftTime;

	protected override void Awake()
	{
		base.Awake();
		m_panelAward = new zbCombatBalanceAward();
		m_panelDamage = new zbCombatBalanceDamage();
		m_panelSet = new zbCombatBalanceSet();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_monsterBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/monster/monster_bg2.png");
		m_lbLeftTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_result", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab" + i, (string)null);
			m_monsters[i] = UQueryExtensions.Q<TemplateContainer>(m_RootElement, "monster" + i, (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_monsters[i], (Action)delegate
			{
				OnClick(tempIndex);
			}, "", (TrickleDown)0);
		}
		m_panelAward.OnEnable(this, m_tabLayers[0]);
		m_panelDamage.OnEnable(this, m_tabLayers[1]);
		m_panelSet.OnEnable(this, m_tabLayers[2]);
		m_btnExit = UQueryExtensions.Q<Button>(m_RootElement, "btn_exit", (string)null);
		m_btnResume = UQueryExtensions.Q<Button>(m_RootElement, "btn_resume", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnExit, (Action<ClickEvent>)OnExit, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnResume, (Action<ClickEvent>)OnResume, "", (TrickleDown)0);
		InitTab(m_tabButtons, m_tabLayers, null);
		AddDark();
	}

	public override void Refresh()
	{
		base.Refresh();
		SelectTab(0);
		m_panelAward.RefreshPauseAward();
		m_panelDamage.Refresh();
		InitMonster();
		zbCombatSystem.Instance.Pause();
		zbAudioMgr.Instance.PauseAllAudio();
		if (zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.LegionWar)
		{
			((TextElement)m_lbLeftTime).text = zbUtil.FormatTimeMMSS(zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
		else
		{
			((TextElement)m_lbLeftTime).text = zbUtil.Language("暂停");
		}
	}

	private void InitMonster()
	{
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
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
				UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_monsters[i], "frame", (string)null).style.backgroundImage = new StyleBackground(m_monsterBg);
				UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_monsters[i], "frame", (string)null).style.height = StyleLength.op_Implicit(270f);
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
				Button val3 = UQueryExtensions.Q<Button>((VisualElement)(object)m_monsters[i], "btn_type", (string)null);
				((VisualElement)val3).pickingMode = (PickingMode)1;
				if (recordByKey2.elementType == 0)
				{
					((VisualElement)val3).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				}
				else
				{
					((VisualElement)val3).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					((VisualElement)val3).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/monster/monster_type{recordByKey2.elementType}.png"));
				}
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
	}

	public override void DoSelectTab()
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		switch (m_currTab)
		{
		case 0:
			m_panelAward.RefreshPauseAward();
			break;
		case 1:
			m_panelDamage.Refresh();
			break;
		case 2:
			m_panelSet.Refresh();
			break;
		}
		for (int i = 0; i < 3; i++)
		{
			if (m_currTab == i)
			{
				Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/combat/balance_tabse.png");
				((VisualElement)m_tabButtons[i]).style.backgroundImage = new StyleBackground(val);
			}
			else
			{
				((VisualElement)m_tabButtons[i]).style.backgroundImage = StyleBackground.op_Implicit((Texture2D)null);
			}
			((VisualElement)m_tabButtons[i]).style.opacity = StyleFloat.op_Implicit(1f);
		}
	}

	public void OnExit(ClickEvent evt)
	{
		if (zbCombatSystem.Instance.isPVPBattleType)
		{
			zbCombatSystem.Instance.combatData.defense.SetCurHp(0L);
		}
		Hide();
		zbAudioMgr.Instance.ResumeAllAudio();
		zbCombatSystem.Instance.combatStateMachine.SetState(ECombatState.Balance);
		OnResume(evt);
		if (zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.MainQuest)
		{
			if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId == 1)
			{
				zbProgressEventHelper.Instance.SetProgressTrack(606);
			}
			else if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId == 2)
			{
				zbProgressEventHelper.Instance.SetProgressTrack(607);
			}
			else if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId == 3)
			{
				zbProgressEventHelper.Instance.SetProgressTrack(608);
			}
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		if (zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.LegionWar)
		{
			((TextElement)m_lbLeftTime).text = zbUtil.FormatTimeMMSS(zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
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

	public void OnResume(ClickEvent evt)
	{
		zbCombatSystem.Instance.Resume();
		zbAudioMgr.Instance.ResumeAllAudio();
		Hide();
	}
}
