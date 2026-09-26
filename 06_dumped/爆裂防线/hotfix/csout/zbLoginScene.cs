public class zbLoginScene : zbBaseScene
{
	protected override void OnEnable()
	{
		base.OnEnable();
	}

	protected override void Start()
	{
		base.Start();
		zbUIMgr.Instance.ShowLogin();
	}
}
