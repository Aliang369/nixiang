public class zbRobItemData
{
	public int index;

	public bool isPrize;

	public bool isRob;

	public zbItemData itemData;

	public zbRobItemData(int newIndex, bool newIsPrize, bool newIsRob, zbItemData newItemData)
	{
		index = newIndex;
		isPrize = newIsPrize;
		isRob = newIsRob;
		itemData = newItemData;
	}
}
