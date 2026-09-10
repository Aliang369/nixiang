using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillGem
{
	[SaveField(null, null)]
	public SkillGemDef def;

	[SaveField(null, null)]
	public int rotateIndex;

	[SaveField(null, null)]
	public Vector2Int offset;

	[SaveField(null, null)]
	public int amount;

	public int tempRotateIndex;

	public Vector2Int tempOffset;

	private SkillGem()
	{
	}

	public SkillGem(SkillGemDef def)
	{
		this.def = def;
		tempRotateIndex = (rotateIndex = 0);
		tempOffset = (offset = Vector2Int.zero);
		amount = 1;
	}

	public SkillGem(SkillGemDef def, int amount)
	{
		this.def = def;
		tempRotateIndex = (rotateIndex = 0);
		tempOffset = (offset = Vector2Int.zero);
		this.amount = amount;
	}

	public SkillGem(SkillGemDef def, int rotateIndex, Vector2Int offset, int amount)
	{
		this.def = def;
		tempRotateIndex = (this.rotateIndex = rotateIndex);
		tempOffset = (this.offset = offset);
		this.amount = amount;
	}

	public SkillGem(SkillGem skillGem)
	{
		def = skillGem.def;
		tempRotateIndex = (rotateIndex = skillGem.rotateIndex);
		tempOffset = (offset = skillGem.offset);
		amount = skillGem.amount;
	}

	public static List<SkillWord> GemToWords(IEnumerable<SkillGem> gems, GameSkill skill)
	{
		return GemToWords(gems.Select((SkillGem g) => g.def), skill);
	}

	public static List<SkillWord> GemToWords(IEnumerable<SkillGemDef> gems, GameSkill skill)
	{
		return (from s in gems.SelectMany((SkillGemDef g) => g.skillWordStacks)
			group s by s.skillWord into g
			select new SkillWord(g.Key, g.Sum((SkillWordStack skillWordStack) => skillWordStack.wordStack), skill)).ToList();
	}
}
