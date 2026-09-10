using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using Common.UI;
using FMODUnity;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.VFX;

public class NFXManager : GlobalGameManager, IFrameUpdate
{
	private const int MaxPoolSizePerPrefab = 4;

	private const int MaxTotalPoolSize = 48;

	public List<NFXPlayState> playingNfxs = new List<NFXPlayState>();

	[NonSerialized]
	private Dictionary<NFXSetup, Stack<NFXSetup>> pooledNfxs;

	[NonSerialized]
	private Dictionary<NFXSetup, bool> poolablePrefabs;

	[NonSerialized]
	private ApplicationStateInfo ownerApplicationState;

	[NonSerialized]
	private Transform poolRoot;

	[NonSerialized]
	private int pooledNfxCount;

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (playingNfxs == null)
		{
			playingNfxs = new List<NFXPlayState>();
		}
		if (pooledNfxs == null)
		{
			pooledNfxs = new Dictionary<NFXSetup, Stack<NFXSetup>>();
		}
		if (poolablePrefabs == null)
		{
			poolablePrefabs = new Dictionary<NFXSetup, bool>();
		}
		ownerApplicationState = applicationState;
	}

	public void FrameUpdate()
	{
		int num = playingNfxs.Count;
		int num2 = 0;
		while (num2 < num)
		{
			NFXPlayState nFXPlayState = playingNfxs[num2];
			if (nFXPlayState.IsCompleted())
			{
				playingNfxs.RemoveAt(num2);
				num--;
			}
			else
			{
				nFXPlayState.Update();
				num2++;
			}
		}
	}

	public NFXPlayState SpawnAndPlay(NFXSetup nfxPrefab, Transform pivot, bool destroyOnStop = true)
	{
		return SpawnAndPlay(nfxPrefab, pivot, () => pivot?.position, destroyOnStop);
	}

	public NFXPlayState SpawnAndPlay(NFXSetup nfxPrefab, Transform parent, Vector3 worldPosition, bool destroyOnStop = true)
	{
		return SpawnAndPlay(nfxPrefab, parent, () => worldPosition, destroyOnStop);
	}

	public NFXPlayState SpawnAndPlay(NFXSetup nfxPrefab, Transform parent, Func<Vector3?> worldPositionUpdater, bool destroyOnStop = true)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!((EventReference)(ref nfxPrefab.sound)).IsNull)
		{
			Bootstrap.instance.audioManager.PlayOneShot(nfxPrefab.sound);
		}
		if (nfxPrefab.cameraControlData != null)
		{
			nfxPrefab.cameraControlData.Run(base.applicationState, 1f);
		}
		bool flag = destroyOnStop && CanPool(nfxPrefab);
		NFXSetup nfxInstance = (flag ? TakeFromPool(nfxPrefab, parent) : UnityEngine.Object.Instantiate(nfxPrefab, parent));
		NFXPlayState nFXPlayState = new NFXPlayState
		{
			nfxInstance = nfxInstance,
			destroyOnStop = destroyOnStop,
			sourcePrefab = nfxPrefab,
			poolOwner = (flag ? this : null)
		};
		nFXPlayState.positionUpdater = worldPositionUpdater;
		nFXPlayState.applicationState = base.applicationState;
		nFXPlayState.nfxInstance.transform.position = worldPositionUpdater() ?? parent.position;
		nFXPlayState.Play();
		playingNfxs.Add(nFXPlayState);
		return nFXPlayState;
	}

	internal void Release(NFXSetup sourcePrefab, NFXSetup instance, VFXPlayState playState, float delay)
	{
		if (instance == null)
		{
			return;
		}
		if (delay > 0f && ownerApplicationState != null)
		{
			ownerApplicationState.StartCoroutine(() => ReleaseAfterDelay(sourcePrefab, instance, playState, delay));
		}
		else
		{
			ReleaseNow(sourcePrefab, instance, playState);
		}
	}

	private IEnumerator ReleaseAfterDelay(NFXSetup sourcePrefab, NFXSetup instance, VFXPlayState playState, float delay)
	{
		yield return new WaitForSeconds(delay);
		ReleaseNow(sourcePrefab, instance, playState);
	}

	private NFXSetup TakeFromPool(NFXSetup sourcePrefab, Transform parent)
	{
		EnsurePoolRoot();
		if (pooledNfxs.TryGetValue(sourcePrefab, out var value))
		{
			while (value.Count > 0)
			{
				NFXSetup nFXSetup = value.Pop();
				pooledNfxCount--;
				if (!(nFXSetup == null))
				{
					ResetRootTransform(nFXSetup, sourcePrefab, parent);
					return nFXSetup;
				}
			}
		}
		return UnityEngine.Object.Instantiate(sourcePrefab, parent);
	}

	private void ReleaseNow(NFXSetup sourcePrefab, NFXSetup instance, VFXPlayState playState)
	{
		if (instance == null)
		{
			return;
		}
		ResetPlayback(playState);
		if (sourcePrefab == null || !CanPool(sourcePrefab) || pooledNfxCount >= 48)
		{
			UnityEngine.Object.Destroy(instance.gameObject);
			return;
		}
		if (!pooledNfxs.TryGetValue(sourcePrefab, out var value))
		{
			value = new Stack<NFXSetup>();
			pooledNfxs.Add(sourcePrefab, value);
		}
		if (value.Count >= 4)
		{
			UnityEngine.Object.Destroy(instance.gameObject);
			return;
		}
		EnsurePoolRoot();
		instance.gameObject.SetActive(value: false);
		instance.transform.SetParent(poolRoot, worldPositionStays: false);
		value.Push(instance);
		pooledNfxCount++;
	}

	private bool CanPool(NFXSetup sourcePrefab)
	{
		if (sourcePrefab == null)
		{
			return false;
		}
		if (poolablePrefabs.TryGetValue(sourcePrefab, out var value))
		{
			return value;
		}
		bool flag = !sourcePrefab.IsLoopNFX;
		if (flag && ((UnityEngine.Object)(object)sourcePrefab.GetComponentInChildren<Animator>(includeInactive: true) != null || (UnityEngine.Object)(object)sourcePrefab.GetComponentInChildren<Animation>(includeInactive: true) != null || (UnityEngine.Object)(object)sourcePrefab.GetComponentInChildren<AudioSource>(includeInactive: true) != null || sourcePrefab.GetComponentInChildren<TrailRenderer>(includeInactive: true) != null || (UnityEngine.Object)(object)sourcePrefab.GetComponentInChildren<PlayableDirector>(includeInactive: true) != null || (UnityEngine.Object)(object)sourcePrefab.GetComponentInChildren<VisualEffect>(includeInactive: true) != null))
		{
			flag = false;
		}
		if (flag)
		{
			MonoBehaviour[] componentsInChildren = sourcePrefab.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
			foreach (MonoBehaviour monoBehaviour in componentsInChildren)
			{
				if (!(monoBehaviour is NFXSetup) && !(monoBehaviour is VFXObject))
				{
					string text = monoBehaviour.GetType().Namespace ?? string.Empty;
					if (!text.StartsWith("Coffee.UIExtensions", StringComparison.Ordinal) && !text.StartsWith("Spine.Unity", StringComparison.Ordinal) && !text.StartsWith("UnityEngine.UI", StringComparison.Ordinal))
					{
						flag = false;
						break;
					}
				}
			}
		}
		poolablePrefabs[sourcePrefab] = flag;
		return flag;
	}

	private void EnsurePoolRoot()
	{
		if (!(poolRoot != null))
		{
			pooledNfxs.Clear();
			pooledNfxCount = 0;
			GameObject gameObject = new GameObject("NFXPool");
			gameObject.transform.SetParent(Bootstrap.instance.transform, worldPositionStays: false);
			poolRoot = gameObject.transform;
		}
	}

	private static void ResetPlayback(VFXPlayState playState)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (playState == null)
		{
			return;
		}
		for (int i = 0; i < playState.uiParticles.Length; i++)
		{
			UIParticle obj = playState.uiParticles[i];
			if (obj != null)
			{
				obj.Stop();
			}
		}
		for (int j = 0; j < playState.particleSystems.Length; j++)
		{
			ParticleSystem val = playState.particleSystems[j];
			if (!((UnityEngine.Object)(object)val == null))
			{
				val.Stop(true, (ParticleSystemStopBehavior)0);
				if (playState.particleSystemLoops != null && j < playState.particleSystemLoops.Length)
				{
					MainModule main = val.main;
					((MainModule)(ref main)).loop = playState.particleSystemLoops[j];
				}
			}
		}
	}

	private static void ResetRootTransform(NFXSetup instance, NFXSetup sourcePrefab, Transform parent)
	{
		Transform transform = instance.transform;
		Transform transform2 = sourcePrefab.transform;
		transform.SetParent(parent, worldPositionStays: false);
		transform.localPosition = transform2.localPosition;
		transform.localRotation = transform2.localRotation;
		transform.localScale = transform2.localScale;
		if (transform is RectTransform rectTransform && transform2 is RectTransform rectTransform2)
		{
			rectTransform.anchorMin = rectTransform2.anchorMin;
			rectTransform.anchorMax = rectTransform2.anchorMax;
			rectTransform.anchoredPosition = rectTransform2.anchoredPosition;
			rectTransform.sizeDelta = rectTransform2.sizeDelta;
			rectTransform.pivot = rectTransform2.pivot;
		}
	}
}
