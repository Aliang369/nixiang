using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnvironmentInfluenceDef : CTScriptableObject, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IAssetIcon
{
	[Serializable]
	public class EnvironmentRenderOverride
	{
		public bool enabled;

		public float baseColorMultiplier = 1f;

		public float roomAmbientMultiplier = 1f;

		public float roomAmbient2Multiplier = 1f;

		public float enterTransitionDuration;
	}

	public enum EnvironmentInfluenceType
	{
		Positive,
		Complex,
		Negative
	}

	[LocalizeText("环境效果名字")]
	[CTSOAssetName]
	public string environmentName;

	[LocalizeText("环境效果描述")]
	public string description;

	[LocalizeText("环境出现提示描述")]
	public string showDescription;

	[LocalizeText("环境消失提示描述")]
	public string hideDescription;

	[SerializeField]
	public Sprite icon;

	public EnvironmentInfluenceType type;

	public EnvironmentRenderOverride renderOverride = new EnvironmentRenderOverride();

	public GameObject spawnEffectPrefab;

	public List<Ability> abilities;

	public override string CTAssetIDPrepend => "ENVINF";

	public Sprite IconAsset => icon;

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	List<IEffectNode> IChildren<IEffectNode>.GetChildren()
	{
		return abilities.Cast<IEffectNode>().ToList();
	}
}
