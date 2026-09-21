using System;
using System.Collections;
using System.Collections.Generic;
using Common.UI;
using DG.Tweening;
using GameRandom;
using TMPro;
using UnityEngine;

public class ShellGamePanel : BonusLocationPanel.GamePanel
{
	private const int boldSelectCount = 2;

	private const float boldSwapInterval = 0.15f;

	private const int boldSwapCount = 20;

	private const int cautiousSelectCount = 1;

	private const float cautiousSwapInterval = 0.3f;

	private const int cautiousSwapCount = 10;

	[SerializeField]
	private GameButton defaultStart;

	[SerializeField]
	private GameButton boldStart;

	[SerializeField]
	private GameButton cautiousStart;

	[SerializeField]
	private TextMeshProUGUI centerText;

	[SerializeField]
	private BonusLocationBonusCell cellPrefab;

	[SerializeField]
	private List<Transform> cellPivots;

	[SerializeField]
	private GameButton switchPanelButton;

	[NonSerialized]
	private CellUIPrefabPool<BonusLocationBonusCell, BonusLocationBonus> cellPool;

	[NonSerialized]
	private bool finished;

	[NonSerialized]
	private int selectAbleCount;

	[NonSerialized]
	private List<BonusLocationBonusCell> selectedCells;

	[NonSerialized]
	private int swapCount;

	[NonSerialized]
	private bool swaped;

	[NonSerialized]
	private float swapInterval;

	protected override void Init()
	{
		base.Init();
		base.gameObject.SetActive(value: true);
		GetPrefabPool(ref cellPool, cellPrefab, base.transform, UIHelper.GetDefaultCommonInfoPanel());
		(parent as BonusLocationPanel).ConfigInfoPanel(cellPool);
		selectedCells = new List<BonusLocationBonusCell>();
		boldStart.onClick.AddListener(_OnBoldButtonClick);
		cautiousStart.onClick.AddListener(_OnCautiousButtonClick);
		defaultStart.onClick.AddListener(_OnCautiousButtonClick);
		switchPanelButton.onClick.AddListener(_OnSwitchPanelButtonClick);
	}

	public override void Setup(BonusLocationEvent bonusEvent)
	{
		base.Setup(bonusEvent);
		SpawnBonus();
		if (bonusEvent.gameSwitched || !UIBase.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EnableSwitchBonusLocationGame))
		{
			switchPanelButton.gameObject.SetActive(value: false);
		}
		base.gameObject.SetActive(value: true);
		selectAbleCount = 0;
		swapInterval = 0f;
		swapCount = 0;
		swaped = false;
		finished = false;
		centerText.text = "";
		boldStart.gameObject.SetActive(value: true);
		cautiousStart.gameObject.SetActive(value: true);
		if (UIBase.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EnableUpgradeBonusLocationGame))
		{
			boldStart.gameObject.SetActive(value: true);
			cautiousStart.gameObject.SetActive(value: true);
			defaultStart.gameObject.SetActive(value: false);
		}
		else
		{
			boldStart.gameObject.SetActive(value: false);
			cautiousStart.gameObject.SetActive(value: false);
			defaultStart.gameObject.SetActive(value: true);
		}
		SetDirty();
	}

	public override void Unset()
	{
		base.Unset();
		foreach (BonusLocationBonusCell activeElement in cellPool.GetActiveElements())
		{
			activeElement.onclick = (Action<BonusLocationBonusCell>)Delegate.Remove(activeElement.onclick, new Action<BonusLocationBonusCell>(_OnCellClicked));
		}
		cellPool.DespawnAll();
		selectedCells.Clear();
		SetDirty();
	}

	private IEnumerator SwapCor()
	{
		boldStart.gameObject.SetActive(value: false);
		cautiousStart.gameObject.SetActive(value: false);
		defaultStart.gameObject.SetActive(value: false);
		yield return new WaitForSeconds(0.2f);
		centerText.text = "3...";
		yield return new WaitForSeconds(0.4f);
		centerText.text = "2...";
		yield return new WaitForSeconds(0.4f);
		centerText.text = "1...";
		yield return new WaitForSeconds(0.4f);
		centerText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_start_exciting;
		yield return new WaitForSeconds(0.2f);
		centerText.text = "";
		foreach (BonusLocationBonusCell activeElement in cellPool.GetActiveElements())
		{
			activeElement.HideCell();
		}
		yield return new WaitForSeconds(0.1f);
		for (int i = 0; i < swapCount; i++)
		{
			List<Transform> list = cellPivots.SelectRandomCount(2, UIBase.gameState.randomManager.NextDouble);
			Transform child = list[0].GetChild(0);
			Transform child2 = list[1].GetChild(0);
			Vector3 position = child.position;
			Vector3 position2 = child2.position;
			float num = (1f - UIBase.gameState.randomManager.NextFloat() * 0.5f) * swapInterval;
			child.DOMove(position2, num);
			child2.DOMove(position, num);
			yield return new WaitForSeconds(num);
		}
		swaped = true;
		centerText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_shellGame_choose_bonus_arg0_per_arg1.FormatWith(selectedCells.Count, selectAbleCount);
		boldStart.gameObject.SetActive(value: false);
		cautiousStart.gameObject.SetActive(value: false);
		defaultStart.gameObject.SetActive(value: false);
		SetDirty();
	}

	private void SpawnBonus()
	{
		List<BonusLocationBonus> list = bonusEvent.GenerateRandomBonus();
		for (int i = 0; i < cellPivots.Count; i++)
		{
			BonusLocationBonusCell bonusLocationBonusCell = cellPool.Spawn(list[i], cellPivots[i]);
			bonusLocationBonusCell.onclick = (Action<BonusLocationBonusCell>)Delegate.Combine(bonusLocationBonusCell.onclick, new Action<BonusLocationBonusCell>(_OnCellClicked));
		}
	}

	private void _OnBoldButtonClick()
	{
		selectAbleCount = 2;
		swapInterval = 0.15f;
		swapCount = 20;
		StartCoroutine(SwapCor());
	}

	private void _OnCautiousButtonClick()
	{
		selectAbleCount = 1;
		swapInterval = 0.3f;
		swapCount = 10;
		UIBase.gameState.StartCoroutine(SwapCor);
	}

	private void _OnCellClicked(BonusLocationBonusCell cell)
	{
		if (swaped && !finished && !selectedCells.Contains(cell) && selectedCells.Count < selectAbleCount)
		{
			selectedCells.Add(cell);
			cell.ShowCell();
			cell.NotifyGet();
			SetDirty();
			if (selectedCells.Count >= selectAbleCount)
			{
				StartCoroutine(FinishCor());
			}
		}
	}

	private IEnumerator FinishCor()
	{
		finished = true;
		yield return new WaitForSeconds(1f);
		UIBase.gameState.UIManager.bonusLocationPanel.FinishBonusLocation(selectedCells.ConvertAll((BonusLocationBonusCell c) => c.bonus));
		Unset();
	}

	private void _OnSwitchPanelButtonClick()
	{
		bonusEvent.SwitchToRandomBonusPanel();
	}
}
