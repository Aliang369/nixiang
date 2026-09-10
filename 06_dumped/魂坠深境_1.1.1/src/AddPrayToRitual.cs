public class AddPrayToRitual : Effect
{
	public ValueGenerator countGenerator;

	public int index;

	public CTScriptableObject source;

	private IEffectObjectDisplay SourceObjectDisplay => source as IEffectObjectDisplay;

	private PrayAbility SourcePrayAbility => SourceObjectDisplay.GetChildren().IndexOrNull(index) as PrayAbility;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (SourcePrayAbility != null)
		{
			int num = countGenerator?.GetValue(eve) ?? 1;
			for (int i = 0; i < num; i++)
			{
				Pray pray = new Pray(SourceObjectDisplay, index);
				if (eve.Caster is IBftOwnable bftOwnable)
				{
					pray.owner = bftOwnable.OwnerBft;
				}
				Effect.EnqueueAction(eve, new AddPrayToRitualAction(pray));
			}
		}
		return NodeCastResult.Failure;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		NodeDisplayInfo nodeDisplayInfo = new NodeDisplayInfo();
		if (SourcePrayAbility != null && !(eve.Caster is Pray))
		{
			nodeDisplayInfo.pray = new NodeDisplayInfo.Element<Pray>(new Pray(SourceObjectDisplay, index));
		}
		return nodeDisplayInfo;
	}
}
