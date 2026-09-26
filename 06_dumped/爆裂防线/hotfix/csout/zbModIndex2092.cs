using System.Collections.Generic;

public class zbModIndex2092 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stMainHitToSubBuff stMainHitToSubBuff2 = new stMainHitToSubBuff
		{
			roll0to1 = modRecord.clientEff[0],
			subBuffId = (int)modRecord.clientEff[1],
			extraTime = modRecord.clientEff[2],
			skillType = modRecord.skillType[0]
		};
		if (!team.mainHitToSubBuffBySkill.TryGetValue(stMainHitToSubBuff2.skillType, out var value))
		{
			value = new List<stMainHitToSubBuff>(2);
			team.mainHitToSubBuffBySkill.Add(stMainHitToSubBuff2.skillType, value);
		}
		value.Add(stMainHitToSubBuff2);
	}
}
