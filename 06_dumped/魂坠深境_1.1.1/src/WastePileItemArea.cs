using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WastePileItemArea : PileItemArea
{
	private enum PileTab
	{
		Waste,
		Exile
	}

	[SerializeField]
	private Image backButtonImage;

	[SerializeField]
	private GameButton_Passive wastePileTabButton;

	[SerializeField]
	private GameButton_Passive exilePileTabButton;

	[SerializeField]
	private RectTransform basePanel;

	[SerializeField]
	private Image background;

	[NonSerialized]
	private Sequence sequence;

	[NonSerialized]
	private ItemContainer wastePileContainer;

	[NonSerialized]
	private ItemContainer exilePileContainer;

	[NonSerialized]
	private PileTab currentPileTab;

	[NonSerialized]
	private bool showing;

	protected override void Init()
	{
		base.Init();
		wastePileTabButton?.onClick.AddListener(_OnWastePileTabButtonClicked);
		exilePileTabButton?.onClick.AddListener(_OnExilePileTabButtonClicked);
		base.gameObject.SetActive(value: false);
		backButtonImage.color = backButtonImage.color.SetA(0f);
		backButtonImage.transform.rotation = Quaternion.Euler(Vector3.zero);
		basePanel.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		currentPileTab = PileTab.Waste;
		showing = false;
	}

	protected override void UpdateDisplay()
	{
		base.UpdateDisplay();
		RefreshPileTabButtons(instant: false);
	}

	public void Setup(ItemContainer wastePileContainer, ItemContainer exilePileContainer)
	{
		this.wastePileContainer = wastePileContainer;
		this.exilePileContainer = exilePileContainer;
		SelectPileTab(PileTab.Waste, instant: true);
	}

	public override void ShowPanel()
	{
		SelectPileTab(PileTab.Waste, instant: true);
		UIBase.gameState.StartCoroutine(OnShowPanel);
	}

	public override void HidePanel()
	{
		UIBase.gameState.StartCoroutine(OnHidePanel);
	}

	protected override void ShowInfo(Item item)
	{
		UIBase.gameState.UIManager.battleItemInfoPanel.Show(item, PivotManager.PivotType.PlayerWastePilePivot);
	}

	private void SelectPileTab(PileTab pileTab, bool instant)
	{
		currentPileTab = pileTab;
		Unselect();
		base.Setup(GetCurrentPileContainer());
		RefreshPileTabButtons(instant);
	}

	private ItemContainer GetCurrentPileContainer()
	{
		if (currentPileTab == PileTab.Exile)
		{
			return exilePileContainer;
		}
		return wastePileContainer;
	}

	private void RefreshPileTabButtons(bool instant)
	{
		wastePileTabButton?.StateTransition((currentPileTab == PileTab.Waste) ? ButtonSelectionState.Selected : ButtonSelectionState.Normal, instant);
		exilePileTabButton?.StateTransition((currentPileTab == PileTab.Exile) ? ButtonSelectionState.Selected : ButtonSelectionState.Normal, instant);
	}

	private void _OnWastePileTabButtonClicked()
	{
		SelectPileTab(PileTab.Waste, instant: false);
	}

	private void _OnExilePileTabButtonClicked()
	{
		SelectPileTab(PileTab.Exile, instant: false);
	}

	private IEnumerator OnShowPanel()
	{
		if (!showing)
		{
			showing = true;
			sequence?.Kill();
			sequence = DOTween.Sequence();
			sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
			base.gameObject.SetActive(value: true);
			SetDirty();
			sequence.Append(DOTweenModuleUI.DOFade(backButtonImage, 1f, 0.2f));
			sequence.Join(backButtonImage.transform.DORotate(new Vector3(0f, 0f, 20f), 0.2f));
			sequence.Join(basePanel.DOScale(1f, 0.2f));
			sequence.Join(DOTweenModuleUI.DOFade(background, 0.5f, 0.2f));
			Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
			});
			sequence.Play();
			yield return sequence.WaitForKill();
		}
	}

	private IEnumerator OnHidePanel()
	{
		if (showing)
		{
			showing = false;
			sequence?.Kill();
			sequence = DOTween.Sequence();
			sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
			base.gameObject.SetActive(value: true);
			sequence.Append(DOTweenModuleUI.DOFade(backButtonImage, 0f, 0.2f));
			sequence.Join(backButtonImage.transform.DORotate(Vector3.zero, 0.2f));
			sequence.Join(basePanel.DOScale(0.1f, 0.2f));
			sequence.Join(DOTweenModuleUI.DOFade(background, 0f, 0.2f));
			Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
				base.gameObject.SetActive(value: false);
			});
			sequence.Play();
			yield return sequence.WaitForKill();
		}
	}
}
