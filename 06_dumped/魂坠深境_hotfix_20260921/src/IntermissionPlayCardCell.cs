using System.Collections;
using ChillyRoom.Services.ContainerOfSoul;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IntermissionPlayCardCell : CellUI<IntermissionPlayCardState>, IPointerClickHandler, IEventSystemHandler
{
	private const float FlipHalfDuration = 0.18f;

	[SerializeField]
	private GlobalItemDisplay rewardDisplay;

	[SerializeField]
	private Image cardBack;

	[SerializeField]
	private Image disabledOverlay;

	protected override void Init()
	{
		InitUI(rewardDisplay);
	}

	protected override void OnSetup(IntermissionPlayCardState state)
	{
		ShowCard(state.OpenedCard);
	}

	public IEnumerator PlayFlipCor(IntermissionPlayOpenedCardDto openedCard)
	{
		base.transform.DOKill();
		base.transform.localScale = Vector3.one;
		disabledOverlay.gameObject.SetActive(value: false);
		base.transform.DOScaleX(0f, 0.18f).SetEase(Ease.InQuad);
		yield return new WaitForSeconds(0.18f);
		ShowCard(openedCard);
		base.transform.DOScaleX(1f, 0.18f).SetEase(Ease.OutQuad);
		yield return new WaitForSeconds(0.18f);
		base.transform.localScale = Vector3.one;
	}

	private void ShowCard(IntermissionPlayOpenedCardDto openedCard)
	{
		bool flag = openedCard != null;
		cardBack.gameObject.SetActive(!flag);
		rewardDisplay.gameObject.SetActive(flag);
		if (flag)
		{
			rewardDisplay.Setup(openedCard.RewardView.ToDisplaySetup(CellUI<IntermissionPlayCardState>.applicationState));
		}
	}

	protected override void UpdateDisplay()
	{
		disabledOverlay.gameObject.SetActive(base.setupData.OpenedCard == null && !base.setupData.CanClick);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (base.setupData.CanClick)
		{
			CellUI<IntermissionPlayCardState>.applicationState.intermissionPlayManager.TryDrawCard(base.setupData.CardIndex);
		}
	}
}
