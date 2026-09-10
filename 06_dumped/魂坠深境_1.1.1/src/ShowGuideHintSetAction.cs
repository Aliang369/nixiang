using System;

public class ShowGuideHintSetAction : ExecuteAction
{
	private GuideHintPanelSet presetPanel;

	private Action onClick;

	public ShowGuideHintSetAction(GuideHintPanelSet presetPanel, Action onClick = null)
	{
		this.presetPanel = presetPanel;
		this.onClick = onClick;
	}

	protected override void Update()
	{
		presetPanel.Show(onClick);
	}
}
