using UnityEngine.UIElements;

public class zbSeasonRankItemPlayer : zbViewBaseItem
{
	public zbSeasonRankItemPlayer(zbSeasonRankWnd wndParent)
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/season/tb_seasonRankPlayer.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void Refresh()
	{
	}
}
