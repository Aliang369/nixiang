using UnityEngine;

public class BoolCheckNode : CheckNode
{
	[SerializeField]
	private bool value;

	public override bool? Check(GameEvent eve)
	{
		return value;
	}
}
