using System;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCrystalDisplay : UIBase, PivotManager.IWithPivot
{
	[SerializeField]
	public Image image;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private GameButton button;

	protected override void Init()
	{
		UIBase.gameState.gameEventManager.onTriggerGameEvent += _OnCrystalChange;
		text.text = UIBase.gameState.playerState.crystalCount.ToString();
		button.onClick.AddListener(_OnClick);
		GameButton gameButton = button;
		gameButton.onHighlight = (Action)Delegate.Combine(gameButton.onHighlight, new Action(ShowHint));
		GameButton gameButton2 = button;
		gameButton2.onUnhighlight = (Action)Delegate.Combine(gameButton2.onUnhighlight, new Action(HideHint));
	}

	protected override void Dispose()
	{
		UIBase.gameState.gameEventManager.onTriggerGameEvent -= _OnCrystalChange;
	}

	private void ShowHint()
	{
		if (!PlatformHelper.IsMobile())
		{
			UIBase.gameState.UIManager.hudPanel.coinCrystalHintPanel.ShowCommonInfo(new CommonInfoData(CoinCrystalHintPanel.Type.Crystal, CommonInfoData.ControlType.Hover, null, false));
		}
	}

	private void HideHint()
	{
		if (!PlatformHelper.IsMobile())
		{
			UIBase.gameState.UIManager.hudPanel.coinCrystalHintPanel.HideCommonInfo(CoinCrystalHintPanel.Type.Crystal, CommonInfoData.ControlType.Hover);
		}
	}

	private void _OnCrystalChange(GameEvent eve)
	{
		if (eve.type == GameEvent.Type.ChangeCrystal)
		{
			Sequence sequence = DOTween.Sequence();
			sequence.Append(text.transform.DOScale(2f, 0.2f));
			sequence.Append(text.transform.DOScale(1f, 0.2f));
			sequence.Play();
			text.text = UIBase.gameState.playerState.crystalCount.ToString();
			if (base.gameObject.activeInHierarchy)
			{
				Vector2 anchoredPosition = UIBase.gameState.UIManager.uiVFXRoot.WorldPositionToAnchored(GetPivot(UIBase.gameState).ToCanvasWorldPosition(UIBase.gameState.UIManager.mainCanvas));
				ScriptableObjectSingleton<VFXDataPreset>.instance.crystalPopupTextPrefab.SpawnGUI(UIBase.gameState.UIManager.uiVFXRoot, anchoredPosition, eve.deltaValue).positiveSign = true;
			}
		}
	}

	private void _OnClick()
	{
		UIBase.gameState.UIManager.hudPanel.coinCrystalHintPanel.ShowCommonInfo(new CommonInfoData(CoinCrystalHintPanel.Type.Crystal, CommonInfoData.ControlType.Click, null, false));
	}

	public ConvertablePivot GetPivot(GameStateInfo gameState)
	{
		return new ConvertablePivot(base.transform, GetComponentInParent<Canvas>());
	}
}
