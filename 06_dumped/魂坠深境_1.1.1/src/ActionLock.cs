using UnityEngine;

public class ActionLock
{
	public enum Type
	{
		Default,
		DrawFromPile,
		AddItem,
		RemoveItem,
		MoveItem,
		SwapItem,
		ShuffleTable,
		Spell,
		HintOverload,
		SpellEnd,
		Influence,
		DeathPending,
		AttachRunePanel,
		ClavisHint,
		StoryAvatarSpeak,
		ForerunnerGiveLegacy,
		Cutscene,
		Intro
	}

	public int count;

	public Type type;

	public ActionLock(Type type)
	{
		this.type = type;
		count = 0;
	}

	public void Acquire()
	{
		count++;
	}

	public void Release()
	{
		count = Mathf.Max(count - 1, 0);
	}

	public bool IsLocked(GameAction action)
	{
		if (count > 0)
		{
			return !action.ignoreLocks.Contains(type);
		}
		return false;
	}
}
