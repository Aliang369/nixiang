using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Types;
using Spine.Unity;
using UnityEngine;

public class zbCombatData
{
	public List<stWallSkillIndex5021> wallSkillIndex5021DataList = new List<stWallSkillIndex5021>();

	public List<stWallSkillIndex5031> wallSkillIndex5031DataList = new List<stWallSkillIndex5031>();

	public List<stWallSkillIndex5041> wallSkillIndex5041DataList = new List<stWallSkillIndex5041>();

	private float[] xs = new float[5] { -2.88f, -1.44f, 0f, 1.44f, 2.88f };

	public List<stWallSkillIndex5051> wallSkillIndex5051DataList = new List<stWallSkillIndex5051>();

	public List<stWallSkillIndex5061> wallSkillIndex5061DataList = new List<stWallSkillIndex5061>();

	public List<stWallSkillIndex5071> wallSkillIndex5071DataList = new List<stWallSkillIndex5071>();

	public zbWallCsvRecord wallRecord;

	public Transform nodeRoot;

	public Transform nodeRootHero;

	public Transform nodeRootHeroSkill;

	public Transform nodeRootDefense;

	public Transform nodeRootDefenseHp;

	public Transform nodeRootBuffEffect;

	public Transform nodeRootMonsterSkillEffect;

	public zbCombatScene combatScene;

	public zbCombatTeam masterTeam;

	public zbCombatTeam helperTeam;

	public zbCombatMonster monsters;

	public zbCombatDefense defense;

	public zbCombatDefenseHp defenseHp;

	public zbMonsterSpawner monsterSpawner;

	public zbAlertRangeGizmo alertRangeGizmo;

	public zbMechaRegister mechaRegister;

	public zbPetRegister petRegister;

	public zbFootholdRegister footholdRegister;

	public zbCombatExp combatExp;

	public zbCombatRoute combatRoute;

	public zbCombatBubble combatBubble;

	public zbMonsterHud combatHud;

	public zbSkillPoolMgr skillPoolMgr;

	public zbMonsterPoolMgr monsterPoolMgr;

	public zbMonsterDieFxPool monsterDieFxPool;

	public zbMonsterBulletEffectPoolMgr monsterBulletEffectPoolMgr;

	public zbBuffEffectPoolMgr buffEffectPoolMgr;

	public zbMonsterEyeEffectPoolMgr monsterEyeEffectPoolMgr;

	public zbMonsterDieEffectPoolMgr monsterDieEffectPoolMgr;

	public zbMonsterHitEffectPoolMgr monsterHitEffectPoolMgr;

	public zbMonsterEffectPoolMgr monsterEffectPoolMgr;

	public zbMonsterOnceEffectPoolMgr monsterOnceEffectPoolMgr;

	public zbSkillEffectPoolMgr skillEffectPoolMgr;

	public zbWallSkillMgr defenseMgr;

	public zbSpineBatchRenderer spineBatchRenderer;

	public zbSpineGpuAnimRenderer spineGpuAnimRenderer;

	public float sceneAlertRange;

	public Camera mainCamera;

	private int maxBubbleActive = 200;

	private int curBubbleActive = 200;

	public int dataIndex;

	public float nodeRootPositonX;

	public float nodeRootPositonY;

	public Camera spineCullCamera;

	public float spineCullMinY = float.MinValue;

	public float spineCullMaxY = float.MaxValue;

	public float spineFarBandY = float.MinValue;

	private float m_spineWindowRefreshTimer;

	private const float kSpineCullMargin = 1f;

	private GameObject m_Chuchang;

	private SkeletonAnimation m_ChuchangAnim;

	public int pvpMonsterDieNumber;

	public readonly int pvpMonsterDieNumberMax = 10;

	public Dictionary<int, int> pvpMonsterDieList = new Dictionary<int, int>();

	public BattleSideTeamInfo battleSideTeamInfo { get; set; }

