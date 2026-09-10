using ContainerOfSoul.Display.Fortress.IndexDisplay;
using UnityEngine;

public class InfluenceRecordInfoPanelModule : InfluenceDisplay.DisplayModule
{
	[SerializeField]
	private GameButton button;

	public override void Init()
	{
		button.onClick.AddListener(_OnButtonClicked);
	}

	private void _OnButtonClicked()
	{
		GetComponentInParent<ReocrdInfoPanel>()?.influenceInfoDisplay.Setup(base.influence);
	}
}
