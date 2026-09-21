using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class EnchantDef : CTScriptableObject, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IAssetIcon
{
	public enum EnchantType
	{
		None,
		Positive,
		Negative
	}

	public bool hideInIndex;

	[LocalizeText("附魔名字(翻译时应当尽可能简短，最好只用1个单词，可以适当传神意译，附魔可以用形容词来翻译)")]
	[CTSOAssetName]
	public string Name;

	[LocalizeText("附魔描述")]
	public string description;

	[SerializeField]
	private Sprite icon;

	public Color displayColor = new Color(0.85f, 0.85f, 0.85f, 1f);

	public bool stacking = true;

	public EnchantType type;

	public bool displayOverrideCostAsMainCost;

	public ItemSpell overrideSpell;

	public ItemConstraint addConstraint;

	public List<Ability> attachAbility;

	public Effect addEffect;

	public Effect changeEffect;

	public Effect removeEffect;

	public EventReference addSoundOverride;

	public NFXSetup onAddNFX;

	public override string CTAssetIDPrepend => "EC";

	public string NameWithColor => Name.ColorText(displayColor);

	public Sprite Icon
	{
		get
		{
			if (!(icon != null))
			{
				return ScriptableObjectSingleton<GameDataManager>.instance.enchantPlaceHolderIcon;
			}
			return icon;
		}
	}

	public Sprite IconAsset => icon;

	List<IEffectNode> IChildren<IEffectNode>.GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (attachAbility != null)
		{
			foreach (Ability item in attachAbility)
			{
				list.Add(item);
			}
		}
		list.Add(addEffect);
		list.Add(removeEffect);
		list.Add(changeEffect);
		list.Add(addConstraint);
		return list;
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public bool IsDiscovered(ApplicationStateInfo applicationState)
	{
		return applicationState.discoverManager.IsDiscovered(this);
	}
}
