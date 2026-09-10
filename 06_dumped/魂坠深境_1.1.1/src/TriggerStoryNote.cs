using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriggerStoryNote : Effect
{
	[TextArea]
	[LocalizeText("信件文本")]
	public string note;

	[LocalizeText("信件签名")]
	public string signature;

	public NewItemGenerator itemGenerator;

	public SkillGenerator skillGenerator;

	public ClavisDef clavisDef;

	public bool gatherGet;

	public bool halt;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		StoryNoteInfo noteInfo = StoryNoteInfo.PlainNote(note, signature, itemGenerator?.GenerateNewItems(eve).FirstOrDefault(), skillGenerator?.GenerateSkills(eve).FirstOrDefault(), clavisDef, gatherGet);
		Effect.EnqueueAction(eve, new TriggerStoryNoteAction(noteInfo, halt));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (itemGenerator != null)
		{
			list.Add(itemGenerator);
		}
		if (skillGenerator != null)
		{
			list.Add(skillGenerator);
		}
		return list;
	}
}
