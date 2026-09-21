using UnityEngine;

public class FortressBoot : BootSetter<FortressStateInfo>
{
	[SerializeField]
	public FortressUIManager uiManager;

	public override void Setup(FortressStateInfo fortressState)
	{
		fortressState.uiManager = uiManager;
		base.Setup(fortressState);
	}
}
