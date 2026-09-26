using System.Collections.Generic;

public class stCombatRecorderData
{
	public long roleId;

	public stElementTrial elementTrial = new stElementTrial();

	public stWorldLair worldLair = new stWorldLair();

	public stWorldBoss worldBoss = new stWorldBoss();

	public stEliteQuest eliteQuest = new stEliteQuest();

	public stLegendary legendary = new stLegendary();

	public stArena arena = new stArena();

	public stGuardVerify guardVerify = new stGuardVerify();

	public stCrossLadder crossLadder = new stCrossLadder();

	public stMineWar mineWar = new stMineWar();

	public stPeakArena peakArena = new stPeakArena();

	public stTruck truck = new stTruck();

	public stLegionWar legionWar = new stLegionWar();

	public stBossSubject bossSubject = new stBossSubject();

	public stWallSubject wallSubject = new stWallSubject();

	public stMechaSubject mechaSubject = new stMechaSubject();

	public stRecordCombatSystem recordCombatSystem = new stRecordCombatSystem();

	public stRecordDefense recordDefense = new stRecordDefense();

	public stRecordMonsterSpawn recordMonsterSpawn = new stRecordMonsterSpawn();

	public stRecordCombatExp recordExp = new stRecordCombatExp();

	public List<stRecordMonster> monsterList = new List<stRecordMonster>();

	public stCurrSelectMod curSelectMod = new stCurrSelectMod();

	public stCurrModWheel curModWheel = new stCurrModWheel();

	public stCurrFightForwardWnd currFightForward = new stCurrFightForwardWnd();

	public stMonster3006Reborn monster3006Reborn = new stMonster3006Reborn();

	public stMonsterBoss monsterBoss = new stMonsterBoss();

	public List<stRecordUseSelectMod> userSelectModList = new List<stRecordUseSelectMod>();

	public List<stRecordHeroDamageStat> heroDamageStats = new List<stRecordHeroDamageStat>();

	public long damageBossSum;

	public List<stRecordHero> heroList = new List<stRecordHero>();

	public stRecordHeroQiang heroQiang = new stRecordHeroQiang();

	public stRecordTeamComm teamCommom = new stRecordTeamComm();

	public List<stRecordUseSelectMod> userHelpSelectModList = new List<stRecordUseSelectMod>();

	public List<stRecordHeroDamageStat> heroHelpDamageStats = new List<stRecordHeroDamageStat>();

	public List<stRecordHero> heroHelpList = new List<stRecordHero>();

	public stRecordHeroQiang heroHelpQiang = new stRecordHeroQiang();

	public stRecordTeamComm teamHelpCommom = new stRecordTeamComm();
}
