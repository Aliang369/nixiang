using System;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

public static class zbCombatSpineAssetUtil
{
	private const string kCacheKeyPrefix = "cg:";

	private static readonly Dictionary<string, SkeletonDataAsset> s_cache = new Dictionary<string, SkeletonDataAsset>();

	public static SkeletonDataAsset Resolve(string spinePath)
	{
		if (string.IsNullOrEmpty(spinePath) || zbAssetLoader.Instance == null)
		{
			return null;
		}
		if (s_cache.TryGetValue(spinePath, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		SkeletonDataAsset val = zbCombatAtlasMerger.Instance.GetEffectMerged(spinePath);
		if ((Object)(object)val == (Object)null)
		{
			val = zbCombatAtlasMerger.Instance.GetMerged(spinePath);
		}
		if ((Object)(object)val == (Object)null)
		{
			val = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(spinePath);
		}
		if ((Object)(object)val != (Object)null)
		{
			s_cache[spinePath] = val;
		}
		return val;
	}

	public static void RefreshSpine(SkeletonAnimation skeletonAnimation, string spinePath, string animaiton = "", bool loop = false)
	{
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			string text = "cg:" + spinePath;
			if (skeletonAnimation.userCacheAssetName != text)
			{
				skeletonAnimation.userCacheAssetName = text;
				SkeletonDataAsset val = Resolve(spinePath);
				if ((Object)(object)val == (Object)null)
				{
					return;
				}
				if ((Object)(object)((SkeletonRenderer)skeletonAnimation).skeletonDataAsset != (Object)(object)val || !((SkeletonRenderer)skeletonAnimation).valid)
				{
					MeshFilter component = ((Component)skeletonAnimation).GetComponent<MeshFilter>();
					if ((Object)(object)component != (Object)null)
					{
						component.sharedMesh = null;
					}
					((SkeletonRenderer)skeletonAnimation).skeletonDataAsset = val;
					((SkeletonRenderer)skeletonAnimation).Initialize(true, false);
				}
				if (animaiton.Length > 0)
				{
					try
					{
						skeletonAnimation.AnimationState.SetAnimation(0, animaiton, loop);
					}
					catch (Exception)
					{
					}
				}
			}
			else if (animaiton.Length > 0 && ((SkeletonRenderer)skeletonAnimation).valid && skeletonAnimation.AnimationState != null)
			{
				TrackEntry current = skeletonAnimation.AnimationState.GetCurrent(0);
				if (current == null || current.Animation.Name != animaiton)
				{
					try
					{
						skeletonAnimation.AnimationState.SetAnimation(0, animaiton, loop);
					}
					catch (Exception)
					{
					}
				}
			}
		}
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			MeshRenderer component2 = ((Component)skeletonAnimation).GetComponent<MeshRenderer>();
			if ((Object)(object)component2 != (Object)null && !((Renderer)component2).enabled && (Object)(object)((Component)skeletonAnimation).GetComponent<SkeletonRenderSeparator>() == (Object)null)
			{
				((Renderer)component2).enabled = true;
			}
		}
	}

	public static void Clear()
	{
		s_cache.Clear();
	}
}
