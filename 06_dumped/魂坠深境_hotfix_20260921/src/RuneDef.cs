using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RuneDef : CTScriptableObject, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IAssetIcon
{
	public class AttachGenerationInfo
	{
		public int stack;

		public int weight;
	}

	public bool hideInIndex;

	[LocalizeText("印记名字(翻译时应当尽可能简短，最好只用1个单词，可以适当传神意译)")]
	[CTSOAssetName]
	public string Name;

	[LocalizeText("印记描述")]
	public string description;

	[SerializeField]
	public Sprite icon;

	public bool stacking = true;

	public QualityLevel quality;

	public AttachGenerationInfo attachGenerationInfo;

	public List<Ability> attachAbility;

	public ItemConstraint addConstraint;

	public IOutGameEffectNode addEffect;

	public IOutGameEffectNode changeEffect;

	public IOutGameEffectNode removeEffect;

	public PVPCompConfig pvpCompConfig;

	public override string CTAssetIDPrepend => "RUNE";

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

	public bool IsPVPComp => pvpCompConfig != null;

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

	public string GetDescription(ApplicationStateInfo applicationState)
	{
		TextDataGenerator generator;
		return TextDataPreset.FormatRuneDescription(applicationState, this, out generator);
	}

	public bool IsDiscovered(ApplicationStateInfo applicationState)
	{
		return applicationState.discoverManager.IsDiscovered(this);
	}

	public static int RuneDefIndexComparer(RuneDef a, RuneDef b, ApplicationStateInfo applicationState = null)
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

	public bool? CheckCanAddRuneTo(Item item)
	{
		if (!stacking && item.Runes.Any((Rune r) => r.def == this))
		{
			return false;
		}
		return CheckAddConstraint(item);
	}

	public bool? CheckAddConstraint(Item item)
	{
		if (addConstraint == null)
		{
			return true;
		}
		return addConstraint.CheckItemAccept(item);
	}
}
