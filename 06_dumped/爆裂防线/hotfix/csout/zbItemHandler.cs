using System.Linq;
using Proto.Enums;
using Proto.Types;

public class zbItemHandler
{
	private int m_awakenUpgradeId = -1;

	private int m_awakenAdvanceId = -1;

	public void Init()
	{
	}

	public int GetItemIdByType(EItemType itemType)
	{
		if (itemType == EItemType.HeroAwakenUpgrade && m_awakenUpgradeId != -1)
		{
			return m_awakenUpgradeId;
		}
		if (itemType == EItemType.HeroAwakenAdvance && m_awakenAdvanceId != -1)
		{
			return m_awakenAdvanceId;
		}
		foreach (zbItemCsvRecord value in zbDataConfig.Instance.itemCsvTable.GetRecords().records.Values)
		{
			if (value.type == (int)itemType)
			{
				if (itemType == EItemType.HeroAwakenUpgrade)
				{
					m_awakenUpgradeId = value.id;
				}
				if (itemType == EItemType.HeroAwakenAdvance)
				{
					m_awakenAdvanceId = value.id;
				}
				return value.id;
			}
		}
		return 0;
	}

	public long GetCountByItemTid(int tid)
	{
		foreach (ResourceDetail item in zbServerData.sGameData.ItemList)
		{
			if (item.Tid == tid && item.Count > 0)
			{
				return item.Count;
			}
		}
		return 0L;
	}

	public bool HasEnoughItem(int tid, long needNum)
	{
		return GetCountByItemTid(tid) >= needNum;
	}

	public long GetCountByPropTid(int tid)
	{
		foreach (PropDetail prop in zbServerData.sGameData.PropList)
		{
			if (prop.Tid == tid && prop.Count > 0)
			{
				return prop.Count;
			}
		}
		return 0L;
	}

	public bool HasEnoughProp(int tid, long needNum)
	{
		return GetCountByPropTid(tid) >= needNum;
	}

