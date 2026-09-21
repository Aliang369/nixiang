using System.Collections.Generic;

public class GetGatherElement : Effect
{
	public NewItemGenerator itemGenerator;

	public InfluenceDefGenerator influenceGenerator;

	public ValueGenerator coinGenerator;

	public ValueGenerator crystalGenerator;

	public SkillGenerator skillGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		MultiInfoDisplayPanel.SetupInfo setupInfo = new MultiInfoDisplayPanel.SetupInfo();
		setupInfo.items = itemGenerator?.GenerateNewItems(eve);
		setupInfo.influences = influenceGenerator?.GenerateInfluenceDefs(eve).ConvertAll((InfluenceDef infd) => new Influence(infd));
		setupInfo.coin = (coinGenerator?.GenerateValue(eve)).GetValueOrDefault();
		setupInfo.crystal = (crystalGenerator?.GenerateValue(eve)).GetValueOrDefault();
		setupInfo.skills = skillGenerator?.GenerateSkills(eve);
		Effect.EnqueueAction(eve, new GatherMultiElementsAction(setupInfo));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemGenerator, influenceGenerator, coinGenerator, crystalGenerator, skillGenerator };
	}
}
