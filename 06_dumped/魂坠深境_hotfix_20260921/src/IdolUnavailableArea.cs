using TMPro;

public class IdolUnavailableArea : CellUI<int>
{
	protected override void OnSetup(int setupData)
	{
		base.gameObject.SetActive(value: true);
		TextMeshProUGUI[] componentsInChildren = GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.SetActive(value: false);
		}
	}
}
