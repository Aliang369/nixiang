using UnityEngine;

public class MItemBlackBoardValueGenerator : ValueGenerator
{
	[SerializeField]
	private readonly float ratio = 1f;

	[SerializeField]
	public ItemBlackBoard.ValueType type;

	public override int? GenerateValue(GameEvent eve)
	{
		Item item = eve.CasterItem?.GetRuntimeTransformBlackboardSourceItem();
		if (item?.blackboard == null)
		{
			return null;
		}
		return Mathf.RoundToInt((float)item.blackboard.GetValue(type).GetValueOrDefault() * ratio);
	}
}
