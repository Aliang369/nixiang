public class zbGuideBase
{
	private bool guildOver = true;

	public virtual void Start()
	{
		guildOver = false;
	}

	public bool isOver()
	{
		return guildOver;
	}

	public void Stop()
	{
		guildOver = true;
	}

	public virtual void Update()
	{
	}
}
