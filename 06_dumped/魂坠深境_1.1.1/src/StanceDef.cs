using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StanceDef : CTScriptableObject, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	[SaveField(null, null)]
	public Dictionary<string, object> tempValues;

	[CTSOAssetName]
	[LocalizeText("架势名称")]
	public string stanceName;

	public string stanceShortName;

	public Color stanceColor;

	public Sprite icon;

	public StanceManager.StanceType stanceTypeBinding;

	[LocalizeText("架势描述")]
	public string description;

	public List<Ability> abilities;

	public override string CTAssetIDPrepend => "STANCE";

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public new List<IEffectNode> GetChildren()
	{
		return ((IEnumerable<Ability>)abilities).Select((Func<Ability, IEffectNode>)((Ability a) => a)).ToList();
	}
}
