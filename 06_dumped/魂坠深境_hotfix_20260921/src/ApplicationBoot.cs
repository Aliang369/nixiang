using UnityEngine;

public class ApplicationBoot : BootSetter<ApplicationStateInfo>
{
	[SerializeField]
	public ApplicationUIManager applicationUIManager;

	private void Update()
	{
		if (bootTarget != null)
		{
			bootTarget.FrameUpdate();
		}
	}

	public override void Setup(ApplicationStateInfo t)
	{
		base.Setup(t);
		t.applicationUIManager = applicationUIManager;
	}
}
