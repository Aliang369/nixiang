using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;

public class zbMoneyHandler
{
	public void Init()
	{
	}

	public List<ResourceDetail> GetMoneyList()
	{
		List<ResourceDetail> list = new List<ResourceDetail>();
		foreach (ResourceDetail money in zbServerData.sGameData.MoneyList)
		{
			if (money.Count > 0)
			{
				list.Add(money);
			}
		}
		return list;
	}

	public long GetCountByTid(int tid)
	{
		foreach (ResourceDetail money in zbServerData.sGameData.MoneyList)
		{
			if (money.Tid == tid && money.Count > 0)
			{
				return money.Count;
			}
		}
		return 0L;
	}

	public bool HasEnoughMoney(int tid, long needNum)
	{
		return GetCountByTid(tid) >= needNum;
	}

	public MoneyExchangeDetail GetMoneyExchangeDetail(EMoneyType moneyType)
	{
		foreach (MoneyExchangeDetail exchange in zbServerData.sGameData.ExchangeList)
		{
			if (exchange.MoneyType == moneyType)
			{
				return exchange;
			}
		}
		return null;
	}

	public bool IsMoneyExchangeHalf(EMoneyType moneyType)
	{
		MoneyExchangeDetail moneyExchangeDetail = GetMoneyExchangeDetail(moneyType);
		if (moneyExchangeDetail != null)
		{
			if (moneyExchangeDetail.Half == EBoolState.False)
			{
				return true;
			}
			return false;
		}
		zbMoneyExchangeCsvRecord recordByKey = zbDataConfig.Instance.moneyExchangeCsvTable.GetRecordByKey((int)moneyType);
		if (recordByKey != null && recordByKey.half == 1)
		{
			return true;
		}
		return false;
	}
}
