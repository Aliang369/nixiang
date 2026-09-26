using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;

public class zbNewTagHandler
{
	private int m_nLastRoleLevel;

	private int m_nLastMainQuestLevel;

	private List<int> m_newHeros = new List<int>();

	private List<int> m_newSuits = new List<int>();

	private List<int> m_newWalls = new List<int>();

	private List<int> m_newAvatars = new List<int>();

	private List<int> m_newMechaSkills = new List<int>();

	private List<int> m_notNotifyMechas = new List<int>();

	private List<int> m_notNotifyHolidayTasks = new List<int>();

	private List<int> m_notNotifyBarScores = new List<int>();

	private List<int> m_notNotifyHeroRaritys = new List<int>();

	private List<int> m_newJourneyMovies = new List<int>();

	private List<int> m_newEndlessMines = new List<int>();

	private List<int> m_newWarmUp = new List<int>();

	private List<int> m_newRelics = new List<int>();

	private List<int> m_newMechaRelics = new List<int>();

	private Dictionary<string, int> m_newGridIds = new Dictionary<string, int>();

	private Dictionary<string, int> m_newGems = new Dictionary<string, int>();

	private Dictionary<string, int> m_newEquips = new Dictionary<string, int>();

	private Dictionary<string, int> m_newLairs = new Dictionary<string, int>();

	private Dictionary<string, int> m_newFuncs = new Dictionary<string, int>();

	private Dictionary<string, int> m_restaurantBatchBuys = new Dictionary<string, int>();

	private const string strDungeonFirstEnter = "DungeonFirstEnter";

	private bool m_isDungeonFirstEnter = true;

	private const string First_UnLockLegion = "First_UnLockLegion";

	private bool m_isFirstUnLockLegion;

	private string strNewSuits = "newSuits";

	private string strNewWalls = "newWalls";

	private string strNewAvatars = "newAvatars";

	private string strNewMechaSkills = "newMechaSkills";

	private string strNotNotifyMechas = "notNotifyMechas";

	private string strNotNotifyHolidayTasks = "notNotifyHolidayTasks";

	private string strNotNotifyBarScores = "notNotifyBarScores";

	private string strNotNotifyHeroRaritys = "notNotifyHeroRaritys";

	private string strNewJourneyMovies = "newJourneyMovies";

	private string strNewEndlessMines = "newEndlessMines";

	private string strRestaurantBatchBuys = "restaurantBatchBuys";

	private string strNewWarmUp = "newWarmUp";

	private string strNewRelics = "newRelics";

	private string strNewMechaRelics = "newMechaRelics";

	private string RoleId => zbSelf.Instance.GetRoleId().ToString();

	public void Init()
	{
		m_nLastRoleLevel = zbServerData.sGameData.RoleBaseInfo.Level;
		m_nLastMainQuestLevel = zbServerData.sGameData.MainQuestInfo.MainLevel;
		m_newHeros = LoadIntList("newHeros");
		m_newSuits = LoadIntList(strNewSuits);
		m_newWalls = LoadIntList(strNewWalls);
		m_newAvatars = LoadIntList(strNewAvatars);
		m_newMechaSkills = LoadIntList(strNewMechaSkills);
		m_notNotifyMechas = LoadIntList(strNotNotifyMechas);
		m_notNotifyHolidayTasks = LoadIntList(strNotNotifyHolidayTasks);
		m_notNotifyBarScores = LoadIntList(strNotNotifyBarScores);
		m_newJourneyMovies = LoadIntList(strNewJourneyMovies);
		m_newEndlessMines = LoadIntList(strNewEndlessMines);
		m_newGridIds = LoadStringIntDict("newGridIds");
		m_newGems = LoadStringIntDict("newGems");
		m_newEquips = LoadStringIntDict("newEquips");
		m_newLairs = LoadStringIntDict("newLairs", "-");
		m_newFuncs = LoadStringIntDict("newFuncs", "-");
		m_restaurantBatchBuys = LoadStringIntDict(strRestaurantBatchBuys, "-");
		m_newWarmUp = LoadIntList(strNewWarmUp);
		strNewRelics += zbSelf.Instance.wallSubjectHandler.GetWallSubjectUUID();
		strNewRelics = strNewRelics.Replace(":", "-");
		m_newRelics = LoadIntList(strNewRelics);
		strNewMechaRelics += zbSelf.Instance.mechaSubjectHandler.GetMechaSubjectUUID();
		strNewMechaRelics = strNewMechaRelics.Replace(":", "-");
		m_newMechaRelics = LoadIntList(strNewMechaRelics);
		string dataFromFile = zbGameSave.Instance.GetDataFromFile("DungeonFirstEnter" + RoleId);
		if (!string.IsNullOrEmpty(dataFromFile) && bool.TryParse(dataFromFile, out var result))
		{
			m_isDungeonFirstEnter = result;
		}
		m_isFirstUnLockLegion = zbGameSave.Instance.GetBool("First_UnLockLegion" + RoleId);
	}

