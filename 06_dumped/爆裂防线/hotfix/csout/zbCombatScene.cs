using Proto.Enums;
using UnityEngine;

public class zbCombatScene
{
	public int sceneId = -1;

	private zbSpineScene normalScene;

	private zbScrollScene scrollScene;

	private GameObject m_night_bg;

	private Transform night_scene;

	public Transform top;

	public BoxCollider2D wallTopBox;

	public BoxCollider2D wallBottomBox;

	public BoxCollider2D wallLeftBox;

	public BoxCollider2D wallRightBox;

	private zbCombatData combatData;

	public void Init(zbCombatData _combatData)
	{
		//IL_0561: Unknown result type (might be due to invalid IL or missing references)
		//IL_0536: Unknown result type (might be due to invalid IL or missing references)
		combatData = _combatData;
		EBattleFuncType battleType = zbCombatSystem.Instance.GetBattleType();
		int enemyId = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId;
		sceneId = 1;
		switch (battleType)
		{
		case EBattleFuncType.MainQuest:
			sceneId = zbDataConfig.Instance.mainLevelCsvTable.GetRecordByKey(enemyId).sceneId;
			break;
		case EBattleFuncType.EliteQuest:
			sceneId = zbDataConfig.Instance.eliteLevelCsvTable.GetRecordByKey(enemyId).sceneId;
			break;
		case EBattleFuncType.DungeonEasyFight:
			sceneId = zbDataConfig.Instance.dungeonCsvTable.GetRecordByKey(enemyId).sceneId;
			break;
		case EBattleFuncType.DungeonBossFight:
			sceneId = zbDataConfig.Instance.dungeonCsvTable.GetRecordByKey(enemyId).sceneId;
			break;
		case EBattleFuncType.DungeonRiotFight:
			sceneId = zbDataConfig.Instance.dungeonCsvTable.GetRecordByKey(enemyId).sceneId;
			break;
		case EBattleFuncType.LegionBoss:
			sceneId = zbDataConfig.Instance.legionBossChallengeCsvTable.GetRecordByKey(enemyId).sceneId;
			break;
		case EBattleFuncType.WorldBoss:
			sceneId = zbDataConfig.Instance.worldBossCsvTable.GetRecordByKey(enemyId).sceneId;
			break;
		case EBattleFuncType.WorldLair:
			sceneId = zbDataConfig.Instance.worldLairCsvTable.GetRecordByKey(enemyId).sceneId;
			break;
		case EBattleFuncType.WorldLairHelp:
			sceneId = zbDataConfig.Instance.worldLairCsvTable.GetRecordByKey(enemyId).sceneId;
			break;
		case EBattleFuncType.WorldRescueDay:
		case EBattleFuncType.WorldRescueNight:
		case EBattleFuncType.WorldEventFightDay:
		case EBattleFuncType.WorldEventFightNight:
			sceneId = zbDataConfig.Instance.worldEventFightCsvTable.GetRecordByKey(enemyId).sceneId;
			break;
		case EBattleFuncType.HeroTrialQiang:
		case EBattleFuncType.HeroTrialPao:
		case EBattleFuncType.HeroTrialJiguang:
		case EBattleFuncType.HeroTrialBingdan:
		case EBattleFuncType.HeroTrialRanshaodan:
		case EBattleFuncType.HeroTrialHongzha:
		case EBattleFuncType.HeroTrialDianci:
		case EBattleFuncType.HeroTrialWurenji:
		case EBattleFuncType.HeroTrialMotuo:
		case EBattleFuncType.HeroTrialBingbao:
		case EBattleFuncType.HeroTrialShexian:
		case EBattleFuncType.HeroTrialDianqiu:
		case EBattleFuncType.HeroTrialLongjuan:
		case EBattleFuncType.HeroTrialFengren:
			sceneId = zbDataConfig.Instance.heroTrialCsvTable.GetRecordByKey(enemyId).sceneId;
			break;
		case EBattleFuncType.ElementTrialElec:
		case EBattleFuncType.ElementTrialFire:
		case EBattleFuncType.ElementTrialIce:
		case EBattleFuncType.ElementTrialPhy:
		case EBattleFuncType.ElementTrialEnergy:
		case EBattleFuncType.ElementTrialWind:
			sceneId = zbDataConfig.Instance.elementTrialCsvTable.GetRecordByKey(enemyId).sceneId;
			break;
		case EBattleFuncType.EndlessHorde:
		{
			zbEndlessHordeLevelCsvRecord item = zbSelf.Instance.endlessHordeHandler.GetRecord(enemyId).curRecord;
			sceneId = item.sceneId;
			break;
		}
		case EBattleFuncType.Legendary:
			sceneId = zbDataConfig.Instance.legendaryLevelCsvTable.GetRecordByKey(enemyId).sceneId;
			break;
		case EBattleFuncType.GuardVerify:
		case EBattleFuncType.Arena:
		case EBattleFuncType.CrossLadderMatch:
		case EBattleFuncType.CrossLadderUpgrade:
		case EBattleFuncType.PeakArena:
			sceneId = 1;
			break;
		case EBattleFuncType.MineWar:
		case EBattleFuncType.CrossMine:
		{
			zbMineNodeCsvRecord recordByKey2 = zbDataConfig.Instance.mineNodeCsvTable.GetRecordByKey(zbSelf.Instance.mineWarHandler.currPointId);
			if (recordByKey2 != null)
			{
				sceneId = recordByKey2.sceneId;
			}
			else
			{
				sceneId = 1;
			}
			break;
		}
		case EBattleFuncType.BossSubject:
		{
			zbBossSubjectCsvRecord recordByKey = zbDataConfig.Instance.bossSubjectCsvTable.GetRecordByKey(enemyId);
			if (recordByKey != null)
			{
				sceneId = recordByKey.sceneId;
			}
			else
			{
				sceneId = 1;
			}
			break;
		}
		case EBattleFuncType.WallSubject:
			sceneId = zbSelf.Instance.wallSubjectHandler.GetSceneId(enemyId, zbSelf.Instance.wallSubjectHandler.currLevelId);
			break;
		case EBattleFuncType.MechaSubject:
			sceneId = zbSelf.Instance.mechaSubjectHandler.GetSceneId(enemyId, zbSelf.Instance.mechaSubjectHandler.currLevelId);
			break;
		default:
			sceneId = 1;
			break;
		}
		if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleModel == EBattleModelType.Pvp && combatData.battleSideTeamInfo.MasterTeamInfo.FootholdInfo.FootholdId > 0)
		{
			sceneId = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(combatData.battleSideTeamInfo.MasterTeamInfo.FootholdInfo.FootholdId).sceneId;
		}
		normalScene = ((Component)combatData.nodeRoot.Find("normalScene")).GetComponent<zbSpineScene>();
		scrollScene = ((Component)combatData.nodeRoot.Find("scrollScene")).GetComponent<zbScrollScene>();
		normalScene.skeletonObject.SetActive(false);
		if (zbCombatSystem.Instance.GetDefenseType() == EDefenseType.CarDefense)
		{
			scrollScene.Load(sceneId);
			scrollScene.SetVisible(flag: true);
			((Component)normalScene).gameObject.SetActive(false);
		}
		else
		{
			normalScene.Load(sceneId);
			((Component)normalScene).gameObject.SetActive(true);
			scrollScene.SetVisible(flag: false);
			if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleModel == EBattleModelType.Pvp && combatData.battleSideTeamInfo.MasterTeamInfo.FootholdInfo.FootholdId <= 1 && combatData.dataIndex == 2)
			{
				((Component)normalScene).gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
			}
			else
			{
				((Component)normalScene).gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			}
		}
		night_scene = combatData.nodeRoot.Find("nightScene");
		m_night_bg = ((Component)combatData.nodeRoot.Find("nightScene").Find("night_bg")).gameObject;
		top = combatData.nodeRoot.Find("InvisibleWall").Find("InvisibleWall_top");
		wallTopBox = ((Component)combatData.nodeRoot.Find("InvisibleWall").Find("InvisibleWall_top")).gameObject.GetComponent<BoxCollider2D>();
		wallBottomBox = ((Component)combatData.nodeRoot.Find("InvisibleWall").Find("InvisibleWall_bom")).gameObject.GetComponent<BoxCollider2D>();
		wallLeftBox = ((Component)combatData.nodeRoot.Find("InvisibleWall").Find("InvisibleWall_left")).gameObject.GetComponent<BoxCollider2D>();
		wallRightBox = ((Component)combatData.nodeRoot.Find("InvisibleWall").Find("InvisibleWall_right")).gameObject.GetComponent<BoxCollider2D>();
		RefreshTopPositionY();
	}

	public void RefreshTopPositionY(float offsetY = 0f)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (zbCombatSystem.Instance.isPVPBattleType)
		{
			if (combatData.dataIndex == 2)
			{
				top.position = new Vector3(top.position.x, 2.0999994f + offsetY, top.position.z);
			}
			else
			{
				top.position = new Vector3(top.position.x, 2.0999994f + offsetY, top.position.z);
			}
		}
		else
		{
			top.position = new Vector3(top.position.x, 8.4f, top.position.z);
		}
	}

	public void RefreshSceneAlertRange(float sceneAlertRange)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		((Component)night_scene).gameObject.SetActive(sceneAlertRange <= 16f);
		m_night_bg.transform.position = Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, sceneAlertRange - 6f - 3f));
	}

	public void Free()
	{
	}
}
