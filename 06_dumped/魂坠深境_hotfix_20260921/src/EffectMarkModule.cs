using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EffectMarkModule : ItemDisplayBase.DisplayModule
{
	[FormerlySerializedAs("effectMarkDisplay")]
	[SerializeField]
	private EffectMarkDisplay effectMarkDisplayPrefab;

	[SerializeField]
	private HorizontalLayoutGroup effectMarkDisplayParent;

	[NonSerialized]
	private CellUIPrefabPool<EffectMarkDisplay, EffectMarkAbility.EffectMarkInfo> effectMarkDisplayPool;

	[NonSerialized]
	private List<(EffectMarkNFX, NFXPlayState)> nfxs;

	public override void Init()
	{
		effectMarkDisplayPool = base.slotDisplay.GetPrefabPool<EffectMarkDisplay, EffectMarkAbility.EffectMarkInfo>(effectMarkDisplayPrefab, effectMarkDisplayParent.transform);
		nfxs = new List<(EffectMarkNFX, NFXPlayState)>();
	}

	public override void Unset()
	{
		DestroyAllEffectMarkNFX();
	}

	public override void UpdateDisplay()
	{
		if (UIDisplayModule.gameState != null)
		{
			CastInfoCache castInfoCache = UIDisplayModule.gameState.UIManager.playerItemArea.controlInfo.GetCastInfoCache(base.slotDisplay.item);
			effectMarkDisplayPool.Sync(castInfoCache.effectMarkInfos_Mask);
			if (base.slotDisplay.GetModule<PlayerTableDisplayStateModule>() != null)
			{
				SynchronizeNFX(castInfoCache.effectMarkInfos_Effect);
			}
			else
			{
				DestroyAllEffectMarkNFX();
			}
		}
	}

	public void AppendMaterial(List<Material> materials)
	{
		foreach (EffectMarkDisplay activeElement in effectMarkDisplayPool.GetActiveElements())
		{
			materials.Add(activeElement.icon.materialForRendering);
		}
	}

	private void DestroyAllEffectMarkNFX()
	{
		for (int i = 0; i < nfxs.Count; i++)
		{
			nfxs[i].Item2.Stop();
		}
		nfxs.Clear();
	}

	public void SynchronizeNFX(List<EffectMarkNFX> buffEffectSetup)
	{
		for (int i = 0; i < nfxs.Count; i++)
		{
			if (!buffEffectSetup.Contains(nfxs[i].Item1))
			{
				nfxs[i].Item2.Stop();
				nfxs.RemoveAt(i);
				i--;
			}
		}
		int j;
		for (j = 0; j < buffEffectSetup.Count; j++)
		{
			if (nfxs.FindIndex(((EffectMarkNFX, NFXPlayState) v) => v.Item1 == buffEffectSetup[j]) == -1)
			{
				nfxs.Add((buffEffectSetup[j], UIDisplayModule.applicationState.nfxManager.SpawnAndPlay(buffEffectSetup[j].setup, base.slotDisplay.transform, destroyOnStop: false)));
			}
		}
	}
}
