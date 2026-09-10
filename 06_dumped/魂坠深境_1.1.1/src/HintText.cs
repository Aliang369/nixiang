using UnityEngine;

public class HintText : Effect
{
	[SerializeField]
	[LocalizeText("节点文本")]
	private string text;

	public bool halt;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Effect.EnqueueAction(eve, new HintAction(TextDataPreset.FormatEffectObjectDescription(eve.Caster as IEffectObjectDisplay, text, eve, out var _), halt));
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		TextDataGenerator generator;
		return new NodeDisplayInfo
		{
			str = new NodeDisplayInfo.Element<string>(TextDataPreset.FormatEffectObjectDescription(eve.Caster as IEffectObjectDisplay, text, eve, out generator))
		};
	}
}
