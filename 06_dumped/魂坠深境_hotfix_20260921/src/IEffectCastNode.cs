public interface IEffectCastNode : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	Effect.NodeCastResult Cast(GameEvent eve);
}
