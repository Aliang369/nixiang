using System.Collections.Generic;
using UnityEngine;

public class TalentDef : CTScriptableObject, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IAssetIcon
{
	public enum TalentType
	{
		PermanentUpgrade,
		ActivatePotential,
		MasteryUnlock,
		Guidance
	}

	public Sprite icon;

	[CTSOAssetName]
	[LocalizeText("天赋名称")]
	public string talentName;

	[LocalizeText("天赋实际显示名称")]
	public string talentDisplayName;

	public TalentType talentType;

	[LocalizeText("天赋效果描述")]
	[TextArea(2, 3)]
	public string effectDescription;

	public CharacterClassDef attachCharacter;

	public GameConfigEffect node;

	public List<Effect> displayOnlyEffect;

	public override string CTAssetIDPrepend => "TAL";

	public string TalentTypeName => ScriptableObjectSingleton<VFXDataPreset>.instance.talentTypeDisplayInfos[talentType].typeName;

	public string TalentTypeDescription => ScriptableObjectSingleton<VFXDataPreset>.instance.talentTypeDisplayInfos[talentType].typeDescription;

	public bool IsPotentialTalent => node is ActivatePotential;

	public Sprite IconAsset => icon;

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo();
	}

	public new List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		list.Add(node);
		list.AddRange(displayOnlyEffect.OrEmptyIfNull());
		return list;
	}

	public Sprite GetDisplayIcon(ApplicationStateInfo applicationState, PlayerCharacterState character)
	{
		return node.GenerateTalentDisplayIcon(character, this);
	}

	public string GetDisplayName(ApplicationStateInfo applicationState, PlayerCharacterState character)
	{
		return node.GenerateTalentDisplayName(character, this);
	}

	public string GetDisplayDescription(ApplicationStateInfo applicationState, PlayerCharacterState character, out TextDataGenerator generator)
	{
		return node.GenerateTalentDisplayDescription(out generator, character, this);
	}
}
