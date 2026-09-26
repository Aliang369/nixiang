using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Types;

public class zbGrowthGiftHandler
{
	public RechargeShopGrowthGiftAck tmpRechargeShopGrowthGiftAck;

	public bool hasNotify = true;

	public int growthGiftPopupInterval = 3600;

	public string growthGiftPopupKey;

	private zbGiftTaskCsvRecord[] m_records;

	public bool isNewGrowPack;

	public void Init()
	{
		growthGiftPopupKey = zbSelf.Instance.GetRoleId() + "growthGiftPopupKey";
		m_records = zbDataConfig.Instance.giftTaskCsvTable.GetRecords().records;
	}

	public List<GrowthGiftShopDetail> GetGrowthGiftList()
	{
		RepeatedField<GrowthGiftShopDetail> val = zbServerData.sGameData?.RechargeShopInfo?.GrowthGiftShop;
		if (val == null || zbSelf.Instance?.timeHandler == null)
		{
			return new List<GrowthGiftShopDetail>();
		}
		long now = zbSelf.Instance.timeHandler.GetServerTime();
		return (from g in ((IEnumerable<GrowthGiftShopDetail>)val).ToList()
			where g != null && g.ExpireTm > now
			select g).ToList();
	}

	public GrowthGiftShopDetail FindEarliestExpiringGrowthGift()
	{
		if (zbServerData.sGameData.RechargeShopInfo != null && zbServerData.sGameData.RechargeShopInfo.GrowthGiftShop != null && zbServerData.sGameData.RechargeShopInfo.GrowthGiftShop.Count > 0)
		{
			return (from detail in (IEnumerable<GrowthGiftShopDetail>)zbServerData.sGameData.RechargeShopInfo.GrowthGiftShop
				where detail.ExpireTm > zbSelf.Instance.timeHandler.GetServerTime()
				orderby detail.ExpireTm
				select detail).FirstOrDefault();
		}
		return null;
	}

	public GrowthGiftShopDetail FindNewActiveGrowthGift()
	{
		if (zbServerData.sGameData.RechargeShopInfo != null && zbServerData.sGameData.RechargeShopInfo.GrowthGiftShop != null && zbServerData.sGameData.RechargeShopInfo.GrowthGiftShop.Count > 0)
		{
			long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
			return ((IEnumerable<GrowthGiftShopDetail>)zbServerData.sGameData.RechargeShopInfo.GrowthGiftShop).Where(delegate(GrowthGiftShopDetail info)
			{
				zbGiftTaskCsvRecord zbGiftTaskCsvRecord2 = m_records.FirstOrDefault((zbGiftTaskCsvRecord r) => r.goodsId == info.GoodsId);
				return zbGiftTaskCsvRecord2 != null && info.ExpireTm - zbGiftTaskCsvRecord2.growthHour * 3600 <= serverTime;
			}).OrderByDescending(delegate(GrowthGiftShopDetail info)
			{
				zbGiftTaskCsvRecord zbGiftTaskCsvRecord2 = m_records.FirstOrDefault((zbGiftTaskCsvRecord r) => r.goodsId == info.GoodsId);
				return info.ExpireTm - zbGiftTaskCsvRecord2.growthHour * 3600;
			}).FirstOrDefault();
		}
		return null;
	}

	public bool CheckCanPopupGrowthGift()
	{
		if (tmpRechargeShopGrowthGiftAck != null)
		{
			return true;
		}
		if (GetGrowthGiftList().Count == 0)
		{
			return false;
		}
		long num = zbGameSave.Instance.GetLong(growthGiftPopupKey, 0L);
		if (num <= 0)
		{
			return true;
		}
		return zbSelf.Instance.timeHandler.GetServerTime() - num >= growthGiftPopupInterval;
	}

	public bool HasRedPoint()
	{
		return hasNotify;
	}
}
