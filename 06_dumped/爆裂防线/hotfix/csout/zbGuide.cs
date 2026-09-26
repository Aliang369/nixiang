using System;
using System.Collections.Generic;
using Proto.Enums;

public class zbGuide
{
	private static zbGuide sInstance;

	private Dictionary<EGuideStep, Func<zbGuideBase>> stepsCreator = new Dictionary<EGuideStep, Func<zbGuideBase>>();

	private zbGuideBase curGuideStep = new zbGuideBase();

	private string strGuideStep = "isFirstGuide_";

	public EGuideStep canGuideStep;

	private bool m_bOpenGuide = true;

	public static zbGuide Instance => sInstance;

	public static void CreateInstatnce()
	{
		sInstance = new zbGuide();
	}

	public void SetOpenGuide(bool value)
	{
		m_bOpenGuide = value;
	}

	public void CheckNewGuideStep()
	{
		canGuideStep = EGuideStep.guide_step_None;
		if (!m_bOpenGuide)
		{
			return;
		}
		if (zbSelf.Instance.arenaHandler.NeedGuidPvpMod())
		{
			canGuideStep = EGuideStep.guide_step_Arena_1;
		}
		else
		{
			if (zbSelf.Instance.arenaHandler.NeedCombatResume())
			{
				return;
			}
			if (!GetGuideBool(EGuideStep.guide_step_Idle) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.Idle))
			{
				canGuideStep = EGuideStep.guide_step_Idle;
				return;
			}
			if (!GetGuideBool(EGuideStep.guide_step_Home1) && !GetGuideBool(EGuideStep.guide_step_Home2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.Homeland))
			{
				canGuideStep = EGuideStep.guide_step_Home1;
				return;
			}
			if (!GetGuideBool(EGuideStep.guide_step_City) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.Explore))
			{
				canGuideStep = EGuideStep.guide_step_City;
				return;
			}
			if (zbServerData.sGameData.MainQuestInfo.MainLevel >= 4 && !GetGuideBool(EGuideStep.guide_step_Friend) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.Friends))
			{
				canGuideStep = EGuideStep.guide_step_Friend;
				return;
			}
			if (!GetGuideBool(EGuideStep.guide_step_Elite) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.EliteQuest))
			{
				canGuideStep = EGuideStep.guide_step_Elite;
				return;
			}
			if (!GetGuideBool(EGuideStep.guide_step_Legion) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.Legion))
			{
				if (!zbSelf.Instance.legionHandler.HasJoinLegion())
				{
					canGuideStep = EGuideStep.guide_step_Legion;
					return;
				}
				Instance.SetGuideBool(EGuideStep.guide_step_Legion);
			}
			if (!GetGuideBool(EGuideStep.guide_step_ElementTrial) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.ElementTrial))
			{
				canGuideStep = EGuideStep.guide_step_ElementTrial;
				return;
			}
			if (!GetGuideBool(EGuideStep.guide_step_WorldLair_1) && !GetGuideBool(EGuideStep.guide_step_WorldLair_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.WorldLair))
			{
				canGuideStep = EGuideStep.guide_step_WorldLair_1;
				return;
			}
			if (!GetGuideBool(EGuideStep.guide_step_HeroAwaken) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.HeroAwaken))
			{
				canGuideStep = EGuideStep.guide_step_HeroAwaken;
				return;
			}
			if (!GetGuideBool(EGuideStep.guide_step_LegionBoss_1) && !GetGuideBool(EGuideStep.guide_step_LegionBoss_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.LegionBoss))
			{
				if (!zbSelf.Instance.legionHandler.HasJoinLegion())
				{
					canGuideStep = EGuideStep.guide_step_LegionBoss_1;
					return;
				}
				Instance.SetGuideBool(EGuideStep.guide_step_LegionBoss_1);
			}
			if (!GetGuideBool(EGuideStep.guide_step_WorldBoss_1) && !GetGuideBool(EGuideStep.guide_step_WorldBoss_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.WorldBoss))
			{
				canGuideStep = EGuideStep.guide_step_WorldBoss_1;
			}
			else if (!GetGuideBool(EGuideStep.guide_step_Mecha) && zbSelf.Instance.GetMechaId() <= 0 && zbSelf.Instance.mechaHandler.HasRedPoint() && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.Mecha))
			{
				canGuideStep = EGuideStep.guide_step_Mecha;
			}
			else if (!GetGuideBool(EGuideStep.guide_step_EndlessHorde_1) && !GetGuideBool(EGuideStep.guide_step_EndlessHorde_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.EndlessHorde))
			{
				canGuideStep = EGuideStep.guide_step_EndlessHorde_1;
			}
			else if (!GetGuideBool(EGuideStep.guide_step_Legendary_1) && !GetGuideBool(EGuideStep.guide_step_Legendary_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.Legendary))
			{
				canGuideStep = EGuideStep.guide_step_Legendary_1;
			}
			else if (!GetGuideBool(EGuideStep.guide_step_Wall_1) && !GetGuideBool(EGuideStep.guide_step_Wall_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.WallDefense))
			{
				canGuideStep = EGuideStep.guide_step_Wall_1;
			}
			else if (!GetGuideBool(EGuideStep.guide_step_Gene) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.GeneLock))
			{
				canGuideStep = EGuideStep.guide_step_Gene;
			}
			else if (!GetGuideBool(EGuideStep.guide_step_CrossLadder_1) && !GetGuideBool(EGuideStep.guide_step_CrossLadder_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.CrossLadder))
			{
				canGuideStep = EGuideStep.guide_step_CrossLadder_1;
			}
			else if (!GetGuideBool(EGuideStep.guide_step_MechaSkill) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.MechaSkill))
			{
				canGuideStep = EGuideStep.guide_step_MechaSkill;
			}
			else if (!GetGuideBool(EGuideStep.guide_step_Treasure_1) && !GetGuideBool(EGuideStep.guide_step_Treasure_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.Treasure))
			{
				canGuideStep = EGuideStep.guide_step_Treasure_1;
			}
			else if (!GetGuideBool(EGuideStep.guide_step_MineWar_1) && !GetGuideBool(EGuideStep.guide_step_MineWar_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.MineWar))
			{
				canGuideStep = EGuideStep.guide_step_MineWar_1;
			}
		}
	}

	private void RegisterGuide(EGuideStep modIndex, Func<zbGuideBase> modConstructor)
	{
		if (!stepsCreator.ContainsKey(modIndex))
		{
			stepsCreator.Add(modIndex, modConstructor);
		}
	}

	public void ExecGuide(EGuideStep modIndex)
	{
		if (stepsCreator.ContainsKey(modIndex))
		{
			curGuideStep = stepsCreator[modIndex]();
			curGuideStep.Start();
		}
	}

	public bool IsGuiding()
	{
		if (curGuideStep != null)
		{
			return !curGuideStep.isOver();
		}
		return false;
	}

	public void StopCurGuideStep()
	{
		if (curGuideStep != null)
		{
			curGuideStep.Stop();
		}
		curGuideStep = null;
		zbUIMgr.Instance.HideWnd(EWndId.Guide);
		zbUIMgr.Instance.HideWnd(EWndId.GuideWeak);
		CheckNewGuideStep();
	}

	public bool GetGuideBool(EGuideStep stepIndex)
	{
		return zbGameSave.Instance.GetBool(strGuideStep + stepIndex.ToString() + zbSelf.Instance.GetRoleId());
	}

	public void SetGuideBool(EGuideStep stepIndex)
	{
		zbGameSave.Instance.SetBool(strGuideStep + stepIndex.ToString() + zbSelf.Instance.GetRoleId(), value: true);
	}

	public void Update()
	{
		if (curGuideStep != null)
		{
			curGuideStep.Update();
		}
	}

	public void Init()
	{
		RegisterGuide(EGuideStep.guide_step_Idle, () => new zbGuideIdle());
		RegisterGuide(EGuideStep.guide_step_Home1, () => new zbGuideHome1());
		RegisterGuide(EGuideStep.guide_step_Home2, () => new zbGuideHome2());
		RegisterGuide(EGuideStep.guide_step_City, () => new zbGuideWorld());
		RegisterGuide(EGuideStep.guide_step_Friend, () => new zbGuideFriend());
		RegisterGuide(EGuideStep.guide_step_Elite, () => new zbGuideElite());
		RegisterGuide(EGuideStep.guide_step_Legion, () => new zbGuideLegion());
		RegisterGuide(EGuideStep.guide_step_ElementTrial, () => new zbGuideElementTrial());
		RegisterGuide(EGuideStep.guide_step_WorldLair_1, () => new zbGuideWorldLair1());
		RegisterGuide(EGuideStep.guide_step_WorldLair_2, () => new zbGuideWorldLair2());
		RegisterGuide(EGuideStep.guide_step_HeroAwaken, () => new zbGuideHeroAwaken());
		RegisterGuide(EGuideStep.guide_step_LegionBoss_1, () => new zbGuideLegionBoss1());
		RegisterGuide(EGuideStep.guide_step_LegionBoss_2, () => new zbGuideLegionBoss2());
		RegisterGuide(EGuideStep.guide_step_WorldBoss_1, () => new zbGuideWorldBoss1());
		RegisterGuide(EGuideStep.guide_step_WorldBoss_2, () => new zbGuideWorldBoss2());
		RegisterGuide(EGuideStep.guide_step_Mecha, () => new zbGuideMecha());
		RegisterGuide(EGuideStep.guide_step_EndlessHorde_1, () => new zbGuideEndlessHorde1());
		RegisterGuide(EGuideStep.guide_step_EndlessHorde_2, () => new zbGuideEndlessHorde2());
		RegisterGuide(EGuideStep.guide_step_Legendary_1, () => new zbGuideLegendary1());
		RegisterGuide(EGuideStep.guide_step_Legendary_2, () => new zbGuideLegendary2());
		RegisterGuide(EGuideStep.guide_step_Wall_1, () => new zbGuideWall1());
		RegisterGuide(EGuideStep.guide_step_Wall_2, () => new zbGuideWall2());
		RegisterGuide(EGuideStep.guide_step_Arena_1, () => new zbGuideArena1());
		RegisterGuide(EGuideStep.guide_step_Arena_2, () => new zbGuideArena2());
		RegisterGuide(EGuideStep.guide_step_Gene, () => new zbGuideGene());
		RegisterGuide(EGuideStep.guide_step_CrossLadder_1, () => new zbGuideCrossLadder1());
		RegisterGuide(EGuideStep.guide_step_CrossLadder_2, () => new zbGuideCrossLadder2());
		RegisterGuide(EGuideStep.guide_step_MechaSkill, () => new zbGuideMechaSkill());
		RegisterGuide(EGuideStep.guide_step_Treasure_1, () => new zbGuideTreasure1());
		RegisterGuide(EGuideStep.guide_step_Treasure_2, () => new zbGuideTreasure2());
		RegisterGuide(EGuideStep.guide_step_MineWar_1, () => new zbGuideMineWar1());
		RegisterGuide(EGuideStep.guide_step_MineWar_2, () => new zbGuideMineWar2());
		RegisterGuide(EGuideStep.guide_step_MineWarCross, () => new zbGuideMineWarCross());
	}
}