	public bool HasPropWorkerNotify()
	{
		foreach (PropDetail prop in zbServerData.sGameData.PropList)
		{
			zbPropCsvRecord recordByKey = zbDataConfig.Instance.propCsvTable.GetRecordByKey(prop.Tid);
			if (recordByKey != null && recordByKey.type == 16)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasBoxNotify(int id)
	{
		zbBoxCsvRecord recordByKey = zbDataConfig.Instance.boxCsvTable.GetRecordByKey(id);
		if (recordByKey != null && zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Box, id) >= recordByKey.boxCost)
		{
			if (recordByKey.key <= 0)
			{
				return true;
			}
			if (zbDataConfig.Instance.itemCsvTable.GetRecordByKey(recordByKey.key) != null && GetCountByItemTid(recordByKey.key) > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyBoxNotify()
	{
		foreach (ResourceDetail box in zbServerData.sGameData.BoxList)
		{
			zbBoxCsvRecord recordByKey = zbDataConfig.Instance.boxCsvTable.GetRecordByKey(box.Tid);
			if (recordByKey != null && box.Count >= recordByKey.boxCost)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRedPoint()
	{
		if (!HasPropWorkerNotify())
		{
			return HasAnyBoxNotify();
		}
		return true;
	}

	public long GetItemCount(EItemType itemClass)
	{
		long num = 0L;
		foreach (ResourceDetail item in zbServerData.sGameData.ItemList)
		{
			zbItemCsvRecord recordByKey = zbDataConfig.Instance.itemCsvTable.GetRecordByKey(item.Tid);
			if (recordByKey != null && recordByKey.type == (int)itemClass)
			{
				num += item.Count;
			}
		}
		return num;
	}

	public string GetStarMaxDesc(int propId)
	{
		zbPropCsvRecord recordByKey = zbDataConfig.Instance.propCsvTable.GetRecordByKey(propId);
		if (recordByKey == null)
		{
			return "";
		}
		int num = -1;
		int key = 5;
		string arg = "";
		if (recordByKey.type == 8)
		{
			zbPlayerSuitCsvRecord zbPlayerSuitCsvRecord2 = zbDataConfig.Instance.playerSuitCsvTable.GetRecords().records.Values.FirstOrDefault((zbPlayerSuitCsvRecord record) => record.propId == propId);
			if (zbPlayerSuitCsvRecord2 != null)
			{
				PlayerSuitDetail playerSuitDetail = zbSelf.Instance.suitGunHandler.GetPlayerSuitDetail(zbPlayerSuitCsvRecord2.id);
				if (playerSuitDetail != null && playerSuitDetail.Star >= 5)
				{
					num = zbPlayerSuitCsvRecord2.convertPropId;
					key = zbPlayerSuitCsvRecord2.quality;
				}
			}
			arg = zbUtil.Language("lang.11394");
		}
		else if (recordByKey.type == 9)
		{
			zbPlayerGunCsvRecord zbPlayerGunCsvRecord2 = zbDataConfig.Instance.playerGunCsvTable.GetRecords().records.Values.FirstOrDefault((zbPlayerGunCsvRecord record) => record.propId == propId);
			if (zbPlayerGunCsvRecord2 != null)
			{
				PlayerGunDetail playerGunDetail = zbSelf.Instance.suitGunHandler.GetPlayerGunDetail(zbPlayerGunCsvRecord2.id);
				if (playerGunDetail != null && playerGunDetail.Star >= zbSelf.Instance.suitGunHandler.maxGunStar)
				{
					num = zbPlayerGunCsvRecord2.convertPropId;
					key = zbPlayerGunCsvRecord2.quality;
				}
			}
			arg = zbUtil.Language("lang.10684");
		}
		else if (recordByKey.type == 12)
		{
			zbMechaCsvRecord zbMechaCsvRecord2 = zbDataConfig.Instance.mechaCsvTable.GetRecords().records.Values.FirstOrDefault((zbMechaCsvRecord record) => record.propId == propId);
			if (zbMechaCsvRecord2 != null)
			{
				MechaDetail mechaDetail = zbSelf.Instance.mechaHandler.GetMechaDetail(zbMechaCsvRecord2.id);
				if (mechaDetail != null && mechaDetail.Star >= 5)
				{
					num = zbMechaCsvRecord2.convertPropId;
					key = zbMechaCsvRecord2.quality;
				}
			}
			arg = zbUtil.Language("lang.10791");
		}
		else if (recordByKey.type == 13)
		{
			zbWallCsvRecord zbWallCsvRecord2 = zbDataConfig.Instance.wallCsvTable.GetRecords().records.Values.FirstOrDefault((zbWallCsvRecord record) => record.propId == propId);
			if (zbWallCsvRecord2 != null)
			{
				WallDetail wallDetail = zbSelf.Instance.wallHandler.GetWallDetail(zbWallCsvRecord2.id);
				if (wallDetail != null && wallDetail.Star >= 5)
				{
					num = zbWallCsvRecord2.convertPropId;
					key = zbWallCsvRecord2.quality;
				}
			}
			arg = zbUtil.Language("WeiQiang");
		}
		else if (recordByKey.type == 14)
		{
			zbFootholdCsvRecord zbFootholdCsvRecord2 = zbDataConfig.Instance.footholdCsvTable.GetRecords().records.Values.FirstOrDefault((zbFootholdCsvRecord record) => record.propId == propId);
			if (zbFootholdCsvRecord2 != null)
			{
				FootholdDetail footholdDetail = zbSelf.Instance.guardHandler.GetFootholdDetail(zbFootholdCsvRecord2.id);
				if (footholdDetail != null && footholdDetail.Star >= zbSelf.Instance.guardHandler.maxFootholdStar)
				{
					num = zbFootholdCsvRecord2.convertPropId;
					key = zbFootholdCsvRecord2.quality;
				}
			}
			arg = zbUtil.Language("lang.11395");
		}
		if (num != -1)
		{
			stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Prop, num);
			zbQualityCsvRecord recordByKey2 = zbDataConfig.Instance.qualityCsvTable.GetRecordByKey(key);
			return string.Format(zbUtil.Language("lang.11393"), resorceIconPath.strName, recordByKey2.name, arg);
		}
		return "";
	}
}
