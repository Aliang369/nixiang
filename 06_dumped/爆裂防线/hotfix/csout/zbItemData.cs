using Proto.Enums;

public class zbItemData
{
	public EResourceType type;

	public long id;

	public long count;

	public zbItemData(EResourceType newType, long newId, long newCount)
	{
		type = newType;
		id = newId;
		count = newCount;
	}
}
