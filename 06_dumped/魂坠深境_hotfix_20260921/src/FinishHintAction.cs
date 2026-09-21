public class FinishHintAction : GameAction
{
	private readonly bool halt;

	private bool started;

	public string text;

	private FinishHint hintObj;

	public FinishHintAction(string text, bool halt = false)
	{
		this.text = text;
		this.halt = halt;
	}

	protected override void Update()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (!started)
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_win_boss_fight_hint);
			hintObj = gameState.UIManager.finishHintPanel.HintText(text, base.actionTimeScale);
			started = true;
		}
		if (started && (!halt || hintObj?.gameObject == null || !hintObj.gameObject.activeInHierarchy))
		{
			FinishAction();
		}
	}
}