	public void TriggerAction5021()
	{
		foreach (stWallSkillIndex5021 wallSkillIndex5021Data in wallSkillIndex5021DataList)
		{
			if (wallSkillIndex5021Data.triggerNumber > 0 && (float)wallSkillIndex5021Data.hpGreaterPresent > 0f)
			{
				wallSkillIndex5021Data.triggerNumber--;
				wallSkillIndex5021Data.saveRecord[0] = wallSkillIndex5021Data.triggerNumber;
				wallSkillIndex5021Data.TriggerAction(this);
				break;
			}
		}
	}

	public void TriggerAction5031()
	{
		foreach (stWallSkillIndex5031 wallSkillIndex5031Data in wallSkillIndex5031DataList)
		{
			if ((int)wallSkillIndex5031Data.shootNumber > 0)
			{
				wallSkillIndex5031Data.TriggerAction(this);
			}
		}
	}

	public void TriggerAction5041()
	{
		List<stWallSkillIndex5041> list = wallSkillIndex5041DataList;
		int count = list.Count;
		if (count > 0)
		{
			_ = list[0];
		}
		if (count > 1)
		{
			_ = list[1];
		}
		for (int i = 0; i < 5; i++)
		{
			TriggerAction5041Index(i);
		}
	}

	public void TriggerAction5041Index(int i)
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		float num = 1.7f + defense.GetAlertPositionY() + defense.GetAlertOffsetY() + nodeRootPositonY;
		List<stWallSkillIndex5041> list = wallSkillIndex5041DataList;
		int count = list.Count;
		stWallSkillIndex5041 stWallSkillIndex5082 = ((count > 0) ? list[0] : null);
		stWallSkillIndex5041 stWallSkillIndex5083 = ((count > 1) ? list[1] : null);
		stWallSkillIndex5041 stWallSkillIndex5084 = null;
		if (stWallSkillIndex5082 != null && stWallSkillIndex5082.saveRecord[i] != -1)
		{
			stWallSkillIndex5084 = stWallSkillIndex5082;
		}
		else if (stWallSkillIndex5083 != null && stWallSkillIndex5083.saveRecord[i] != -1)
		{
			stWallSkillIndex5084 = stWallSkillIndex5083;
		}
		if (stWallSkillIndex5084 != null)
		{
			zbWall5041 obj = (zbWall5041)zbSkillSystem.Instance.CreateSkill("prefab/combat/wall/wall_5041.prefab", defense, new Vector3(xs[i] + nodeRootPositonX, num, 0f), Vector2.up);
			obj.wallSkillIndex5041Data = stWallSkillIndex5084;
			obj.SetWallAtr((float)stWallSkillIndex5084.damagePrecent * (float)(int)stWallSkillIndex5084.wallSkillIndex5041.team.teamAttack, stWallSkillIndex5084.wallSkillIndex5041.wallSkillRecord);
			obj.sortIndex = i;
		}
	}

	public void TriggerAction5041Passive(zbMonster attacker)
	{
		foreach (stWallSkillIndex5041 wallSkillIndex5041Data in wallSkillIndex5041DataList)
		{
			if ((int)wallSkillIndex5041Data.addHpByKillUnCall > 0)
			{
				defense.ChangeHp(attacker.GetCurPositionXY().x, (int)wallSkillIndex5041Data.addHpByKillUnCall);
			}
		}
	}

	public void TriggerAction5051(zbMonster attacker)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)attacker != (Object)null))
		{
			return;
		}
		foreach (stWallSkillIndex5051 wallSkillIndex5051Data in wallSkillIndex5051DataList)
		{
			if ((int)wallSkillIndex5051Data.triggerNumber > 0)
			{
				wallSkillIndex5051Data.DesShootNumber();
				zbAudioMgr.Instance.PlayAudio("audio/combat/diancichuanci.ogg");
				zbWall5051 obj = (zbWall5051)zbSkillSystem.Instance.CreateSkill("prefab/combat/wall/wall_5051.prefab", defense, ((Component)attacker).transform.position, Vector2.up);
				obj.wallSkillIndex5051Data = wallSkillIndex5051Data;
				obj.SetWallAtr((float)wallSkillIndex5051Data.damagePrecent * (float)(int)wallSkillIndex5051Data.wallSkillIndex5051.team.teamAttack, wallSkillIndex5051Data.wallSkillIndex5051.wallSkillRecord);
				break;
			}
		}
	}

	public void TriggerAction5061()
	{
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		foreach (stWallSkillIndex5061 wallSkillIndex5061Data in wallSkillIndex5061DataList)
		{
			if (wallSkillIndex5061Data.triggerNumber > 0)
			{
				if (skillPoolMgr.m_pool.TryGetValue("prefab/combat/wall/wall_5061.prefab", out var value) && value.m_activeSkills.Count > 0)
				{
					break;
				}
				wallSkillIndex5061Data.triggerNumber--;
				wallSkillIndex5061Data.saveRecord[0] = wallSkillIndex5061Data.triggerNumber;
				defense.SetDamageImmunity((int)defense.damageImmunity + (int)wallSkillIndex5061Data.addImmunity);
				if ((float)wallSkillIndex5061Data.knockback > 0f)
				{
					zbAudioMgr.Instance.PlayAudio("audio/wall/hailang.ogg");
					zbWall5061 obj = (zbWall5061)zbSkillSystem.Instance.CreateSkill("prefab/combat/wall/wall_5061.prefab", defense, Vector2.op_Implicit(new Vector2(nodeRootPositonX, nodeRootPositonY)), Vector2.up);
					obj.wallSkillIndex5061Data = wallSkillIndex5061Data;
					obj.SetWallAtr((float)wallSkillIndex5061Data.damagePrecent * (float)(int)wallSkillIndex5061Data.wallSkillIndex5061.team.teamAttack, wallSkillIndex5061Data.wallSkillIndex5061.wallSkillRecord);
					break;
				}
			}
		}
	}

	public void TriggerAction5071()
	{
		foreach (stWallSkillIndex5071 wallSkillIndex5071Data in wallSkillIndex5071DataList)
		{
			if (Time.time - wallSkillIndex5071Data.lastCallTime <= (float)(int)wallSkillIndex5071Data.subSkillNumber * (float)wallSkillIndex5071Data.fireInterval)
			{
				break;
			}
			if (wallSkillIndex5071Data.triggerNumber > 0)
			{
				wallSkillIndex5071Data.triggerNumber--;
				wallSkillIndex5071Data.saveRecord[0] = wallSkillIndex5071Data.triggerNumber;
				((MonoBehaviour)defense).StartCoroutine(ActionEnter5071(wallSkillIndex5071Data));
				break;
			}
		}
	}

	public IEnumerator ActionEnter5071(stWallSkillIndex5071 data)
	{
		for (int i = 0; i < (int)data.subSkillNumber; i++)
		{
			data.lastCallTime = Time.time;
			zbAudioMgr.Instance.PlayAudio("audio/combat/jiguang.ogg");
			zbWall5071 obj = (zbWall5071)zbSkillSystem.Instance.CreateSkill("prefab/combat/wall/wall_5071.prefab", defense, new Vector3(nodeRootPositonX, defense.GetAlertPositionY() + nodeRootPositonY, 0f), Vector2.up);
			obj.wallSkillIndex5071Data = data;
			obj.SetWallAtr((float)data.damagePrecent * (float)(int)data.wallSkillIndex5071.team.teamAttack, data.wallSkillIndex5071.wallSkillRecord);
			yield return (object)new WaitForSeconds((float)data.fireInterval);
		}
	}

	public void OnZuobiConfirm()
	{
		zbUIMgr.Instance.DestroyWnd(EWndId.Combat);
		zbUIMgr.Instance.ShowMainLevel();
		zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10667"), zbUtil.Language("lang.10668") + "_5", null);
		zbCombatSystem.Instance.combatRecorder.ClearRecord();
	}

	public zbSpineGpuAnimRenderer EnsureGpuAnimRenderer()
	{
		if (spineGpuAnimRenderer != null && spineGpuAnimRenderer.isValid)
		{
			return spineGpuAnimRenderer;
		}
		spineGpuAnimRenderer = new zbSpineGpuAnimRenderer(nodeRoot, spineCullCamera, monsters, this, $"zbSpineGpuAnimRenderer_Battle{dataIndex}");
		zbPerfLog.Log($"[zbSpineGpuAnim] 战场{dataIndex} GPU 动画渲染器已创建（全场 mega 单批 + 全局排序 + 旧路径边界对齐）");
		return spineGpuAnimRenderer;
	}

	public zbSpineBatchRenderer EnsureSpineBatchRenderer(int layer)
	{
		if (spineBatchRenderer != null && spineBatchRenderer.isValid)
		{
			return spineBatchRenderer;
		}
		Material activeMergedMaterial = zbCombatAtlasMerger.Instance.GetActiveMergedMaterial();
		if ((Object)(object)activeMergedMaterial == (Object)null)
		{
			return null;
		}
		spineBatchRenderer = new zbSpineBatchRenderer(nodeRoot, activeMergedMaterial, $"zbSpineBatchRenderer_Battle{dataIndex}", layer);
		zbPerfLog.Log($"[zbSpineBatchRenderer] 战场{dataIndex} 合并渲染器已创建（怪物 spine 接入后收敛为 1 draw/合并组）");
		return spineBatchRenderer;
	}

	public void Init(Transform _nodeRoot, BattleSideTeamInfo _battleInfo, int _dataIndex)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		nodeRoot = _nodeRoot;
		dataIndex = _dataIndex;
		battleSideTeamInfo = _battleInfo;
		nodeRootHero = nodeRoot.Find("RootHero");
		nodeRootHeroSkill = nodeRoot.Find("RootHeroSkill");
		nodeRootDefense = nodeRoot.Find("RootDefense");
		nodeRootDefenseHp = nodeRoot.Find("RootDefenseHp");
		nodeRootBuffEffect = nodeRoot.Find("RootBuffEffect");
		nodeRootMonsterSkillEffect = nodeRoot.Find("RootMonsterSkillEffect");
		nodeRootPositonX = nodeRoot.position.x;
		nodeRootPositonY = nodeRoot.position.y;
		mainCamera = Camera.main;
		Transform obj = nodeRoot.Find("GOCamera");
		Transform val = ((obj != null) ? obj.Find("Camera") : null);
		Camera val2 = (((Object)(object)val != (Object)null) ? ((Component)val).GetComponent<Camera>() : null);
		spineCullCamera = ((!zbCombatSystem.Instance.isPVPBattleType) ? (((Object)(object)mainCamera != (Object)null) ? mainCamera : val2) : (((Object)(object)val2 != (Object)null) ? val2 : mainCamera));
		m_spineWindowRefreshTimer = 0f;
		RefreshSpineCullWindow();
		zbPerfLog.Log(string.Format("[spineLOD] 战场{0} 分级窗口：相机={1} ", dataIndex, ((Object)(object)spineCullCamera != (Object)null) ? ((Object)spineCullCamera).name : "<null>") + $"Y∈[{spineCullMinY:0.0},{spineCullMaxY:0.0}] 远怪分界Y={spineFarBandY:0.0}");
		combatScene = new zbCombatScene();
		combatExp = new zbCombatExp();
		combatRoute = new zbCombatRoute();
		combatBubble = new zbCombatBubble();
		combatHud = new zbMonsterHud();
		monsters = new zbCombatMonster();
		skillPoolMgr = new zbSkillPoolMgr();
		monsterPoolMgr = new zbMonsterPoolMgr();
		monsterDieFxPool = new zbMonsterDieFxPool();
		monsterBulletEffectPoolMgr = new zbMonsterBulletEffectPoolMgr();
		monsterHitEffectPoolMgr = new zbMonsterHitEffectPoolMgr();
		buffEffectPoolMgr = new zbBuffEffectPoolMgr();
		monsterEyeEffectPoolMgr = new zbMonsterEyeEffectPoolMgr();
		monsterDieEffectPoolMgr = new zbMonsterDieEffectPoolMgr();
		monsterEffectPoolMgr = new zbMonsterEffectPoolMgr();
		monsterOnceEffectPoolMgr = new zbMonsterOnceEffectPoolMgr();
		mechaRegister = new zbMechaRegister();
		petRegister = new zbPetRegister();
		skillEffectPoolMgr = new zbSkillEffectPoolMgr();
		footholdRegister = new zbFootholdRegister();
		mechaRegister.Init();
		petRegister.Init();
		combatScene.Init(this);
		combatRoute.Init(this);
		combatExp.Init(this);
		combatBubble.Init(this);
		combatHud.Init(this);
		monsters.Init(this);
		footholdRegister.Init();
		skillPoolMgr.Init(nodeRoot);
		monsterPoolMgr.Init(nodeRoot);
		monsterDieFxPool.Init(nodeRoot);
		monsterBulletEffectPoolMgr.Init(nodeRoot);
		monsterHitEffectPoolMgr.Init(nodeRoot);
		buffEffectPoolMgr.Init(nodeRoot);
		monsterEyeEffectPoolMgr.Init(nodeRoot);
		monsterDieEffectPoolMgr.Init(nodeRoot);
		monsterEffectPoolMgr.Init(nodeRoot);
		monsterOnceEffectPoolMgr.Init(nodeRoot);
		skillEffectPoolMgr.Init(nodeRoot);
		masterTeam = new zbCombatTeam();
		masterTeam.Init(this, battleSideTeamInfo.MasterTeamInfo, 1, battleSideTeamInfo.HelperTeamInfo.SkillList.Count > 0);
		if (battleSideTeamInfo.HelperTeamInfo.SkillList.Count > 0)
		{
			helperTeam = new zbCombatTeam();
			helperTeam.Init(this, battleSideTeamInfo.HelperTeamInfo, 2, _haveHelp: true);
		}
		InitDefense();
		InitDefenseHp();
		InitMonsterSpawner();
		InitAlertRangeGizmo();
		sceneAlertRange = (float)(zbCombatSystem.Instance.battleInfo.BattleBaseInfo.VisionRange + defense.visionRange) / 100f;
		defenseMgr = new zbWallSkillMgr();
		defenseMgr.Init(this);
		foreach (int wallSkill in battleSideTeamInfo.MasterTeamInfo.DefenseInfo.WallSkillList)
		{
			defenseMgr.CreateDefense(wallSkill, masterTeam, isP1: true);
		}
		foreach (int wallSkill2 in battleSideTeamInfo.HelperTeamInfo.DefenseInfo.WallSkillList)
		{
			defenseMgr.CreateDefense(wallSkill2, helperTeam, isP1: false);
		}
		foreach (int item in new List<int>())
		{
			defenseMgr.CreateDefense(item, masterTeam, isP1: true);
		}
		foreach (int item2 in new List<int>())
		{
			defenseMgr.CreateDefense(item2, helperTeam, isP1: false);
		}
		combatScene.RefreshSceneAlertRange(sceneAlertRange);
		SetMaxBubbleActive(zbSelf.Instance.combatHandler.GetMaxBubbleNumber());
		m_Chuchang = ((Component)nodeRoot.Find("ChuChang")).gameObject;
		m_ChuchangAnim = ((Component)m_Chuchang.transform.Find("Chuchang_spine")).gameObject.GetComponent<SkeletonAnimation>();
		m_Chuchang.SetActive(false);
	}

	public void Free()
	{
		masterTeam.Free();
		helperTeam?.Free();
		monsters.Free();
		combatRoute.Free();
		combatBubble.Free();
		combatHud.Free();
		defenseMgr.Free();
		combatScene?.Free();
		skillPoolMgr.Free();
		monsterPoolMgr.Free();
		monsterDieFxPool.Free();
		monsterBulletEffectPoolMgr.Free();
		monsterHitEffectPoolMgr.Free();
		buffEffectPoolMgr.Free();
		monsterEyeEffectPoolMgr.Free();
		monsterDieEffectPoolMgr.Free();
		monsterEffectPoolMgr.Free();
		monsterOnceEffectPoolMgr.Free();
		skillEffectPoolMgr.Free();
		spineBatchRenderer?.Free();
		spineBatchRenderer = null;
		spineGpuAnimRenderer?.Free();
		spineGpuAnimRenderer = null;
	}

	public void Update()
	{
		combatExp.Update();
		combatBubble.Update();
		combatHud.Update();
		monsters.Update();
		defenseMgr.Update();
		monsterPoolMgr.UpdateExpand();
		m_spineWindowRefreshTimer += Time.deltaTime;
		if (m_spineWindowRefreshTimer >= 2f)
		{
			m_spineWindowRefreshTimer = 0f;
			RefreshSpineCullWindow();
		}
	}

	public void RefreshSpineCullWindow()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)spineCullCamera == (Object)null))
		{
			float y = ((Component)spineCullCamera).transform.position.y;
			float num = spineCullCamera.orthographicSize + 1f;
			spineCullMinY = y - num;
			spineCullMaxY = y + num;
			spineFarBandY = y;
		}
	}

	public string GetSpineTierSnapshot()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		Dictionary<int, zbMonster> monsterList = monsters.monsterList;
		foreach (KeyValuePair<int, zbMonster> item in monsterList)
		{
			zbMonster value = item.Value;
			if (!((Object)(object)value == (Object)null) && !value.isUsingSprite)
			{
				switch (value.GetCurrentSpineTier())
				{
				case zbMonster.ESpineUpdateTier.Culled:
					num++;
					break;
				case zbMonster.ESpineUpdateTier.Stepped:
					num2++;
					break;
				default:
					num3++;
					break;
				}
			}
		}
		return string.Format("战场{0} spine分级 停推={1} 隔帧={2} 全速={3} 活跃怪={4}", new object[5] { dataIndex, num, num2, num3, monsterList.Count });
	}

	public void Restore(zbCombatRecorder restoreData)
	{
		masterTeam.Restore(restoreData);
		helperTeam?.Restore(restoreData);
		defense.Restore(restoreData);
		combatExp.Restore(restoreData);
		defenseMgr.Restore(restoreData);
	}

	public void UnRestoreData()
	{
		masterTeam.UnRestoreData();
		helperTeam?.UnRestoreData();
		TriggerAction5031();
		TriggerAction5041();
	}

	private void InitDefense()
	{
		defense = ((Component)nodeRootDefense).GetComponent<zbCombatDefense>();
		defense.InitData(this, battleSideTeamInfo);
	}

	private void InitDefenseHp()
	{
		defenseHp = ((Component)nodeRootDefenseHp).GetComponent<zbCombatDefenseHp>();
		defenseHp.InitData(this);
	}

	private void InitMonsterSpawner()
	{
		monsterSpawner = ((Component)nodeRoot.Find("RootMonster").Find("GameObjectMonsterSpawn")).GetComponent<zbMonsterSpawner>();
		monsterSpawner.InitData(this);
	}

	private void InitAlertRangeGizmo()
	{
		alertRangeGizmo = ((Component)nodeRoot.Find("AlertRange")).GetComponent<zbAlertRangeGizmo>();
	}

	public float GetSceneAlertRange()
	{
		return sceneAlertRange;
	}

	public int GetMaxBubbleActive()
	{
		if (zbCombatSystem.Instance.combatFPS.IsSpriteReplaceMode())
		{
			return 50;
		}
		if (zbCombatSystem.Instance.combatFPS.IsVeryLowPerfomanceMode())
		{
			return 30;
		}
		return curBubbleActive;
	}

	public void SetMaxBubbleActive(float maxNumberPre)
	{
		curBubbleActive = (int)Math.Clamp(maxNumberPre * (float)(maxBubbleActive - 50) + 50f, 50f, maxBubbleActive);
	}

	public void ActionChuchangAnim(float alertRange)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		zbAudioMgr.Instance.PlayAudio("audio/ui/chuchang.mp3");
		m_ChuchangAnim.AnimationState.SetAnimation(0, "animation", false);
		m_Chuchang.SetActive(true);
		((Component)m_ChuchangAnim).transform.position = new Vector3(nodeRootPositonX, alertRange - 6f, 0f);
	}
}
