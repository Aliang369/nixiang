public class EffectNodeInfo
{
	public IEffectObjectDisplay displayObjet;

	public int index;

	public IEffectNode referenceNode;

	public string typeName => referenceNode.GetType().Name;

	public EffectNodeInfo(int index, IEffectNode referenceNode, IEffectObjectDisplay displayObjet)
	{
		this.index = index;
		this.referenceNode = referenceNode;
		this.displayObjet = displayObjet;
	}
}
