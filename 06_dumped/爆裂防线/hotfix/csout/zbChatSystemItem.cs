using UnityEngine.UIElements;

public class zbChatSystemItem : zbViewBaseItem
{
	public zbChatSystemItem(VisualTreeAsset tAsset)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
	}

	public void Init(zbChatWnd parent, object data)
	{
	}
}
