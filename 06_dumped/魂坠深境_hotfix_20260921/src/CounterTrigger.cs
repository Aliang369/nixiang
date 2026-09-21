using System.Collections.Generic;

public class CounterTrigger : Ability
{
	public enum Type
	{
		Full,
		Zero,
		LessEqualOne,
		ToTargetValue,
		WithChange,
		OutTargetValue,
		AddToMultiTargetValue,
		AddOutMultiTargetValue,
		AddToTargetValue,
		SubToTargetValue,
		AddOutTargetValue,
		SubOutTargetValue
	}

	public Effect effect;

	public List<int> multiTargetCounters;

	public bool resetmCounterAfterTrigger;

	public ValueGenerator targetCounterGen;

	public Type type;

	private bool showCounterGen
	{
		get
		{
			if (type != Type.ToTargetValue && type != Type.OutTargetValue && type != Type.AddToTargetValue && type != Type.SubToTargetValue && type != Type.AddOutTargetValue)
			{
				return type == Type.SubOutTargetValue;
			}
			return true;
		}
	}

	private bool showMultiCounter
	{
		get
		{
			if (type != Type.AddToMultiTargetValue)
			{
				return type == Type.AddOutMultiTargetValue;
			}
			return true;
		}
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { triggerCheckNode, targetCounterGen, effect };
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			TriggerEffect(eve, effect);
			if (resetmCounterAfterTrigger)
			{
				eve.gameState.actionManager.EnqueueAction(new ResetCounterAction(eve.CasterItem, eve.CasterItem));
			}
		}
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.ChangeCounter;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve))
		{
			return false;
		}
		if (eve.type == GameEvent.Type.ChangeCounter && eve.TargetItem == eve.CasterItem)
		{
			int num = ((targetCounterGen != null) ? targetCounterGen.GenerateValue(eve).Value : 0);
			if (eve.deltaValue != 0)
			{
				int num2 = eve.CasterItem.counter;
				switch (type)
				{
				case Type.Full:
					if (num2 >= (int)eve.CasterItem.maxCounter)
					{
						return true;
					}
					break;
				case Type.Zero:
					if (num2 <= 0)
					{
						return true;
					}
					break;
				case Type.LessEqualOne:
					if (num2 <= 1)
					{
						return true;
					}
					break;
				case Type.ToTargetValue:
					if (num2 == num)
					{
						return true;
					}
					break;
				case Type.WithChange:
					if (eve.deltaValue != 0)
					{
						return true;
					}
					break;
				case Type.OutTargetValue:
					if (num2 != num && num2 == num + eve.deltaValue)
					{
						return true;
					}
					break;
				case Type.AddToMultiTargetValue:
					if (multiTargetCounters != null && eve.deltaValue > 0 && multiTargetCounters.Contains(num2))
					{
						return true;
					}
					break;
				case Type.AddOutMultiTargetValue:
					if (multiTargetCounters != null && eve.deltaValue > 0 && multiTargetCounters.Contains(num2 - eve.deltaValue))
					{
						return true;
					}
					break;
				case Type.AddToTargetValue:
					if (num2 == num && eve.deltaValue > 0)
					{
						return true;
					}
					break;
				case Type.SubToTargetValue:
					if (num2 == num && eve.deltaValue < 0)
					{
						return true;
					}
					break;
				case Type.AddOutTargetValue:
					if (num2 != num && num2 == num + eve.deltaValue && eve.deltaValue > 0)
					{
						return true;
					}
					break;
				case Type.SubOutTargetValue:
					if (num2 != num && num2 == num + eve.deltaValue && eve.deltaValue < 0)
					{
						return true;
					}
					break;
				}
			}
		}
		return false;
	}
}
