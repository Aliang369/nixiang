using System;
using System.Collections;
using System.Collections.Generic;
using Common.UI;
using DG.Tweening;
using GameRandom;
using TMPro;
using UnityEngine;

public class ChooseSequenceGamePanel : BonusLocationPanel.GamePanel
{
	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private GameButton nextButton;

	[SerializeField]
	private GameButton startButton;

	[SerializeField]
	private RectTransform spawnPivot;

	[SerializeField]
	private TextMeshProUGUI nextButtonText;

	[SerializeField]
	private TextMeshProUGUI remainChanceText;

	[SerializeField]
	private BonusLocationBonusCell cellPrefab;

	[SerializeField]
	private int replaceChance = 2;

	[SerializeField]
	private GameButton switchPanelButton;

	[NonSerialized]
	private Stack<BonusLocationBonusCell> bonusStack;

	[NonSerialized]
	private CellUIPrefabPool<BonusLocationBonusCell, BonusLocationBonus> cellPool;

	[NonSerialized]
	private int currentChance;

	[NonSerialized]
	private bool shuffled;

	[NonSerialized]
	private Sequence swapSeq;

	protected override void Init()
	{
		confirmButton.onClick.AddListener(_OnConfirmButtonClick);
		nextButton.onClick.AddListener(_OnNextButtonClick);
		startButton.onClick.AddListener(_OnStartButtonClicked);
		switchPanelButton.onClick.AddListener(_OnSwitchPanelButtonClick);
		GetPrefabPool(ref cellPool, cellPrefab, spawnPivot, UIHelper.GetDefaultCommonInfoPanel());
		(parent as BonusLocationPanel).ConfigInfoPanel(cellPool);
		shuffled = false;
	}

	public override void Setup(BonusLocationEvent bonusEvent)
	{
		base.Setup(bonusEvent);
		currentChance = replaceChance;
		if (UIBase.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EnableUpgradeBonusLocationGame))
		{
			currentChance++;
		}
		base.gameObject.SetActive(value: true);
		startButton.gameObject.SetActive(value: true);
		confirmButton.gameObject.SetActive(value: false);
		nextButton.gameObject.SetActive(value: false);
		confirmButton.interactable = true;
		nextButton.interactable = true;
		if (bonusEvent.gameSwitched || !UIBase.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EnableSwitchBonusLocationGame))
		{
			switchPanelButton.gameObject.SetActive(value: false);
		}
		List<BonusLocationBonus> list = bonusEvent.GenerateRandomBonus();
		for (int i = 0; i < list.Count; i++)
		{
			BonusLocationBonusCell bonusLocationBonusCell = cellPool.Spawn(list[i]);
			Rect rect = spawnPivot.rect;
			bonusLocationBonusCell.transform.localPosition = new Vector3(rect.x + rect.width * ((float)i / (float)(list.Count - 1)), rect.y + rect.height * 0.5f, 0f);
		}
		SetDirty();
	}

	public override void Unset()
	{
		base.Unset();
		cellPool.DespawnAll();
		bonusStack = null;
		shuffled = false;
	}

	protected override void UpdateDisplay()
	{
		nextButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_switch_another;
		remainChanceText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_remain_chance_count_arg0.FormatWith(currentChance);
	}

	private void _OnStartButtonClicked()
	{
		startButton.gameObject.SetActive(value: false);
		UIBase.gameState.StartCoroutine(ShuffleCor);
		IEnumerator ShuffleCor()
		{
			IReadOnlyList<BonusLocationBonusCell> cells = cellPool.GetActiveElements();
			foreach (BonusLocationBonusCell item in cells)
			{
				item.HideCell();
			}
			yield return new WaitForSeconds(0.1f);
			float num = 0.5f;
			foreach (BonusLocationBonusCell item2 in cells)
			{
				item2.transform.DOMove(spawnPivot.transform.position, num);
			}
			yield return new WaitForSeconds(num);
			List<BonusLocationBonusCell> shuffledCells = new List<BonusLocationBonusCell>(cells).Shuffle(UIBase.gameState.randomManager.NextDouble);
			foreach (BonusLocationBonusCell cell in cells)
			{
				Sequence sequence = DOTween.Sequence();
				Vector3 localPosition = cell.transform.localPosition;
				sequence.Append(cell.transform.DOLocalMoveX(localPosition.x + 100f, 0.1f));
				sequence.AppendCallback(delegate
				{
					cell.transform.SetSiblingIndex(shuffledCells.IndexOf(cell));
				});
				sequence.Append(cell.transform.DOLocalMoveX(localPosition.x, 0.1f));
				yield return sequence.Play().WaitForCompletion();
			}
			foreach (BonusLocationBonusCell item3 in shuffledCells)
			{
				item3.transform.SetAsLastSibling();
			}
			bonusStack = new Stack<BonusLocationBonusCell>();
			shuffledCells.ForEach(delegate(BonusLocationBonusCell c)
			{
				bonusStack.Push(c);
			});
			bonusStack.Peek().ShowCell();
			confirmButton.gameObject.SetActive(value: true);
			nextButton.gameObject.SetActive(value: true);
			shuffled = true;
			SetDirty();
		}
	}

	private void _OnConfirmButtonClick()
	{
		confirmButton.interactable = false;
		StartCoroutine(FinishCor());
		IEnumerator FinishCor()
		{
			yield return new WaitForSeconds(1f);
			UIBase.gameState.UIManager.bonusLocationPanel.FinishBonusLocation(new List<BonusLocationBonus> { bonusStack.Peek().bonus });
			Unset();
		}
	}

	private void _OnNextButtonClick()
	{
		if (currentChance <= 0 || swapSeq != null)
		{
			return;
		}
		currentChance--;
		swapSeq = DOTween.Sequence();
		swapSeq.AppendCallback(delegate
		{
			bonusStack.Peek().HideCell();
			nextButton.interactable = false;
		});
		swapSeq.AppendInterval(0.5f);
		swapSeq.AppendCallback(delegate
		{
			bonusStack.Peek().gameObject.SetActive(value: false);
			bonusStack.Pop();
			bonusStack.Peek().ShowCell();
			SetDirty();
			if (currentChance > 0)
			{
				nextButton.interactable = true;
			}
			swapSeq = null;
		});
		swapSeq.Play();
	}

	private void _OnSwitchPanelButtonClick()
	{
		bonusEvent.SwitchToRandomBonusPanel();
	}
}