	public void Update()
	{
		UpdateLevelNewTag();
		UpdateMainQuestNewTag();
	}

	private void UpdateLevelNewTag()
	{
		int level = zbServerData.sGameData.RoleBaseInfo.Level;
		if (level > m_nLastRoleLevel)
		{
			m_nLastRoleLevel = level;
		}
	}

	private void UpdateMainQuestNewTag()
	{
		int mainLevel = zbServerData.sGameData.MainQuestInfo.MainLevel;
		if (mainLevel > m_nLastMainQuestLevel)
		{
			m_nLastMainQuestLevel = mainLevel;
		}
	}

	public bool isFirstUnLockLegion()
	{
		return m_isFirstUnLockLegion;
	}

	public void SetFirstUnlockLegion(bool isFirstUnLockLegion)
	{
		m_isFirstUnLockLegion = isFirstUnLockLegion;
		zbGameSave.Instance.SetBool("First_UnLockLegion" + RoleId, m_isFirstUnLockLegion);
	}

	public bool IsFirstEnterDungeon()
	{
		return m_isDungeonFirstEnter;
	}

	public void SetDungeonEntered()
	{
		m_isDungeonFirstEnter = false;
		zbGameSave.Instance.SaveDataToFile("DungeonFirstEnter" + RoleId, m_isDungeonFirstEnter.ToString());
	}

	public bool IsNewAvatar(int avatarId)
	{
		return m_newAvatars.Contains(avatarId);
	}

	public void AddNewAvatar(int avatarId)
	{
		AddToList(avatarId, m_newAvatars, strNewAvatars);
	}

	public void RemoveNewAvatar(int avatarId)
	{
		RemoveFromList(avatarId, m_newAvatars, strNewAvatars);
	}

	public void RemoveAllNewAvatars()
	{
		m_newAvatars.Clear();
		SaveIntList(strNewAvatars, m_newAvatars);
	}

	public bool IsNewMechaSkill(int mechaSkillId)
	{
		return m_newMechaSkills.Contains(mechaSkillId);
	}

	public void AddNewMechaSkill(int mechaSkillId)
	{
		AddToList(mechaSkillId, m_newMechaSkills, strNewMechaSkills);
	}

	public void RemoveNewMechaSkill(int mechaSkillId)
	{
		RemoveFromList(mechaSkillId, m_newMechaSkills, strNewMechaSkills);
	}

	public void RemoveAllNewMechaSkills()
	{
		m_newMechaSkills.Clear();
		SaveIntList(strNewMechaSkills, m_newMechaSkills);
	}

	public bool IsNotNotifyMecha(int mechaId)
	{
		return m_notNotifyMechas.Contains(mechaId);
	}

	public void AddNotNotifyMecha(int mechaId)
	{
		AddToList(mechaId, m_notNotifyMechas, strNotNotifyMechas);
	}

	public void RemoveNotNotifyMecha(int mechaId)
	{
		RemoveFromList(mechaId, m_notNotifyMechas, strNotNotifyMechas);
	}

	public void RemoveAllNotNotifyMechas()
	{
		m_notNotifyMechas.Clear();
		SaveIntList(strNotNotifyMechas, m_notNotifyMechas);
	}

