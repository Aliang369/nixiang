using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine;

public class zbLegionWarFightMgr : MonoBehaviour
{
	public static zbLegionWarFightMgr Instance;

	public Transform heroTF;

	private int[] ints = new int[3];

	private HashSet<long> longs = new HashSet<long>();

	public List<zbLegionWarHero> heroList { get; private set; }

	public List<zbLegionWarBattleHero> heroBattleList { get; private set; }

	public Dictionary<long, Vector2> heroPositions { get; set; } = new Dictionary<long, Vector2>();

	private void Awake()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			Instance = this;
		}
		heroList = new List<zbLegionWarHero>();
		heroBattleList = new List<zbLegionWarBattleHero>();
	}

	public void InitHeroList()
	{
		foreach (zbLegionWarHero hero in heroList)
		{
			((Component)hero).gameObject.SetActive(false);
			hero.m_heroHud.SetHudVisible(visible: false);
		}
		foreach (zbLegionWarBattleHero heroBattle in heroBattleList)
		{
			((Component)heroBattle).gameObject.SetActive(false);
			heroBattle.m_heroLeftHud.SetHudVisible(visible: false);
			heroBattle.m_heroRightHud.SetHudVisible(visible: false);
		}
		longs.Clear();
		Array.Clear(ints, 0, ints.Length);
		int num = 0;
		foreach (LegionWarFightingDetail fighting in zbSelf.Instance.legionWarHandler.legionWarPointInfo.FightingList)
		{
			if (zbUtil.IsNpc(fighting.AttackerRoleId) || zbUtil.IsNpc(fighting.DefenderRoleId))
			{
				break;
			}
			LegionWarPointPlayerDetail legionWarPointPlayerDetail = null;
			LegionWarPointPlayerDetail legionWarPointPlayerDetail2 = null;
			foreach (LegionWarPointPlayerDetail player in zbSelf.Instance.legionWarHandler.legionWarPointInfo.PlayerList)
			{
				if (fighting.AttackerRoleId == player.RoleBaseInfo.RoleId)
				{
					legionWarPointPlayerDetail = player;
					longs.Add(player.RoleBaseInfo.RoleId);
				}
				if (fighting.DefenderRoleId == player.RoleBaseInfo.RoleId)
				{
					legionWarPointPlayerDetail2 = player;
					longs.Add(player.RoleBaseInfo.RoleId);
				}
			}
			if (num < 8 && legionWarPointPlayerDetail != null && legionWarPointPlayerDetail2 != null)
			{
				CreateBatterHero(legionWarPointPlayerDetail, legionWarPointPlayerDetail2, num);
				num++;
			}
		}
		int num2 = 0;
		foreach (LegionWarPointPlayerDetail player2 in zbSelf.Instance.legionWarHandler.legionWarPointInfo.PlayerList)
		{
			if (ints[(int)player2.RoleStateInfo.ForceId] <= 30 && !longs.Contains(player2.RoleBaseInfo.RoleId))
			{
				ints[(int)player2.RoleStateInfo.ForceId]++;
				CreateHero(player2, num2);
				num2++;
			}
		}
	}

	private void CreateHero(LegionWarPointPlayerDetail legionWarPointPlayerDetail, int index)
	{
		if (heroList.Count > index)
		{
			((Component)heroList[index]).gameObject.SetActive(true);
			heroList[index].Init(legionWarPointPlayerDetail, index);
			heroList[index].m_heroHud.SetHudVisible(visible: true);
		}
		else
		{
			GameObject obj = Object.Instantiate<GameObject>(zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/wnd/legionWar/legionWarHero.prefab"));
			obj.transform.SetParent(heroTF);
			zbLegionWarHero component = ((Component)obj.transform).GetComponent<zbLegionWarHero>();
			component.Init(legionWarPointPlayerDetail, index);
			heroList.Add(component);
		}
	}

	private void CreateBatterHero(LegionWarPointPlayerDetail playerAttackDetail, LegionWarPointPlayerDetail playerDefendDetail, int index)
	{
		if (heroBattleList.Count > index)
		{
			((Component)heroBattleList[index]).gameObject.SetActive(true);
			heroBattleList[index].Init(playerAttackDetail, playerDefendDetail, index);
			heroBattleList[index].m_heroLeftHud.SetHudVisible(visible: true);
			heroBattleList[index].m_heroRightHud.SetHudVisible(visible: true);
		}
		else
		{
			GameObject obj = Object.Instantiate<GameObject>(zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/wnd/legionWar/legionWarBattleHero.prefab"));
			obj.transform.SetParent(heroTF);
			zbLegionWarBattleHero component = ((Component)obj.transform).GetComponent<zbLegionWarBattleHero>();
			component.Init(playerAttackDetail, playerDefendDetail, index);
			heroBattleList.Add(component);
		}
	}

	public void Update()
	{
		foreach (zbLegionWarHero hero in heroList)
		{
			if (((Component)hero).gameObject.activeSelf)
			{
				hero.m_heroHud.Update();
			}
		}
	}
}
