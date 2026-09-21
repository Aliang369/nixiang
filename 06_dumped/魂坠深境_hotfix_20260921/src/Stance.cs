using System.Collections.Generic;
using System.Linq;

public class Stance : IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IGameEventSubscriber, IBftOwnable
{
	public enum ChangeType
	{
		None,
		Stack,
		Shift,
		Full,
		Break
	}

	[SaveField(null, null)]
	public StanceDef def;

	[SaveField(null, null)]
	public int value;

	[SaveField(null, null)]
	public BattleFieldTransform ownerBft;

	[SaveField(null, null)]
	public Dictionary<string, object> tempValues;

	public BattleFieldTransform OwnerBft => ownerBft;

	private Stance()
	{
	}

	public Stance(StanceDef def, int value, BattleFieldTransform ownerBft)
	{
		this.def = def;
		this.ownerBft = ownerBft;
		this.value = value;
	}

	public static Stance Cast(Stance previous, Stance cast, out ChangeType changeType, bool preview = false)
	{
		changeType = ChangeType.None;
		if (previous == null && cast == null)
		{
			return null;
		}
		if (previous == null)
		{
			if (cast.value > 0)
			{
				return new Stance(cast.def, cast.value, cast.ownerBft);
			}
			return null;
		}
		if (cast == null)
		{
			return new Stance(previous.def, previous.value, previous.ownerBft);
		}
		if (previous.def != cast.def)
		{
			changeType = ChangeType.Shift;
			return new Stance(cast.def, cast.value, cast.ownerBft);
		}
		if (previous.def == cast.def)
		{
			if (previous.value + cast.value <= 0)
			{
				changeType = ChangeType.Break;
				return null;
			}
			if (cast.value < 0)
			{
				changeType = ChangeType.None;
			}
			changeType = ChangeType.Stack;
			return Add(previous, cast);
		}
		return null;
	}

	public static Stance Add(Stance a, Stance b)
	{
		if (a == null && b == null)
		{
			return null;
		}
		if (a == null)
		{
			return new Stance(b.def, b.value, b.ownerBft);
		}
		if (b == null)
		{
			return new Stance(a.def, a.value, a.ownerBft);
		}
		if (a.def != b.def)
		{
			if (a.def == null && b.def == null)
			{
				return null;
			}
			if (a.def == null)
			{
				return new Stance(b.def, b.value, b.ownerBft);
			}
			if (b.def == null)
			{
				return new Stance(a.def, a.value, a.ownerBft);
			}
			if (a.value == b.value)
			{
				return null;
			}
			if (a.value > b.value)
			{
				int num = a.value - b.value;
				if (num > 0)
				{
					return new Stance(a.def, num, a.ownerBft);
				}
				return null;
			}
			int num2 = b.value - a.value;
			if (num2 > 0)
			{
				return new Stance(b.def, num2, b.ownerBft);
			}
			return null;
		}
		int num3 = a.value + b.value;
		if (num3 > 0)
		{
			return new Stance(a.def, num3, a.ownerBft);
		}
		return null;
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		if (def == null)
		{
			return null;
		}
		return def.GetNodeDisplayInfo(eve);
	}

	public List<IEffectNode> GetChildren()
	{
		if (def == null)
		{
			return null;
		}
		return def.GetChildren();
	}

	public IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		if (def == null)
		{
			return null;
		}
		return def.abilities.SelectMany((Ability a) => a.GetAllSubscribeGameEventType());
	}

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (def == null)
		{
			return false;
		}
		return def.abilities.Any((Ability a) => a.CheckTriggerGameEventAble(eve));
	}

	public void TriggerGameEvent(GameEvent eve)
	{
		if (def == null)
		{
			return;
		}
		foreach (Ability ability in def.abilities)
		{
			ability.TriggerGameEvent(eve);
		}
	}
}
