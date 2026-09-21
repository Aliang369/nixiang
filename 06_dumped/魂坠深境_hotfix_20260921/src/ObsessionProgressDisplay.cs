using UnityEngine;

public class ObsessionProgressDisplay : CellUI<PlayerCharacterState>
{
	private const string normalText = "<color=#FFFFFF>{0}</color><size=90%>/{1}</size>";

	private const string lackText = "<color=#FFE397>{0}</color><size=90%>/{1}</size>";

	[SerializeField]
	private ProgressBar progressBar;

	protected override void OnSetup(PlayerCharacterState setupData)
	{
		ForceUpdate();
	}

	protected override void UpdateDisplay()
	{
		int requiredCount = base.setupData.currentObsessionDebrisRequiredCount;
		progressBar.Set(base.setupData.obsessionDebrisCount, requiredCount, () => progressBar.text.text = ((base.setupData.obsessionDebrisCount >= requiredCount) ? "<color=#FFFFFF>{0}</color><size=90%>/{1}</size>" : "<color=#FFE397>{0}</color><size=90%>/{1}</size>").FormatWith(base.setupData.obsessionDebrisCount, requiredCount));
	}
}
