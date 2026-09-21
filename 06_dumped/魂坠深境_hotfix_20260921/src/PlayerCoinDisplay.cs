using System;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCoinDisplay : UIBase, PivotManager.IWithPivot
{
	[SerializeField]
	public Image image;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private GameButton button;

	protected override void Init()
	{
		UIBase.gameState.gameEventManager.onTriggerGameEvent += _OnCoinChange;
		text.text = UIBase.gameState.playerState.coinCount.ToString();
		button.onClick.AddListener(_OnClick);
		GameButton gameButton = button;
		gameButton.onHighlight = (Action)Delegate.Combine(gameButton.onHighlight, new Action(ShowHint));
		GameButton gameButton2 = button;
		gameButton2.onUnhighlight = (Action)Delegate.Combine(gameButton2.onUnhighlight, new Action(HideHint));
	}

	protected override void Dispose()
	{
		UIBase.gameState.gameEventManager.onTriggerGameEvent -= _OnCoinChange;
	}

	private void ShowHint()
	{
		if (!PlatformHelper.IsMobile())
		{
			UIBase.gameState.UIManager.hudPanel.coinCrystalHintPanel.ShowCommonInfo(new CommonInfoData(CoinCrystalHintPanel.Type.Coin, CommonInfoData.ControlType.Hover, null, false));
		}
	}

	private void HideHint()
	{
		if (!PlatformHelper.IsMobile())
		{
			UIBase.gameState.UIManager.hudPanel.coinCrystalHintPanel.HideCommonInfo(CoinCrystalHintPanel.Type.Coin, CommonInfoData.ControlType.Hover);
		}
	}

	private void _OnCoinChange(GameEvent eve)
	{
		if (eve.type == GameEvent.Type.ChangeCoin)
		{
			Sequence sequence = DOTween.Sequence();
			sequence.Append(text.transform.DOScale(2f, 0.2f));
			sequence.Append(text.transform.DOScale(1f, 0.2f));
			sequence.Play();
			text.text = UIBase.gameState.playerState.coinCount.ToString();
			if (base.gameObject.activeInHierarchy)
			{
				Vector2 anchoredPosition = UIBase.gameState.UIManager.uiVFXRoot.WorldPositionToAnchored(GetPivot(UIBase.gameState).ToCanvasWorldPosition(UIBase.gameState.UIManager.mainCanvas));
				ScriptableObjectSingleton<VFXDataPreset>.instance.coinPopupTextPrefab.SpawnGUI(UIBase.gameState.UIManager.uiVFXRoot, anchoredPosition, eve.deltaValue).positiveSign = true;
			}
		}
	}

	private void _OnClick()
	{
		UIBase.gameState.UIManager.hudPanel.coinCrystalHintPanel.ShowCommonInfo(new CommonInfoData(CoinCrystalHintPanel.Type.Coin, CommonInfoData.ControlType.Click, null, false));
	}

	public ConvertablePivot GetPivot(GameStateInfo gameState)
	{
		return new ConvertablePivot(base.transform, GetComponentInParent<Canvas>());
	}
}