	public bool IsNotNotifyHolidayTask(int index)
	{
		return m_notNotifyHolidayTasks.Contains(index);
	}

	public void AddNotNotifyHolidayTask(int index)
	{
		AddToList(index, m_notNotifyHolidayTasks, strNotNotifyHolidayTasks);
	}

	public void RemoveNotNotifyHolidayTask(int index)
	{
		RemoveFromList(index, m_notNotifyHolidayTasks, strNotNotifyHolidayTasks);
	}

	public void RemoveAllNotNotifyHolidayTasks()
	{
		m_notNotifyHolidayTasks.Clear();
		SaveIntList(strNotNotifyHolidayTasks, m_notNotifyHolidayTasks);
	}

	public bool IsNotNotifyBarScore(int index)
	{
		return m_notNotifyBarScores.Contains(index);
	}

	public void AddNotNotifyBarScore(int index)
	{
		AddToList(index, m_notNotifyBarScores, strNotNotifyBarScores);
	}

	public void RemoveNotNotifyBarScore(int index)
	{
		RemoveFromList(index, m_notNotifyBarScores, strNotNotifyBarScores);
	}

	public void RemoveAllNotNotifyBarScores()
	{
		m_notNotifyBarScores.Clear();
		SaveIntList(strNotNotifyBarScores, m_notNotifyBarScores);
	}

	public bool IsNotNotifyHeroRarity(int heroTid)
	{
		return m_notNotifyHeroRaritys.Contains(heroTid);
	}

	public void AddNotNotifyHeroRarity(int heroTid)
	{
		AddToList(heroTid, m_notNotifyHeroRaritys, strNotNotifyHeroRaritys);
	}

	public void RemoveNotNotifyHeroRarity(int heroTid)
	{
		RemoveFromList(heroTid, m_notNotifyHeroRaritys, strNotNotifyHeroRaritys);
	}

	public void RemoveAllNotNotifyHeroRaritys()
	{
		m_notNotifyHeroRaritys.Clear();
		SaveIntList(strNotNotifyHeroRaritys, m_notNotifyHeroRaritys);
	}

	public bool isNewLair(string uuid)
	{
		if (m_newLairs.TryGetValue(uuid, out var value))
		{
			return value == 0;
		}
		return false;
	}

	public void AddNewLair(string uuid)
	{
		if (!m_newLairs.ContainsKey(uuid))
		{
			m_newLairs[uuid] = 0;
			SaveStringIntDict("newLairs", m_newLairs, "-");
		}
	}

	public void SetNewLairViewed(string uuid)
	{
		if (m_newLairs.TryGetValue(uuid, out var value) && value == 0)
		{
			m_newLairs[uuid] = 1;
			SaveStringIntDict("newLairs", m_newLairs, "-");
		}
	}

	public void RemoveNewLair(string uuid)
	{
		if (m_newLairs.Remove(uuid))
		{
			SaveStringIntDict("newLairs", m_newLairs, "-");
		}
	}

	private string funcTypeToString(EFuncType funcType)
	{
		int num = (int)funcType;
		return num.ToString();
	}

	public bool isNewFunc(EFuncType funcType)
	{
		if (m_newFuncs.TryGetValue(funcTypeToString(funcType), out var value) && value == 0)
		{
			return !zbSelf.Instance.funHandler.IsLockNoTip(funcType);
		}
		return false;
	}

	public void AddNewFunc(string funcType)
	{
		if (!m_newFuncs.ContainsKey(funcType))
		{
			m_newFuncs[funcType] = 0;
			SaveStringIntDict("newFuncs", m_newFuncs, "-");
		}
	}

	public void SetNewFuncViewed(EFuncType funcType)
	{
		if (m_newFuncs.TryGetValue(funcTypeToString(funcType), out var value) && value == 0)
		{
			m_newFuncs[funcTypeToString(funcType)] = 1;
			SaveStringIntDict("newFuncs", m_newFuncs, "-");
		}
	}

	public void RemoveNewFunc(EFuncType funcType)
	{
		if (m_newFuncs.Remove(funcTypeToString(funcType)))
		{
			SaveStringIntDict("newFuncs", m_newFuncs, "-");
		}
	}

