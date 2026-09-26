using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using zbType;

public class zbDataHandler
{
	public HeroDetail[] lastHeroList;

	public void Init()
	{
	}

	public void DataChangeShowAward(AwardDetail[] awardList)
	{
		List<int> list = new List<int>();
		AwardDetail[] array = awardList;
		foreach (AwardDetail awardDetail in array)
		{
			if (awardDetail.Type == EResourceType.Hero)
			{
				list.Add((int)awardDetail.Tid);
			}
		}
		if (list.Count > 0)
		{
			zbUIMgr.Instance.ShowHeroGet(list.ToArray(), delegate(bool isOK)
			{
				if (isOK)
				{
					zbUIMgr.Instance.ShowAward(awardList);
				}
			});
		}
		else
		{
			zbUIMgr.Instance.ShowAward(awardList);
		}
	}

	public void DataChange(DataChange dataChange, AwardDetail[] awardList = null)
	{
		lastHeroList = ((IEnumerable<HeroDetail>)zbServerData.sGameData.HeroList).ToArray();
		if (dataChange == null)
		{
			return;
		}
		if (awardList != null && awardList.Length != 0)
		{
			DataChangeShowAward(awardList);
		}
		if (dataChange.RoleBaseInfo != null)
		{
			zbSelf.Instance.UpdateRoleBaseInfo(dataChange.RoleBaseInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_RoleBaseInfo);
		}
		if (dataChange.RoleCommonInfo != null)
		{
			if (zbServerData.sGameData.RoleCommonInfo.RechargeAmount <= 0f && dataChange.RoleCommonInfo.RechargeAmount > 0f && zbCombatSystem.Instance != null)
			{
				zbCombatSystem.Instance.combatSpeed.SetSpeed(ECombatSpeed.X1);
			}
			zbServerData.sGameData.RoleCommonInfo = dataChange.RoleCommonInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_RoleCommonInfo);
		}
		if (dataChange.RoleTeamInfo != null)
		{
			zbServerData.sGameData.RoleTeamInfo = dataChange.RoleTeamInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_RoleTeamInfo);
			zbSelf.Instance.attrHandler.DataChangeTeam();
		}
		if (dataChange.Money != null)
		{
			if (dataChange.Money.Update != null)
			{
				ResourcesUpdate(dataChange.Money.Update, zbServerData.sGameData.MoneyList);
			}
			if (dataChange.Money.Del != null)
			{
				ResourcesDelete(dataChange.Money.Del, zbServerData.sGameData.MoneyList);
			}
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Money);
		}
		if (dataChange.Item != null)
		{
			if (dataChange.Item.Update != null)
			{
				ResourcesUpdate(dataChange.Item.Update, zbServerData.sGameData.ItemList);
			}
			if (dataChange.Item.Del != null)
			{
				ResourcesDelete(dataChange.Item.Del, zbServerData.sGameData.ItemList);
			}
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Item);
		}
		if (dataChange.HeroFrag != null)
		{
			if (dataChange.HeroFrag.Update != null)
			{
				ResourcesUpdate(dataChange.HeroFrag.Update, zbServerData.sGameData.HeroFragList);
			}
			if (dataChange.HeroFrag.Del != null)
			{
				ResourcesDelete(dataChange.HeroFrag.Del, zbServerData.sGameData.HeroFragList);
			}
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_HeroFrag);
		}
		if (dataChange.Gem != null)
		{
			if (dataChange.Gem.Update != null)
			{
				ResourcesUpdate(dataChange.Gem.Update, zbServerData.sGameData.GemList, isGem: true);
			}
			if (dataChange.Gem.Del != null)
			{
				ResourcesDelete(dataChange.Gem.Del, zbServerData.sGameData.GemList);
			}
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Gem);
		}
		if (dataChange.Box != null)
		{
			if (dataChange.Box.Update != null)
			{
				ResourcesUpdate(dataChange.Box.Update, zbServerData.sGameData.BoxList);
			}
			if (dataChange.Box.Del != null)
			{
				ResourcesDelete(dataChange.Box.Del, zbServerData.sGameData.BoxList);
			}
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Box);
		}
		if (dataChange.Prop != null)
		{
			if (dataChange.Prop.Update != null)
			{
				PropUpdate(dataChange.Prop.Update, zbServerData.sGameData.PropList);
			}
			if (dataChange.Prop.Del != null)
			{
				PropDelete(dataChange.Prop.Del, zbServerData.sGameData.PropList);
			}
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Prop);
		}
		if (dataChange.Equip != null)
		{
			if (dataChange.Equip.Update != null)
			{
				EquipUpdate(dataChange.Equip.Update, zbServerData.sGameData.EquipList);
			}
			if (dataChange.Equip.Del != null && dataChange.Equip.Del.Count > 0)
			{
				EquipDelete(dataChange.Equip.Del, zbServerData.sGameData.EquipList);
			}
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Equip);
		}
		if (dataChange.Hero != null)
		{
			if (dataChange.Hero.Update != null)
			{
				HeroUpdate(dataChange.Hero.Update, zbServerData.sGameData.HeroList);
			}
			if (dataChange.Hero.Del != null)
			{
				HeroDelete(dataChange.Hero.Del, zbServerData.sGameData.HeroList);
			}
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Hero);
		}
		if (dataChange.Gene != null)
		{
			if (dataChange.Gene.Update != null)
			{
				ResourcesUpdate(dataChange.Gene.Update, zbServerData.sGameData.GeneList);
			}
			if (dataChange.Gene.Del != null)
			{
				ResourcesDelete(dataChange.Gene.Del, zbServerData.sGameData.GeneList);
			}
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Gene);
		}
	}

	private void ResourcesUpdate(RepeatedField<ResourceDetail> resourceDetails, RepeatedField<ResourceDetail> existList, bool isGem = false)
	{
		foreach (ResourceDetail resourceDetail in resourceDetails)
		{
			bool flag = false;
			foreach (ResourceDetail exist in existList)
			{
				if (exist.Tid == resourceDetail.Tid)
				{
					existList.Remove(exist);
					existList.Add(resourceDetail);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (isGem)
				{
					zbSelf.Instance.newTagHandler.AddNewGem(resourceDetail.Tid.ToString());
				}
				existList.Add(resourceDetail);
			}
		}
	}

	private void ResourcesDelete(RepeatedField<int> resourceDetails, RepeatedField<ResourceDetail> existList)
	{
		foreach (int resourceDetail in resourceDetails)
		{
			foreach (ResourceDetail exist in existList)
			{
				if (exist.Tid == resourceDetail)
				{
					existList.Remove(exist);
					break;
				}
			}
		}
	}

	private void PropUpdate(RepeatedField<PropDetail> resourceDetails, RepeatedField<PropDetail> existList)
	{
		foreach (PropDetail resourceDetail in resourceDetails)
		{
			bool flag = false;
			foreach (PropDetail exist in existList)
			{
				if (exist.Tid == resourceDetail.Tid)
				{
					existList.Remove(exist);
					existList.Add(resourceDetail);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				existList.Add(resourceDetail);
			}
		}
	}

	private void PropDelete(RepeatedField<int> resourceDetails, RepeatedField<PropDetail> existList)
	{
		foreach (int resourceDetail in resourceDetails)
		{
			foreach (PropDetail exist in existList)
			{
				if (exist.Tid == resourceDetail)
				{
					existList.Remove(exist);
					break;
				}
			}
		}
	}

	private void EquipUpdate(RepeatedField<EquipDetail> resourceDetails, RepeatedField<EquipDetail> existList)
	{
		foreach (EquipDetail resourceDetail in resourceDetails)
		{
			bool flag = false;
			foreach (EquipDetail exist in existList)
			{
				if (exist.EquipId == resourceDetail.EquipId)
				{
					existList.Remove(exist);
					existList.Add(resourceDetail);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				zbSelf.Instance.newTagHandler.AddNewEquip(resourceDetail.EquipId);
				existList.Add(resourceDetail);
			}
		}
	}

	private void EquipDelete(RepeatedField<string> resourceDetails, RepeatedField<EquipDetail> existList)
	{
		foreach (string resourceDetail in resourceDetails)
		{
			foreach (EquipDetail exist in existList)
			{
				if (exist.EquipId == resourceDetail)
				{
					existList.Remove(exist);
					break;
				}
			}
		}
	}

	private void HeroUpdate(RepeatedField<HeroDetail> resourceDetails, RepeatedField<HeroDetail> existList)
	{
		foreach (HeroDetail resourceDetail in resourceDetails)
		{
			bool flag = false;
			foreach (HeroDetail exist in existList)
			{
				if (exist.HeroId == resourceDetail.HeroId)
				{
					existList.Remove(exist);
					existList.Add(resourceDetail);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				zbSelf.Instance.newTagHandler.AddNewHero(resourceDetail.HeroTid);
				existList.Add(resourceDetail);
			}
		}
	}

	private void HeroDelete(RepeatedField<string> resourceDetails, RepeatedField<HeroDetail> existList)
	{
		foreach (string resourceDetail in resourceDetails)
		{
			foreach (HeroDetail exist in existList)
			{
				if (exist.HeroId == resourceDetail)
				{
					zbSelf.Instance.newTagHandler.RemoveNewHero(exist.HeroTid);
					existList.Remove(exist);
					break;
				}
			}
		}
	}

	public zbItemData GetCostListLackItem(int[][] costList)
	{
		foreach (int[] array in costList)
		{
			if (!HasResourceEnough((EResourceType)array[0], array[1], array[2]))
			{
				return new zbItemData((EResourceType)array[0], array[1], array[2]);
			}
		}
		return null;
	}

	public bool HasResourceEnough(EResourceType eResourceType, int id, int count)
	{
		return GetResourceCount(eResourceType, id) >= count;
	}

	public long GetResourceCount(EResourceType eResourceType, int id)
	{
		long result = 0L;
		switch (eResourceType)
		{
		case EResourceType.Money:
			result = GetCommonCount(zbServerData.sGameData.MoneyList, id);
			break;
		case EResourceType.Prop:
			result = GetPropCount(zbServerData.sGameData.PropList, id);
			break;
		case EResourceType.Item:
			result = GetCommonCount(zbServerData.sGameData.ItemList, id);
			break;
		case EResourceType.Equip:
			result = GetEquipCount(zbServerData.sGameData.EquipList, id);
			break;
		case EResourceType.Hero:
		case EResourceType.HeroFrag:
			result = GetCommonCount(zbServerData.sGameData.HeroFragList, id);
			break;
		case EResourceType.Gem:
			result = GetGemCount(zbServerData.sGameData.GemList, id);
			break;
		case EResourceType.Box:
			result = GetCommonCount(zbServerData.sGameData.BoxList, id);
			break;
		case EResourceType.Gene:
			result = GetCommonCount(zbServerData.sGameData.GeneList, id);
			break;
		}
		return result;
	}

	private long GetCommonCount(RepeatedField<ResourceDetail> resourceDetails, int id)
	{
		foreach (ResourceDetail resourceDetail in resourceDetails)
		{
			if (resourceDetail.Tid == id)
			{
				return resourceDetail.Count;
			}
		}
		return 0L;
	}

	private long GetGemCount(RepeatedField<ResourceDetail> resourceDetails, int id)
	{
		foreach (ResourceDetail resourceDetail in resourceDetails)
		{
			if (resourceDetail.Tid == id)
			{
				return resourceDetail.Count;
			}
		}
		return 0L;
	}

	private long GetPropCount(RepeatedField<PropDetail> resourceDetails, int id)
	{
		foreach (PropDetail resourceDetail in resourceDetails)
		{
			if (resourceDetail.Tid == id)
			{
				return resourceDetail.Count;
			}
		}
		return 0L;
	}

	private long GetEquipCount(RepeatedField<EquipDetail> resourceDetails, int id)
	{
		long num = 0L;
		foreach (EquipDetail resourceDetail in resourceDetails)
		{
			if (resourceDetail.EquipTid == id)
			{
				num++;
			}
		}
		return num;
	}

	public EPreviewType GetPreviewType(EResourceType m_type, int m_id)
	{
		EPreviewType result = EPreviewType.EmptyValue;
		switch (m_type)
		{
		case EResourceType.Prop:
		{
			zbPropCsvRecord recordByKey2 = zbDataConfig.Instance.propCsvTable.GetRecordByKey(m_id);
			if (recordByKey2 != null)
			{
				if (recordByKey2.type == 8)
				{
					result = EPreviewType.Suit;
				}
				else if (recordByKey2.type == 9)
				{
					result = EPreviewType.Gun;
				}
				else if (recordByKey2.type == 12)
				{
					result = EPreviewType.Mecha;
				}
				else if (recordByKey2.type == 13)
				{
					result = EPreviewType.Wall;
				}
				else if (recordByKey2.type == 14)
				{
					result = EPreviewType.Foothold;
				}
			}
			break;
		}
		case EResourceType.Hero:
		case EResourceType.HeroFrag:
		{
			zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(m_id);
			if (recordByKey != null && recordByKey.type != 1)
			{
				result = EPreviewType.Hero;
			}
			break;
		}
		}
		return result;
	}
}
