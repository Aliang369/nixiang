using System;
using System.Collections.Generic;
using System.Linq;

public class SkillQualityIndexEntry : ICTAsset, IChildren<ICTAsset>
{
	private readonly SkillDef skillDef;

	private readonly QualityLevel qualityLevel;

	private readonly int archetypeIndex;

	public SkillDef SkillDef => skillDef;

	public QualityLevel QualityLevel => qualityLevel;

	public int ArchetypeIndex => archetypeIndex;

	public SkillDef.SkillArchetype SkillArchetype => skillDef?.skillArchetypes?.ElementAtOrDefault(archetypeIndex);

	public int AssetIndex
	{
		get
		{
			return ((ICTAsset)skillDef).AssetIndex * 10 + GetQualityOrdinal(qualityLevel);
		}
		set
		{
		}
	}

	public string AssetGuid
	{
		get
		{
			return $"{((ICTAsset)skillDef).AssetGuid}:{qualityLevel}";
		}
		set
		{
		}
	}

	public string CTAssetIDPrepend => "SKQ";

	public Type CTAssetType => typeof(SkillQualityIndexEntry);

	public SkillQualityIndexEntry(SkillDef skillDef, int archetypeIndex)
	{
		this.skillDef = skillDef;
		this.archetypeIndex = archetypeIndex;
		qualityLevel = skillDef.skillArchetypes[archetypeIndex].qualityLevel;
	}

	public bool IsValidAsset()
	{
		if (skillDef != null && skillDef.IsValidAsset())
		{
			return SkillArchetype != null;
		}
		return false;
	}

	public List<ICTAsset> GetChildren()
	{
		return null;
	}

	public ProfileSkill ToProfileSkill()
	{
		return new ProfileSkill(skillDef, archetypeIndex);
	}

	public static int GetQualityOrdinal(QualityLevel qualityLevel)
	{
		return qualityLevel switch
		{
			QualityLevel.Base => 1, 
			QualityLevel.Common => 2, 
			QualityLevel.Uncommon => 3, 
			QualityLevel.Rare => 4, 
			QualityLevel.Epic => 5, 
			_ => 0, 
		};
	}
}