	public bool IsRestaurantBatchBuy(string uuid)
	{
		if (m_restaurantBatchBuys.Count != 0)
		{
			if (m_restaurantBatchBuys.TryGetValue(uuid, out var value))
			{
				return value == 0;
			}
			return false;
		}
		return true;
	}

	public void SetRestaurantBatchBuy(string uuid, int newState)
	{
		if (!m_restaurantBatchBuys.TryGetValue(uuid, out var value) || value != newState)
		{
			m_restaurantBatchBuys[uuid] = newState;
			SaveStringIntDict(strRestaurantBatchBuys, m_restaurantBatchBuys, "-");
		}
	}

	public bool IsNewHero(int tid)
	{
		return m_newHeros.Contains(tid);
	}

	public void AddNewHero(int tid)
	{
		AddToList(tid, m_newHeros, "newHeros");
	}

	public void RemoveNewHero(int tid)
	{
		RemoveFromList(tid, m_newHeros, "newHeros");
	}

	public bool IsNewSuit(int tid)
	{
		return m_newSuits.Contains(tid);
	}

	public void AddNewSuit(int tid)
	{
		AddToList(tid, m_newSuits, strNewSuits);
	}

	public void RemoveNewSuit(int tid)
	{
		RemoveFromList(tid, m_newSuits, strNewSuits);
	}

	public void RemoveAllNewSuits()
	{
		m_newSuits.Clear();
		SaveIntList(strNewSuits, m_newSuits);
	}

	public bool IsNewWarmUp(int id)
	{
		return m_newWarmUp.Contains(id);
	}

	public void AddNewWarmUp(int id)
	{
		AddToList(id, m_newWarmUp, strNewWarmUp);
	}

	public void RemoveNewWarmUp(int id)
	{
		RemoveFromList(id, m_newWarmUp, strNewWarmUp);
	}

	public bool IsNewRelic(int tid)
	{
		return m_newRelics.Contains(tid);
	}

	public void AddNewRelic(int tid)
	{
		AddToList(tid, m_newRelics, strNewRelics);
	}

	public void RemoveNewRelic(int tid)
	{
		RemoveFromList(tid, m_newRelics, strNewRelics);
	}

	public void RemoveAllNewRelics()
	{
		m_newRelics.Clear();
		SaveIntList(strNewRelics, m_newRelics);
	}

	public bool IsNewMechaRelic(int tid)
	{
		return m_newMechaRelics.Contains(tid);
	}

	public void AddNewMechaRelic(int tid)
	{
		AddToList(tid, m_newMechaRelics, strNewMechaRelics);
	}

	public void RemoveNewMechaRelic(int tid)
	{
		RemoveFromList(tid, m_newMechaRelics, strNewMechaRelics);
	}

	public void RemoveAllNewMechaRelics()
	{
		m_newMechaRelics.Clear();
		SaveIntList(strNewMechaRelics, m_newMechaRelics);
	}

	public bool IsJourneyMovieUnSeen(int type)
	{
		return !m_newJourneyMovies.Contains(type);
	}

	public void AddJourneyMovieSeen(int type)
	{
		AddToList(type, m_newJourneyMovies, strNewJourneyMovies);
	}

	public bool IsNewEndlessMine(int type)
	{
		return !m_newEndlessMines.Contains(type);
	}

	public void AddNewEndlessMine(int type)
	{
		AddToList(type, m_newEndlessMines, strNewEndlessMines);
	}

	public bool IsNewWall(int tid)
	{
		return m_newWalls.Contains(tid);
	}

	public void AddNewWall(int tid)
	{
		AddToList(tid, m_newWalls, strNewWalls);
	}

	public void RemoveNewWall(int tid)
	{
		RemoveFromList(tid, m_newWalls, strNewWalls);
	}

	public void RemoveAllNewWalls()
	{
		m_newWalls.Clear();
		SaveIntList(strNewWalls, m_newWalls);
	}

