using Proto.Enums;
using Proto.Types;
using zbType;

public class zbGetWayBuyPanelData
{
	public EGetWayBuyPanelType Type;

	public AwardDetail[] AwardList;

	public int Limit;

	public int Count;

	public EMoneyType MoneyType;

	public string Price;

	public int IndexOrId;

	public string Bonus;

	public OperateBaseInfo BaseInfo;

	public zbGetWayBuyPanelData(EGetWayBuyPanelType type, AwardDetail[] awardList, int limit, int count, EMoneyType moneyType, string price, int indexOrId, string bonus = "", OperateBaseInfo baseInfo = null)
	{
		Type = type;
		AwardList = awardList;
		Limit = limit;
		Count = count;
		MoneyType = moneyType;
		Price = price;
		IndexOrId = indexOrId;
		Bonus = bonus;
		BaseInfo = baseInfo;
	}
}
