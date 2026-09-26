using UnityEngine.UIElements;

public class zbSeasonRankItemScience : zbViewBaseItem
{
	public zbSeasonRankItemScience(zbSeasonRankWnd wndParent)
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/season/tb_seasonRankScience.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void Refresh()
	{
	}
}
