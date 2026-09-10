using System;
using Coffee.UIExtensions;
using Spine.Unity;
using UnityEngine;

public class NFXPlayState
{
	public NFXSetup nfxInstance;

	[NonSerialized]
	public VFXPlayState vfxPlayState;

	[NonSerialized]
	public bool destroyOnStop = true;

	[NonSerialized]
	public bool started;

	[NonSerialized]
	public Func<Vector3?> positionUpdater;

	[NonSerialized]
	public ApplicationStateInfo applicationState;

	[NonSerialized]
	public NFXSetup sourcePrefab;

	[NonSerialized]
	public NFXManager poolOwner;

	[NonSerialized]
	private float? lifetime;

	public bool IsCompleted()
	{
		return nfxInstance == null;
	}

	public void Stop(float delay = 0f)
	{
		if (nfxInstance != null)
		{
			NFXSetup nFXSetup = nfxInstance;
			nfxInstance = null;
			if (poolOwner != null)
			{
				poolOwner.Release(sourcePrefab, nFXSetup, vfxPlayState, delay);
			}
			else
			{
				UnityEngine.Object.Destroy(nFXSetup.gameObject, delay);
			}
		}
	}

	public void Update()
	{
		if (positionUpdater != null)
		{
			nfxInstance.transform.position = positionUpdater() ?? nfxInstance.transform.position;
		}
		if (lifetime.HasValue)
		{
			lifetime = lifetime.Value - Time.deltaTime;
			if (lifetime.Value <= 0f)
			{
				Stop();
				return;
			}
		}
		VFXPlayState vFXPlayState = vfxPlayState;
		if (vFXPlayState != null && vFXPlayState.destroyOnStop && started && CheckVFXFinished())
		{
			Stop();
		}
	}

	public void KeepAliveFor(float duration)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (duration <= 0f)
		{
			return;
		}
		lifetime = duration;
		if (vfxPlayState?.particleSystems == null)
		{
			return;
		}
		ParticleSystem[] particleSystems = vfxPlayState.particleSystems;
		foreach (ParticleSystem val in particleSystems)
		{
			if (((Component)(object)val).gameObject.activeInHierarchy)
			{
				MainModule main = val.main;
				((MainModule)(ref main)).loop = true;
				if (!val.isPlaying)
				{
					val.Play(true);
				}
			}
		}
	}

	public VFXPlayState Play()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		started = true;
		nfxInstance.gameObject.SetActive(value: true);
		RestoreSourceActiveStates();
		vfxPlayState = new VFXPlayState();
		vfxPlayState.particleSystems = nfxInstance.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		vfxPlayState.particleSystemLoops = new bool[vfxPlayState.particleSystems.Length];
		for (int i = 0; i < vfxPlayState.particleSystems.Length; i++)
		{
			bool[] particleSystemLoops = vfxPlayState.particleSystemLoops;
			int num = i;
			MainModule main = vfxPlayState.particleSystems[i].main;
			particleSystemLoops[num] = ((MainModule)(ref main)).loop;
		}
		vfxPlayState.uiParticles = nfxInstance.GetComponentsInChildren<UIParticle>();
		vfxPlayState.skeletonAnimations = nfxInstance.GetComponentsInChildren<SkeletonAnimation>();
		vfxPlayState.skeletonGraphics = nfxInstance.GetComponentsInChildren<SkeletonGraphic>();
		vfxPlayState.destroyOnStop = destroyOnStop;
		for (int j = 0; j < vfxPlayState.uiParticles.Length; j++)
		{
			if (vfxPlayState.uiParticles[j].isPaused)
			{
				vfxPlayState.uiParticles[j].Play();
			}
		}
		if (vfxPlayState.uiParticles.Length == 0)
		{
			for (int k = 0; k < vfxPlayState.particleSystems.Length; k++)
			{
				if (((Component)(object)vfxPlayState.particleSystems[k]).gameObject.activeInHierarchy && !vfxPlayState.particleSystems[k].isPlaying)
				{
					vfxPlayState.particleSystems[k].Play();
				}
			}
		}
		for (int l = 0; l < vfxPlayState.skeletonAnimations.Length; l++)
		{
			((SkeletonRenderer)vfxPlayState.skeletonAnimations[l]).Initialize(true, false);
			vfxPlayState.skeletonAnimations[l].AnimationState.SetAnimation(0, vfxPlayState.skeletonAnimations[l].AnimationName, loop: false);
		}
		for (int m = 0; m < vfxPlayState.skeletonGraphics.Length; m++)
		{
			vfxPlayState.skeletonGraphics[m].Initialize(true);
			vfxPlayState.skeletonGraphics[m].Skeleton.SetToSetupPose();
			vfxPlayState.skeletonGraphics[m].AnimationState.ClearTracks();
			vfxPlayState.skeletonGraphics[m].AnimationState.SetAnimation(0, vfxPlayState.skeletonGraphics[m].startingAnimation, loop: false);
		}
		return vfxPlayState;
	}

	private void RestoreSourceActiveStates()
	{
		Transform[] componentsInChildren = sourcePrefab.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			Transform transform = componentsInChildren[i];
			string relativePath = GetRelativePath(sourcePrefab.transform, transform);
			nfxInstance.transform.Find(relativePath).gameObject.SetActive(transform.gameObject.activeSelf);
		}
	}

	private static string GetRelativePath(Transform root, Transform target)
	{
		string text = target.name;
		while (target.parent != root)
		{
			target = target.parent;
			text = target.name + "/" + text;
		}
		return text;
	}

	public bool CheckVFXFinished()
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < vfxPlayState.skeletonAnimations.Length; i++)
		{
			if (!vfxPlayState.skeletonAnimations[i].AnimationState.GetCurrent(0).IsComplete)
			{
				return false;
			}
		}
		for (int j = 0; j < vfxPlayState.skeletonGraphics.Length; j++)
		{
			if (!vfxPlayState.skeletonGraphics[j].AnimationState.GetCurrent(0).IsComplete)
			{
				return false;
			}
		}
		for (int k = 0; k < vfxPlayState.particleSystems.Length; k++)
		{
			MainModule main = vfxPlayState.particleSystems[k].main;
			if (!((MainModule)(ref main)).loop && !vfxPlayState.particleSystems[k].isStopped)
			{
				return false;
			}
		}
		return true;
	}
}
