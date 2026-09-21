using System.Collections.Generic;
using DG.Tweening;

public class MultiNode : CameraControlNode
{
	public List<CameraControlNode> nodes;

	public bool seq;

	public override Sequence ExecuteCor(ApplicationStateInfo applicationState)
	{
		Sequence sequence = DOTween.Sequence();
		if (seq)
		{
			foreach (CameraControlNode node in nodes)
			{
				sequence.Append(node.ExecuteCor(applicationState));
			}
		}
		else
		{
			foreach (CameraControlNode node2 in nodes)
			{
				sequence.Join(node2.ExecuteCor(applicationState));
			}
		}
		return sequence;
	}
}
