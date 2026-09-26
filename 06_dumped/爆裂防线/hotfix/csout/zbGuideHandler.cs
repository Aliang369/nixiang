using Proto.Enums;

public class zbGuideHandler
{
	private string m_FirstEnterGameKey;

	private bool m_isFirstEnterGame;

	private string m_firstEnterHomeWndKey;

	private bool m_isFirstEnterHomeWnd;

	private string m_FirstMainQuestBattleKey;

	private bool m_isFirstMainQuestBattle = true;

	private string m_FirstSelectHeroKey;

	private bool m_isFirstSelectHero;

	private bool m_bOpenGuide = true;

	public void Init()
	{
		m_FirstEnterGameKey = "isFirstEnterGameWnd" + zbSelf.Instance.GetRoleId();
		m_isFirstEnterGame = zbGameSave.Instance.GetBool(m_FirstEnterGameKey, defaultValue: true);
		m_firstEnterHomeWndKey = "isFirstEnterHomeWnd" + zbSelf.Instance.GetRoleId();
		m_isFirstEnterHomeWnd = zbGameSave.Instance.GetBool(m_firstEnterHomeWndKey, defaultValue: true);
		m_FirstMainQuestBattleKey = "isFirstMainQuestBattle" + zbSelf.Instance.GetRoleId();
		m_isFirstMainQuestBattle = zbGameSave.Instance.GetBool(m_FirstMainQuestBattleKey, defaultValue: true);
		m_FirstSelectHeroKey = "isFirstSelectHero" + zbSelf.Instance.GetRoleId();
		m_isFirstSelectHero = true;
	}

	public void SetOpenGuide(bool value)
	{
		m_bOpenGuide = value;
	}

	public bool IsFirstEnterGame()
	{
		return m_isFirstEnterGame;
	}

	public void SetFirstEnterGame()
	{
		m_isFirstEnterGame = false;
		zbGameSave.Instance.SetBool(m_FirstEnterGameKey, value: false);
	}

	public bool IsFirstEnterHomeWnd()
	{
		if (m_bOpenGuide)
		{
			return m_isFirstEnterHomeWnd;
		}
		return false;
	}

	public void SetEnteredHomeWnd()
	{
		m_isFirstEnterHomeWnd = false;
		zbGameSave.Instance.SetBool(m_firstEnterHomeWndKey, value: false);
	}

	public bool IsFirstLevelMod()
	{
		if (m_bOpenGuide && zbCombatSystem.Instance != null && zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.MainQuest && zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId == 1)
		{
			return (int)zbCombatSystem.Instance.combatData.combatExp.curLevel == 1;
		}
		return false;
	}

	public bool IsFirstMainQuestBattle()
	{
		if (m_bOpenGuide && m_isFirstMainQuestBattle && zbCombatSystem.Instance != null && zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.MainQuest)
		{
			return zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId == 1;
		}
		return false;
	}

	public bool IsFirstSelectHero(ESkillType skillType)
	{
		if (m_bOpenGuide && skillType != ESkillType.Qiang && m_isFirstSelectHero && zbCombatSystem.Instance != null && zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.MainQuest)
		{
			return zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId == 1;
		}
		return false;
	}

	public void SetFirstSelectHero(bool flag)
	{
		zbGameSave.Instance.SetBool(m_FirstSelectHeroKey, m_isFirstSelectHero);
	}

	public void OnResume()
	{
		m_isFirstMainQuestBattle = false;
		zbCombatSystem.Instance.Resume();
		zbAudioMgr.Instance.ResumeAllAudio();
		zbGameSave.Instance.SetBool(m_FirstMainQuestBattleKey, m_isFirstMainQuestBattle);
	}
}
