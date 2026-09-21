using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "影响词条", menuName = "影响词条", order = 0)]
public class InfluenceDef : CTScriptableObject, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IAssetIcon
{
	public enum InfluenceType
	{
		Positive,
		Neutral,
		Negative
	}

	[SerializeField]
	public Sprite icon;

	[SerializeField]
	public Sprite mapIcon;

	[LocalizeText("祝福名字(翻译时应当尽可能简短，最好不超过3个单词，可以适当传神意译)")]
	[CTSOAssetName]
	public string InfluenceName;

	[LocalizeText("祝福描述")]
	[TextArea]
	public string description;

	public bool enableInfluenceValue;

	public int initValue;

	public InfluenceType type;

	public QualityLevel quality;

	[SerializeField]
	public PlayerCharacterDef attachCharacter;

	[SerializeField]
	public CharacterClassDef attachCharacterClass;

	public bool randomGenerate;

	public bool rootBonfireBlessPool;

	public bool hideInIndex;

	public bool retainSingleDiscoverRewardWhenHidden;

	public List<Ability> abilities;

	public PVPCompConfig pvpCompConfig;

	public override string CTAssetIDPrepend => "INF";

	public Sprite Icon
	{
		get
		{
			if (!(icon != null))
			{
				return ScriptableObjectSingleton<GameDataManager>.instance.influencePlaceHolderIcon;
			}
			return icon;
		}
	}

	public Sprite MapIcon
	{
		get
		{
			if (!(mapIcon != null))
			{
				return Icon;
			}
			return mapIcon;
		}
	}

	public bool IsPVPComp => pvpCompConfig != null;

	public int InGameCrystalCost => quality switch
	{
		QualityLevel.Base => 3, 
		QualityLevel.Common => 5, 
		QualityLevel.Uncommon => 8, 
		QualityLevel.Rare => 10, 
		_ => 9, 
	};

	public Sprite IconAsset => icon;

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	List<IEffectNode> IChildren<IEffectNode>.GetChildren()
	{
		return abilities.Cast<IEffectNode>().ToList();
	}

	public string GetInfluenceDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return TextDataPreset.FormatInfluenceDescription(this, applicationState, out generator);
	}

	public string GetNameWithDescription(ApplicationStateInfo applicationState)
	{
		TextDataGenerator generator;
		return InfluenceName + "：" + GetInfluenceDescription(applicationState, out generator);
	}

	public string GetNameWithQualityColor()
	{
		return InfluenceName.ColorText(ScriptableObjectSingleton<GameDataManager>.instance.GetQualityBaseColor(quality));
	}

	public bool IsDiscovered(ApplicationStateInfo applicationState)
	{
		return applicationState.discoverManager.IsDiscovered(this);
	}

	public static int InfluenceDefIndexComparer(InfluenceDef a, InfluenceDef b, ApplicationStateInfo applicationState = null)
	{
		if (applicationState != null)
		{
			int num = applicationState.globalUnlockManager.IsUnlocked(b).CompareTo(applicationState.globalUnlockManager.IsUnlocked(a));
			if (num != 0)
			{
				return num;
			}
			int num2 = b.IsDiscovered(applicationState).CompareTo(a.IsDiscovered(applicationState));
			if (num2 != 0)
			{
				return num2;
			}
		}
		int num3 = a.quality.CompareTo(b.quality);
		if (num3 != 0)
		{
			return num3;
		}
		return a.SOindex.CompareTo(b.SOindex);
	}
}
