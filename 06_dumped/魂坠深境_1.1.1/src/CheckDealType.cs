using UnityEngine;

public class CheckDealType : CheckNode
{
	[SerializeField]
	private GameEvent.DealType dealType;

	public override bool? Check(GameEvent eve)
	{
		return eve.CheckDealType(dealType);
	}
}
