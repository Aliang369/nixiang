using Proto.Types;
using UnityEngine.UIElements;

public class zbSeasonRankItemForce : zbViewBaseItem
{
	public zbSeasonRankItemForce(zbSeasonRankWnd wndParent)
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/season/tb_seasonRankForce.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void Refresh(RankDetail detail)
	{
	}
}
