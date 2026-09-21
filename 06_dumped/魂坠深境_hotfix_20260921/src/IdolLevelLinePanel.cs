using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using UnityEngine;

public class IdolLevelLinePanel : CellUI<IdolState>
{
	[SerializeField]
	private CellUIPrefabPoolConfig<IdolLevelCell, int> pool;

	[SerializeField]
	private WheelScrollRect wheelScrollRect;

	[SerializeField]
	private float quickScrollSpeed;

	[SerializeField]
	private float upgradeScrollSpeed;

	[SerializeField]
	public float upgradeAnimTime;

	[NonSerialized]
	public bool isPlayingAnim;

	[NonSerialized]
	private Coroutine coroutine;

	protected override bool shouldUpdate
	{
		get
		{
			if (!isPlayingAnim)
			{
				return base.shouldUpdate;
			}
			return false;
		}
	}

	private int currentLevel => idolState.currentLevel;

	public IdolState idolState => base.setupData;

	private int maxLevel => idolState.idolDef.maxLevel;

	protected override void Init()
	{
		Init(pool);
	}

	protected override void OnSetup(IdolState setupData)
	{
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
		}
		base.gameObject.SetActive(value: true);
		SetDirty();
		List<int> list = new List<int>();
		for (int i = 0; i <= maxLevel + 1; i++)
		{
			list.Add(i);
		}
		isPlayingAnim = false;
		pool.DespawnAll();
		pool.Sync(list);
		pool.pool.Order((List<IdolLevelCell> cs) => cs.OrderBy((IdolLevelCell c) => -c.setupData).ToList());
		StartCoroutine(SetScrollRect());
		IEnumerator SetScrollRect()
		{
			yield return null;
			wheelScrollRect.SnapToElementInstant(pool.pool.GetActiveElement(GetFocusLevel(currentLevel)).rectTransform);
		}
	}

	public void PlayUpgradeAnim(int startLevel, GlobalRewardPackage pack)
	{
		isPlayingAnim = true;
		coroutine = StartCoroutine(Cor());
		IEnumerator Cor()
		{
			yield return new WaitForSeconds(0.2f);
			bool snapping = false;
			IdolLevelCell to = pool.pool.GetActiveElement(startLevel + 1);
			to.UpgradeAnimStart();
			wheelScrollRect.SnapToElement(pool.pool.GetActiveElement(GetFocusLevel(startLevel)).rectTransform, quickScrollSpeed, delegate
			{
				snapping = true;
			});
			yield return new WaitUntil(() => snapping);
			to.UpgradeAnim();
			snapping = false;
			wheelScrollRect.SnapToElement(pool.pool.GetActiveElement(GetFocusLevel(startLevel + 1)).rectTransform, upgradeScrollSpeed, delegate
			{
				snapping = true;
			});
			yield return new WaitUntil(() => snapping);
			isPlayingAnim = false;
			coroutine = null;
			if (CellUI<IdolState>.fortressState != null)
			{
				CellUI<IdolState>.fortressState.StartCoroutine(() => CellUI<IdolState>.fortressState.uiManager.globalRewardPackageDisplay.ShowNestedRewardPackSeperateCor(pack));
			}
			SetDirty();
		}
	}

	public void SnapToUpgradeEndInstant(int targetLevel)
	{
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
			coroutine = null;
		}
		isPlayingAnim = false;
		if (pool?.pool != null && !(wheelScrollRect == null) && targetLevel >= 0 && targetLevel < pool.pool.activeCount)
		{
			IdolLevelCell activeElement = pool.pool.GetActiveElement(targetLevel);
			if (!(activeElement == null))
			{
				wheelScrollRect.SnapToElementInstant(activeElement.rectTransform);
				SetDirty();
			}
		}
	}

	private int GetFocusLevel(int level)
	{
		return Mathf.Clamp(level + 1, 0, maxLevel + 1);
	}
}
