using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbGeneHandler
{
	public int[] geneUnlockLevels;

	public int geneOpenCost;

	public int geneRefreshCost;

	public int ackNeedAnimIndex = -1;

	public float[] geneShapeResonance = new float[5] { 0.1f, 0.2f, 0.3f, 0.4f, 1f };

	public int composeIndex = -1;

	public int composeAckGeneTid;

	public float[] autoIndexs = new float[3] { 0.2f, 0.3f, 1f };

	public int selAutoIndex = -1;

	private readonly Dictionary<int, GeneSlotDetail> _geneDetailCache = new Dictionary<int, GeneSlotDetail>();

	private int _lastCacheFrame = -1;

	private readonly Dictionary<int, long> _bagGeneCountCache = new Dictionary<int, long>();

	private int _lastBagCountFrame = -1;

	public void Init()
	{
		JObject record = zbDataConfig.Instance.resourceBaseJsonTable.GetRecord();
		geneUnlockLevels = record["geneOpenHole"][(object)"unlockLevel"].ToObject<int[]>();
		geneOpenCost = Extensions.Value<int>((IEnumerable<JToken>)record["geneOpenHole"][(object)"cost"]);
		geneRefreshCost = Extensions.Value<int>((IEnumerable<JToken>)record["geneRefreshHole"][(object)"cost"]);
	}

	public GeneSlotDetail GetGeneSlotDetail(ESkillType skillType, int index)
	{
		int frameCount = Time.frameCount;
		if (_lastCacheFrame != frameCount)
		{
			_geneDetailCache.Clear();
			_lastCacheFrame = frameCount;
		}
		int key = (int)skillType * 1000 + index;
		if (_geneDetailCache.TryGetValue(key, out var value))
		{
			return value;
		}
		RepeatedField<GeneSlotDetail> geneSlotListBySkillTypeToList = GetGeneSlotListBySkillTypeToList(skillType);
		if (geneSlotListBySkillTypeToList != null)
		{
			for (int i = 0; i < geneSlotListBySkillTypeToList.Count; i++)
			{
				GeneSlotDetail geneSlotDetail = geneSlotListBySkillTypeToList[i];
				if (geneSlotDetail != null && geneSlotDetail.Index == index)
				{
					_geneDetailCache[key] = geneSlotDetail;
					return geneSlotDetail;
				}
			}
		}
		_geneDetailCache[key] = null;
		return null;
	}

	public List<ResourceDetail> GetGenesByShape(EGeneShape geneShape)
	{
		List<ResourceDetail> list = new List<ResourceDetail>();
		foreach (ResourceDetail gene in zbServerData.sGameData.GeneList)
		{
			if (getBagGeneCount(gene.Tid) > 0)
			{
				zbGeneCsvRecord recordByKey = zbDataConfig.Instance.geneCsvTable.GetRecordByKey(gene.Tid);
				if (recordByKey != null && recordByKey.geneShape == (int)geneShape)
				{
					list.Add(gene);
				}
			}
		}
		return list;
	}

	public long getBagGeneCount(int geneTid)
	{
		int frameCount = Time.frameCount;
		if (_lastBagCountFrame != frameCount)
		{
			_bagGeneCountCache.Clear();
			_lastBagCountFrame = frameCount;
		}
		if (_bagGeneCountCache.TryGetValue(geneTid, out var value))
		{
			return value;
		}
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Gene, geneTid);
		int num = 0;
		RepeatedField<HeroAwakenDetail> awakenList = zbServerData.sGameData.HeroAwakenInfo.AwakenList;
		for (int i = 0; i < awakenList.Count; i++)
		{
			HeroAwakenDetail heroAwakenDetail = awakenList[i];
			if (heroAwakenDetail?.GeneSlotList == null)
			{
				continue;
			}
			RepeatedField<GeneSlotDetail> geneSlotList = heroAwakenDetail.GeneSlotList;
			for (int j = 0; j < geneSlotList.Count; j++)
			{
				if (geneSlotList[j].GeneTid == geneTid)
				{
					num++;
				}
			}
		}
		long num2 = resourceCount - num;
		_bagGeneCountCache[geneTid] = num2;
		return num2;
	}

	public void CheckNewOpenGeneSlot(HeroAwakenInfo oldInfo, HeroAwakenInfo newInfo, ESkillType skillType)
	{
		zbSelf.Instance.geneHandler.ackNeedAnimIndex = -1;
		if (newInfo == null)
		{
			return;
		}
		HeroAwakenDetail heroAwakenDetail = ((IEnumerable<HeroAwakenDetail>)newInfo.AwakenList).FirstOrDefault((HeroAwakenDetail v) => v.SkillType == skillType);
		if (heroAwakenDetail == null || heroAwakenDetail.GeneSlotList == null)
		{
			return;
		}
		HeroAwakenDetail heroAwakenDetail2 = null;
		if (oldInfo != null)
		{
			heroAwakenDetail2 = ((IEnumerable<HeroAwakenDetail>)oldInfo.AwakenList).FirstOrDefault((HeroAwakenDetail v) => v.SkillType == skillType);
		}
		for (int num = 0; num < heroAwakenDetail.GeneSlotList.Count; num++)
		{
			GeneSlotDetail geneSlotDetail = heroAwakenDetail.GeneSlotList[num];
			if (geneSlotDetail.GeneShape != EGeneShape.EmptyValue)
			{
				bool flag = true;
				if (heroAwakenDetail2 != null && heroAwakenDetail2.GeneSlotList != null && num < heroAwakenDetail2.GeneSlotList.Count)
				{
					flag = heroAwakenDetail2.GeneSlotList[num].GeneShape == EGeneShape.EmptyValue;
				}
				if (flag)
				{
					zbSelf.Instance.geneHandler.ackNeedAnimIndex = geneSlotDetail.Index;
					break;
				}
			}
		}
	}

	public GeneSlotDetail[] GetGeneSlotListBySkillType(ESkillType skillType)
	{
		HeroAwakenDetail heroAwakenDetail = zbSelf.Instance.heroHandler.GetHeroAwakenDetail(skillType);
		if (heroAwakenDetail != null)
		{
			return ((IEnumerable<GeneSlotDetail>)heroAwakenDetail.GeneSlotList).ToArray();
		}
		return Array.Empty<GeneSlotDetail>();
	}

	public RepeatedField<GeneSlotDetail> GetGeneSlotListBySkillTypeToList(ESkillType skillType)
	{
		return zbSelf.Instance.heroHandler.GetHeroAwakenDetail(skillType)?.GeneSlotList;
	}

	public int GetGeneSlotListOpenCount(ESkillType skillType)
	{
		int num = 0;
		RepeatedField<GeneSlotDetail> geneSlotListBySkillTypeToList = GetGeneSlotListBySkillTypeToList(skillType);
		if (geneSlotListBySkillTypeToList == null)
		{
			return num;
		}
		for (int i = 0; i < geneSlotListBySkillTypeToList.Count; i++)
		{
			if (geneSlotListBySkillTypeToList[i].GeneShape != EGeneShape.EmptyValue)
			{
				num++;
			}
		}
		return num;
	}

	public bool IsAllSlotSameShape(ESkillType skillType)
	{
		RepeatedField<GeneSlotDetail> geneSlotListBySkillTypeToList = GetGeneSlotListBySkillTypeToList(skillType);
		if (geneSlotListBySkillTypeToList == null || geneSlotListBySkillTypeToList.Count != 5)
		{
			return false;
		}
		EGeneShape geneShape = geneSlotListBySkillTypeToList[0].GeneShape;
		if (geneShape == EGeneShape.EmptyValue)
		{
			return false;
		}
		for (int i = 1; i < 5; i++)
		{
			if (geneSlotListBySkillTypeToList[i].GeneShape != geneShape)
			{
				return false;
			}
		}
		return true;
	}

	public GeneResonanceResult GetGeneResonanceResult(ESkillType skillType)
	{
		GeneResonanceResult result = new GeneResonanceResult
		{
			ResonanceEdges = new List<int>(),
			OpenResonanceEdges = new List<int>(),
			FilledResonanceSlots = new List<int>(),
			EmptyResonanceSlots = new List<int>(),
			MaxCount = 0,
			MaxPotentialCount = 0
		};
		RepeatedField<GeneSlotDetail> geneSlotListBySkillTypeToList = GetGeneSlotListBySkillTypeToList(skillType);
		if (geneSlotListBySkillTypeToList == null || geneSlotListBySkillTypeToList.Count != 5)
		{
			return result;
		}
		HashSet<int> hashSet = new HashSet<int>();
		HashSet<int> hashSet2 = new HashSet<int>();
		for (int i = 0; i < 5; i++)
		{
			int num = (i + 1) % 5;
			GeneSlotDetail geneSlotDetail = geneSlotListBySkillTypeToList[i];
			GeneSlotDetail geneSlotDetail2 = geneSlotListBySkillTypeToList[num];
			if (geneSlotDetail.GeneShape != EGeneShape.EmptyValue && geneSlotDetail.GeneShape == geneSlotDetail2.GeneShape)
			{
				bool num2 = geneSlotDetail.GeneTid > 0;
				bool flag = geneSlotDetail2.GeneTid > 0;
				if (num2 & flag)
				{
					result.ResonanceEdges.Add(i);
				}
				else
				{
					result.OpenResonanceEdges.Add(i);
				}
				if (num2)
				{
					hashSet.Add(i);
				}
				else
				{
					hashSet2.Add(i);
				}
				if (flag)
				{
					hashSet.Add(num);
				}
				else
				{
					hashSet2.Add(num);
				}
			}
		}
		result.MaxCount = result.ResonanceEdges.Count;
		result.MaxPotentialCount = result.ResonanceEdges.Count + result.OpenResonanceEdges.Count;
		result.FilledResonanceSlots = hashSet.ToList();
		result.EmptyResonanceSlots = hashSet2.ToList();
		return result;
	}

	public float GetGeneResonanceAttr(ESkillType skillType, float attrValue)
	{
		if (skillType != ESkillType.EmptyValue)
		{
			GeneResonanceResult geneResonanceResult = GetGeneResonanceResult(skillType);
			if (geneResonanceResult.MaxCount > 0)
			{
				return (1f + geneShapeResonance[geneResonanceResult.MaxCount - 1]) * attrValue;
			}
		}
		return attrValue;
	}

	public List<Dictionary<string, object>> GetGenesInfoByType(ESkillType skillType)
	{
		Dictionary<string, float> dictionary = new Dictionary<string, float>();
		Dictionary<string, zbTeamAttrCsvRecord> dictionary2 = new Dictionary<string, zbTeamAttrCsvRecord>();
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		RepeatedField<GeneSlotDetail> geneSlotListBySkillTypeToList = zbSelf.Instance.geneHandler.GetGeneSlotListBySkillTypeToList(skillType);
		if (geneSlotListBySkillTypeToList == null)
		{
			return list;
		}
		foreach (GeneSlotDetail item in geneSlotListBySkillTypeToList)
		{
			if (item.GeneShape != EGeneShape.EmptyValue && item.GeneTid > 0)
			{
				zbGeneCsvRecord recordByKey = zbDataConfig.Instance.geneCsvTable.GetRecordByKey(item.GeneTid);
				zbTeamAttrCsvRecord recordByKey2 = zbDataConfig.Instance.teamAttrCsvTable.GetRecordByKey((int)recordByKey.attr[0]);
				string name = recordByKey2.name;
				float num = recordByKey.attr[1];
				if (dictionary.ContainsKey(name))
				{
					dictionary[name] += num;
					continue;
				}
				dictionary[name] = num;
				dictionary2[name] = recordByKey2;
			}
		}
		foreach (KeyValuePair<string, float> item2 in dictionary)
		{
			string key = item2.Key;
			float value = item2.Value;
			zbTeamAttrCsvRecord zbTeamAttrCsvRecord2 = dictionary2[key];
			Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
			dictionary3["attr"] = key;
			dictionary3["showPercent"] = zbTeamAttrCsvRecord2.showPercent;
			dictionary3["val"] = GetGeneResonanceAttr(skillType, value);
			list.Add(dictionary3);
		}
		return list;
	}

	public List<ResourceDetail> GetGenesByTid(int geneTid)
	{
		zbGeneCsvRecord recordByKey = zbDataConfig.Instance.geneCsvTable.GetRecordByKey(geneTid);
		List<ResourceDetail> list = new List<ResourceDetail>();
		foreach (ResourceDetail gene in zbServerData.sGameData.GeneList)
		{
			long bagGeneCount = getBagGeneCount(gene.Tid);
			if (bagGeneCount > 0)
			{
				zbGeneCsvRecord recordByKey2 = zbDataConfig.Instance.geneCsvTable.GetRecordByKey(gene.Tid);
				if (recordByKey2 != null && recordByKey2.type == recordByKey.type && recordByKey2.geneShape == recordByKey.geneShape)
				{
					ResourceDetail item = new ResourceDetail
					{
						Tid = gene.Tid,
						Count = bagGeneCount
					};
					list.Add(item);
				}
			}
		}
		return (from g in list
			orderby zbDataConfig.Instance.geneCsvTable.GetRecordByKey(g.Tid)?.quality ?? 0 descending, g.Tid
			select g).ToList();
	}

	public bool HasGeneNotify(int type, int geneShape, int count = 1)
	{
		if (type >= 12)
		{
			return false;
		}
		int num = 0;
		foreach (ResourceDetail gene in zbServerData.sGameData.GeneList)
		{
			long bagGeneCount = getBagGeneCount(gene.Tid);
			if (bagGeneCount <= 0)
			{
				continue;
			}
			zbGeneCsvRecord recordByKey = zbDataConfig.Instance.geneCsvTable.GetRecordByKey(gene.Tid);
			if (recordByKey != null && type == recordByKey.type && geneShape == recordByKey.geneShape)
			{
				num += (int)bagGeneCount;
				if (num >= count)
				{
					return true;
				}
			}
		}
		count -= num;
		int num2 = (int)Mathf.Pow(5f, (float)(type - 1)) * count;
		num = 0;
		foreach (ResourceDetail gene2 in zbServerData.sGameData.GeneList)
		{
			long bagGeneCount2 = getBagGeneCount(gene2.Tid);
			if (bagGeneCount2 <= 0)
			{
				continue;
			}
			zbGeneCsvRecord recordByKey2 = zbDataConfig.Instance.geneCsvTable.GetRecordByKey(gene2.Tid);
			if (recordByKey2 != null && type > recordByKey2.type && geneShape == recordByKey2.geneShape)
			{
				num += (int)Mathf.Pow(5f, (float)(recordByKey2.type - 1)) * (int)bagGeneCount2;
				if (num >= num2)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasGeneUnlockNotify(ESkillType skillType, int index)
	{
		GeneSlotDetail geneSlotDetail = GetGeneSlotDetail(skillType, index);
		int num = geneUnlockLevels[index];
		if (geneSlotDetail == null && zbSelf.Instance.heroHandler.GetHeroAwakenLevel(skillType) >= num && zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Money, 19) > geneOpenCost)
		{
			return true;
		}
		return false;
	}

	public bool HasGeneSetNotify(ESkillType skillType, int index)
	{
		GeneSlotDetail geneSlotDetail = GetGeneSlotDetail(skillType, index);
		if (geneSlotDetail == null || geneSlotDetail.GeneShape == EGeneShape.EmptyValue || geneSlotDetail.GeneTid != 0)
		{
			return false;
		}
		foreach (ResourceDetail item in zbSelf.Instance.geneHandler.GetGenesByShape(geneSlotDetail.GeneShape))
		{
			if (zbDataConfig.Instance.geneCsvTable.GetRecordByKey(item.Tid).geneShape == (int)geneSlotDetail.GeneShape)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasGeneExchangeNotify(ESkillType skillType, int index)
	{
		GeneSlotDetail geneSlotDetail = GetGeneSlotDetail(skillType, index);
		if (geneSlotDetail == null || geneSlotDetail.GeneTid == 0)
		{
			return false;
		}
		zbGeneCsvRecord recordByKey = zbDataConfig.Instance.geneCsvTable.GetRecordByKey(geneSlotDetail.GeneTid);
		if (recordByKey.type >= 12)
		{
			return false;
		}
		foreach (ResourceDetail item in zbSelf.Instance.geneHandler.GetGenesByShape(geneSlotDetail.GeneShape))
		{
			if (zbDataConfig.Instance.geneCsvTable.GetRecordByKey(item.Tid).type > recordByKey.type)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasGeneUpgradeNotify(ESkillType skillType, int index)
	{
		GeneSlotDetail geneSlotDetail = GetGeneSlotDetail(skillType, index);
		if (geneSlotDetail != null && geneSlotDetail.GeneTid > 0)
		{
			zbGeneCsvRecord recordByKey = zbDataConfig.Instance.geneCsvTable.GetRecordByKey(geneSlotDetail.GeneTid);
			if (HasGeneNotify(recordByKey.type, recordByKey.geneShape, 4))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyGeneUnlockNotify(ESkillType skillType)
	{
		for (int i = 0; i < 5; i++)
		{
			if (HasGeneUnlockNotify(skillType, i))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyGeneSetNotify(ESkillType skillType)
	{
		for (int i = 0; i < 5; i++)
		{
			if (HasGeneSetNotify(skillType, i))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyGeneExchangeNotify(ESkillType skillType)
	{
		for (int i = 0; i < 5; i++)
		{
			if (HasGeneExchangeNotify(skillType, i))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyGeneUpgradeNotify(ESkillType skillType)
	{
		for (int i = 0; i < 5; i++)
		{
			if (HasGeneUpgradeNotify(skillType, i))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasHeroTabGeneNotify(ESkillType skillType, int heroTid = 0)
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.GeneLock))
		{
			return false;
		}
		if (heroTid != 0 && zbSelf.Instance.heroHandler.GetHeroDetail(heroTid) == null)
		{
			return false;
		}
		if (!HasAnyGeneUnlockNotify(skillType) && !HasAnyGeneSetNotify(skillType) && !HasAnyGeneUpgradeNotify(skillType))
		{
			return HasAnyGeneExchangeNotify(skillType);
		}
		return true;
	}
}