	public bool IsNewGridId(string gridId)
	{
		if (m_newGridIds.TryGetValue(gridId, out var value))
		{
			return value == 0;
		}
		return false;
	}

	public void AddNewGridId(string gridId)
	{
		if (!m_newGridIds.ContainsKey(gridId))
		{
			m_newGridIds[gridId] = 0;
			SaveStringIntDict("newGridIds", m_newGridIds);
		}
	}

	public void SetNewGridIdViewed(string gridId)
	{
		if (m_newGridIds.TryGetValue(gridId, out var value) && value == 0)
		{
			m_newGridIds[gridId] = 1;
			SaveStringIntDict("newGridIds", m_newGridIds);
		}
	}

	public void RemoveNewGridId(string gridId)
	{
		if (m_newGridIds.Remove(gridId))
		{
			SaveStringIntDict("newGridIds", m_newGridIds);
		}
	}

	public bool IsNewGem(string gemTid)
	{
		if (m_newGems.TryGetValue(gemTid, out var value))
		{
			return value == 0;
		}
		return false;
	}

	public void AddNewGem(string gemTid)
	{
		if (!m_newGems.ContainsKey(gemTid))
		{
			m_newGems[gemTid] = 0;
			SaveStringIntDict("newGems", m_newGems);
		}
	}

	public void SetNewGemViewed(string equipId)
	{
		if (m_newGems.TryGetValue(equipId, out var value) && value == 0)
		{
			m_newGems[equipId] = 1;
			SaveStringIntDict("newGems", m_newGems);
		}
	}

	public void RemoveNewGem(string equipId)
	{
		if (m_newGems.Remove(equipId))
		{
			SaveStringIntDict("newGems", m_newGems);
		}
	}

	public bool IsNewEquip(string equipId)
	{
		if (m_newEquips.TryGetValue(equipId, out var value))
		{
			return value == 0;
		}
		return false;
	}

	public void AddNewEquip(string equipId)
	{
		if (!m_newEquips.ContainsKey(equipId))
		{
			m_newEquips[equipId] = 0;
			SaveStringIntDict("newEquips", m_newEquips);
		}
	}

	public void SetNewEquipViewed(string equipId)
	{
		if (m_newEquips.TryGetValue(equipId, out var value) && value == 0)
		{
			m_newEquips[equipId] = 1;
			SaveStringIntDict("newEquips", m_newEquips);
		}
	}

	public void RemoveNewEquip(string equipId)
	{
		if (m_newEquips.Remove(equipId))
		{
			SaveStringIntDict("newEquips", m_newEquips);
		}
	}

	private void AddToList(int id, List<int> list, string key)
	{
		if (!list.Contains(id))
		{
			list.Add(id);
			SaveIntList(key, list);
		}
	}

	private void RemoveFromList(int id, List<int> list, string key)
	{
		if (list.Remove(id))
		{
			SaveIntList(key, list);
		}
	}

	private void SaveIntList(string key, List<int> list)
	{
		string jsonData = string.Join(",", list.Distinct());
		zbGameSave.Instance.SaveDataToFile(key + RoleId, jsonData);
	}

	private List<int> LoadIntList(string key)
	{
		return (from s in zbGameSave.Instance.GetDataFromFile(key + RoleId).Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries)
			select s.Trim() into s
			where int.TryParse(s, out var _)
			select s).Select(int.Parse).Distinct().ToList();
	}

	private void SaveStringIntDict(string key, Dictionary<string, int> dict, string splitSymbol = ":")
	{
		string jsonData = string.Join(",", dict.Select((KeyValuePair<string, int> kv) => $"{kv.Key}{splitSymbol}{kv.Value}"));
		zbGameSave.Instance.SaveDataToFile(key + RoleId, jsonData);
	}

	private Dictionary<string, int> LoadStringIntDict(string key, string splitSymbol = ":")
	{
		string dataFromFile = zbGameSave.Instance.GetDataFromFile(key + RoleId);
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		string[] array = dataFromFile.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(splitSymbol);
			if (array2.Length == 2 && int.TryParse(array2[1], out var result))
			{
				dictionary[array2[0]] = result;
			}
		}
		return dictionary;
	}
}
