using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StartGameBuffDef : CTScriptableObject, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IAssetIcon
{
	[CTSOAssetName]
	[LocalizeText("加成名字")]
	public string buffName;

	public Sprite icon;

	[LocalizeText("加成描述")]
	public string description;

	[LocalizeText("加成类别名字")]
	public string className;

	public QualityLevel quality;

	public int consumeWisdom;

	public bool dailyShuffle;

	public List<Ability> abilities;

	public Effect activateEffect;

	public override string CTAssetIDPrepend => "GAMEBUFF";

	public Sprite IconAsset => icon;

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public new List<IEffectNode> GetChildren()
	{
		return ((IEnumerable<Ability>)abilities).Select((Func<Ability, IEffectNode>)((Ability x) => x)).Append(activateEffect).ToList();
	}
}
