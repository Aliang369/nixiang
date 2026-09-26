using System.Collections.Generic;

public class zbModIndex1003 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		if (!team.mainHitToSubBuffBySkill.TryGetValue(modRecord.skillType[0], out var value))
		{
			value = new List<stMainHitToSubBuff>(2);
			team.mainHitToSubBuffBySkill.Add(modRecord.skillType[0], value);
		}
		int num = modRecord.clientEff.Length / 3;
		for (int i = 0; i < num; i++)
		{
			stMainHitToSubBuff stMainHitToSubBuff2 = new stMainHitToSubBuff
			{
				roll0to1 = modRecord.clientEff[i * 3],
				subBuffId = (int)modRecord.clientEff[i * 3 + 1],
				extraTime = modRecord.clientEff[i * 3 + 2],
				skillType = modRecord.skillType[0]
			};
			bool flag = false;
			foreach (stMainHitToSubBuff item in value)
			{
				if (stMainHitToSubBuff2.skillType == item.skillType && stMainHitToSubBuff2.subBuffId == item.subBuffId && stMainHitToSubBuff2.extraTime > item.extraTime)
				{
					item.extraTime = stMainHitToSubBuff2.extraTime;
					flag = true;
				}
			}
			if (!flag)
			{
				value.Add(stMainHitToSubBuff2);
			}
		}
	}
}
