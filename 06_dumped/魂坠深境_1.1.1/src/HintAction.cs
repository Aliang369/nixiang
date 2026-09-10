public class HintAction : GameAction
{
	private readonly bool halt;

	private bool started;

	public string text;

	private QuickHint hintObj;

	public HintAction(string text, bool halt = false)
	{
		this.text = text;
		this.halt = halt;
	}

	protected override void Update()
	{
		if (!started)
		{
			hintObj = gameState.UIManager.quickHintPanel.HintText(text, base.actionTimeScale);
			started = true;
		}
		if (started && (!halt || !hintObj.gameObject.activeInHierarchy))
		{
			FinishAction();
		}
	}
}
