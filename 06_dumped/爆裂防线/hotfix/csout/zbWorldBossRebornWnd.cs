using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldBossRebornWnd : zbBaseWnd
{
	private Label m_lbCountDown;

	private Button m_btnCancel;

	private Button m_btnConfim;

	private VisualElement m_iconBg;

	private VisualElement m_progress;

	private VisualElement m_award;

	private int m_timeout = 20;

	private float m_rotateAngle;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbCountDown = UQueryExtensions.Q<Label>(m_RootElement, "lb_countDown", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_btnConfim = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_iconBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_bg", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_award = UQueryExtensions.Q<VisualElement>(m_RootElement, "award", (string)null);
		m_iconBg.schedule.Execute((Action)delegate
		{
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			float deltaTime = Time.deltaTime;
			m_rotateAngle += 60f * deltaTime;
			if (m_rotateAngle >= 360f)
			{
				m_rotateAngle -= 360f;
			}
			m_iconBg.transform.rotation = Quaternion.Euler(0f, 0f, m_rotateAngle);
		}).Every(0L);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfim, (Action<ClickEvent>)OnComfirm, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_WorldBoss_Reborn, OnEventUpdateWorldBossReborn);
		AddDark();
	}

	public void RefreshCustom()
	{
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		zbCombatSystem.Instance.Pause();
		zbAudioMgr.Instance.PauseAllAudio();
		m_award.Clear();
		int enemyId = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId;
		long nextStepDmg = zbSelf.Instance.worldBossHandler.GetNextStepDmg(enemyId, zbCombatSystem.Instance.combatData.masterTeam.combatStat.damageBossSum);
		zbItemData nextStepAward = zbSelf.Instance.worldBossHandler.GetNextStepAward();
		if (nextStepAward != null)
		{
			m_award.Add((VisualElement)(object)zbSelf.Instance.combatHandler.CreatezbItemByAwardPreview(nextStepAward));
		}
		if (nextStepDmg > 0)
		{
			float num = (float)zbCombatSystem.Instance.combatData.masterTeam.combatStat.damageBossSum / (float)nextStepDmg * 100f;
			UQueryExtensions.Q<VisualElement>(m_progress, "progressVal", (string)null).style.width = new StyleLength(new Length(Mathf.Min(num, 100f), (LengthUnit)1));
			((TextElement)UQueryExtensions.Q<Label>(m_progress, "lb_progressVal", (string)null)).text = zbUtil.ConvertToTBMK(zbCombatSystem.Instance.combatData.masterTeam.combatStat.damageBossSum) + "/" + zbUtil.ConvertToTBMK(nextStepDmg, isFloor: false);
		}
		else
		{
			UQueryExtensions.Q<VisualElement>(m_progress, "progressVal", (string)null).style.width = new StyleLength(new Length(100f, (LengthUnit)1));
			((TextElement)UQueryExtensions.Q<Label>(m_progress, "lb_progressVal", (string)null)).text = zbUtil.ConvertToTBMK(zbCombatSystem.Instance.combatData.masterTeam.combatStat.damageBossSum) + "/Max";
		}
		m_timeout = 20;
		((TextElement)m_lbCountDown).text = m_timeout.ToString();
	}

	private void OnClose()
	{
		zbCombatSystem.Instance.Resume();
		zbAudioMgr.Instance.ResumeAllAudio();
		zbCombatSystem.Instance.fightRebornNum--;
		Hide();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		if (m_timeout <= 0)
		{
			OnClose();
			return;
		}
		m_timeout--;
		((TextElement)m_lbCountDown).text = m_timeout.ToString();
	}

	public void OnComfirm(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.WorldBoss_FightReborn_Req(zbSelf.Instance.worldBossHandler.worldBossuuid, zbCombatSystem.Instance.GetBattleUUID());
	}

	private void OnEventUpdateWorldBossReborn(object data)
	{
		zbCombatSystem.Instance.combatData.defense.immunityDuration += 30f;
		zbCombatSystem.Instance.combatData.defense.SetCurHp(zbCombatSystem.Instance.combatData.defense.maxHp);
		zbCombatSystem.Instance.maxTime += 120;
		zbCombatSystem.Instance.countDown.Value += 120f;
		OnClose();
	}
}
