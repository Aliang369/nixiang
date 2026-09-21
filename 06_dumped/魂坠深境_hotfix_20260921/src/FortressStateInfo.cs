using System;
using System.Collections;
using System.Collections.Generic;
using ContainerOfSoul.Display.Fortress.DotManagers;
using DG.Tweening;
using UnityEngine;

public class FortressStateInfo : GlobalGameManager
{
	public FortressUIManager uiManager;

	public FortressRoomManager fortressRoomManager;

	public FortressRedDotManager redDotManager;

	public bool inited;

	public Sequence sequence;

	private readonly List<FortressGameManager> subManagers = new List<FortressGameManager>();

	public override void Init(ApplicationStateInfo applicationState)
	{
		RenderSettings.fogStartDistance = 50f;
		RenderSettings.fogEndDistance = 150f;
		InitSubManager(ref redDotManager);
		uiManager.Init();
		uiManager.gameObject.SetActive(value: true);
		InitSubManager(ref fortressRoomManager);
		inited = true;
	}

	public override void Dispose()
	{
		uiManager.Dispose();
	}

	public void FrameUpdate()
	{
		uiManager.FrameUpdate();
	}

	public IEnumerator<CustomYieldInstruction> InitCor(ApplicationStateInfo state)
	{
		Init(state);
		base.applicationState.eventManager.fortressInitFinishEvent.TriggerEvent();
		yield return new WaitUntil(() => BootstrapExtra.instance.loadingPanel.showing);
		Bootstrap.instance.adsManager.LoadAd("free_money");
	}

	private void InitSubManager<T>(ref T manager) where T : FortressGameManager, new()
	{
		if (manager == null)
		{
			manager = new T();
		}
		manager.Init(this);
		subManagers.Add(manager);
	}

	public WrapperEnumerator StartCoroutine(Func<IEnumerator> enumerator)
	{
		if (uiManager == null || !uiManager.gameObject.activeInHierarchy)
		{
			return null;
		}
		return WrapperEnumerator.StartCoroutine(enumerator, uiManager);
	}
}
