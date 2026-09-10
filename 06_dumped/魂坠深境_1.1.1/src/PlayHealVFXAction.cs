public class PlayHealVFXAction : ExecuteAction
{
	private readonly BattleFieldTransform bft;

	public PlayHealVFXAction(BattleFieldTransform bft)
	{
		this.bft = bft;
	}

	protected override void Update()
	{
		if (bft.isPlayer)
		{
			EnqueueAction(new DisplayNFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_heal_UI, bft.GetUIPivot(gameState)));
		}
		else if (bft.attachDisplay != null)
		{
			EnqueueAction(new DisplayNFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_heal, bft.Get3DNFXPivot(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_heal)));
		}
	}
}
