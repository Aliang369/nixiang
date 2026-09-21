public class AddItemEventInfo
{
	public enum AddType
	{
		Spawn,
		Transfer
	}

	public Item addItem;

	public AddType addType;

	public object source;

	public ItemContainer targetContainer;
}
