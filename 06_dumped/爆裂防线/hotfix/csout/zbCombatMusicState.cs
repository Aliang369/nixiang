using System.Collections.Generic;
using Proto.Enums;

public class zbCombatMusicState(EMusicState eState) : zbMusicState(eState)
{
	private List<string> filePath = new List<string>();

	private float m_fScaleFactor = 0.5f;

	public override void OnEnter()
	{
		base.OnEnter();
		EBattleFuncType battleType = zbCombatSystem.Instance.GetBattleType();
		switch (battleType)
		{
		case EBattleFuncType.MainQuest:
			filePath.Clear();
			filePath.Add("audio/music/rm_fight1.ogg");
			filePath.Add("audio/music/rm_fight3.ogg");
			filePath.Add("audio/music/rm_fight6.ogg");
			zbMusicMgr.Instance.Play(filePath[Rand.Range(1, 4) - 1], m_fScaleFactor);
			break;
		case EBattleFuncType.EliteQuest:
		case EBattleFuncType.DungeonEasyFight:
		case EBattleFuncType.DungeonRiotFight:
		case EBattleFuncType.DungeonBossFight:
			filePath.Clear();
			filePath.Add("audio/music/rm_fight4.ogg");
			filePath.Add("audio/music/rm_fight5.ogg");
			zbMusicMgr.Instance.Play(filePath[Rand.Range(1, 3) - 1], m_fScaleFactor);
			break;
		default:
			if (battleType == EBattleFuncType.WorldBoss || battleType == EBattleFuncType.BossSubject)
			{
				zbMusicMgr.Instance.Play("audio/music/rm_fight2.ogg", m_fScaleFactor);
			}
			else
			{
				zbMusicMgr.Instance.Play("audio/music/rm_fight" + Rand.Range(1, 7) + ".ogg", m_fScaleFactor);
			}
			break;
		}
	}
}
