using System;
using System.Collections.Generic;

public class ChapterTriggerAbility : Ability
{
	public enum ChapterTriggerType
	{
		TriggerChapter,
		FirstChapter,
		FinalChapter,
		IndexChapter
	}

	public List<int> chapterIndex;

	public Effect chapterTriggerEffect;

	public ChapterTriggerType chapterTriggerType;

	public override void TriggerGameEvent(GameEvent eve)
	{
		switch (chapterTriggerType)
		{
		case ChapterTriggerType.TriggerChapter:
		{
			if ((eve.type != GameEvent.Type.ChangeCounter || eve.deltaValue <= 0) && (eve.type != GameEvent.Type.TriggerChapterEffect || eve.TargetItem != eve.CasterItem))
			{
				break;
			}
			int num = ((eve.type != GameEvent.Type.ChangeCounter) ? 1 : eve.deltaValue);
			int num2 = ((eve.type == GameEvent.Type.ChangeCounter) ? eve.amount : ((int)eve.CasterItem.counter));
			bool flag = eve.nodeTempValues.TryGetValue("TriggerChapterIndexKey", out var value);
			try
			{
				for (int i = 0; i < num; i++)
				{
					int num3 = num2 - num + i + 1;
					eve.nodeTempValues["TriggerChapterIndexKey"] = num3;
					chapterTriggerEffect?.Cast(eve);
					eve.gameState.gameEventManager.TriggerAfterTriggerChapter(eve.CasterItem, eve.CasterItem.counter);
				}
				break;
			}
			finally
			{
				if (flag)
				{
					eve.nodeTempValues["TriggerChapterIndexKey"] = value;
				}
				else
				{
					eve.nodeTempValues.Remove("TriggerChapterIndexKey");
				}
			}
		}
		case ChapterTriggerType.FirstChapter:
			if (eve.type == GameEvent.Type.ChangeCounter && eve.deltaValue > 0 && eve.amount - eve.deltaValue == 0)
			{
				chapterTriggerEffect?.Cast(eve);
			}
			break;
		case ChapterTriggerType.FinalChapter:
			if (eve.type == GameEvent.Type.CounterReachMaxCounter)
			{
				chapterTriggerEffect?.Cast(eve);
			}
			break;
		case ChapterTriggerType.IndexChapter:
			if ((eve.type == GameEvent.Type.TriggerChapterEffect && chapterIndex.Contains(eve.TargetItem.counter)) || (eve.type == GameEvent.Type.ChangeCounter && chapterIndex.Contains(eve.TargetItem.counter)))
			{
				chapterTriggerEffect?.Cast(eve);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (triggerCheckNode != null && triggerCheckNode.Check(eve) == false)
		{
			return false;
		}
		if (eve.CasterItem != eve.TargetItem)
		{
			return false;
		}
		switch (chapterTriggerType)
		{
		case ChapterTriggerType.TriggerChapter:
			if ((eve.type == GameEvent.Type.ChangeCounter && eve.deltaValue > 0) || (eve.type == GameEvent.Type.TriggerChapterEffect && eve.TargetItem == eve.CasterItem))
			{
				return true;
			}
			break;
		case ChapterTriggerType.FirstChapter:
			if (eve.type == GameEvent.Type.ChangeCounter && eve.deltaValue > 0 && eve.amount - eve.deltaValue == 0)
			{
				return true;
			}
			break;
		case ChapterTriggerType.FinalChapter:
			if (eve.type == GameEvent.Type.CounterReachMaxCounter)
			{
				return true;
			}
			break;
		case ChapterTriggerType.IndexChapter:
			if ((eve.type == GameEvent.Type.TriggerChapterEffect && chapterIndex.Contains(eve.TargetItem.counter)) || (eve.type == GameEvent.Type.ChangeCounter && chapterIndex.Contains(eve.TargetItem.counter)))
			{
				return true;
			}
			break;
		}
		return false;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		list.Add(triggerCheckNode);
		if (chapterTriggerEffect != null)
		{
			list.Add(chapterTriggerEffect);
		}
		return list;
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.ChangeCounter;
		yield return GameEvent.Type.ChangeMaxCounter;
		yield return GameEvent.Type.CounterReachMaxCounter;
		yield return GameEvent.Type.TriggerChapterEffect;
	}
}
