using System;
using TMPro;
using UnityEngine;

public class WastePileButton : UIBase
{
	[SerializeField]
	private GameButton button;

	[SerializeField]
	private TextMeshProUGUI counter;

	[SerializeField]
	private RectTransform flicker;

	[NonSerialized]
	private ConvertablePivot flickerPivot;

	protected override void Init()
	{
		flickerPivot = new ConvertablePivot(flicker);
		button.onClick.AddListener(_OnButtonClicked);
		UpdateDisplay();
	}

	protected override void UpdateDisplay()
	{
		counter.text = $"{UIBase.gameState.playerState.WastePileContainer?.ItemsCount}";
	}

	public void PlayFlick()
	{
		UIBase.gameState.applicationState.nfxManager.SpawnAndPlay(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_wastePileButtonFlicker, flickerPivot.transform, flickerPivot.WorldPosition);
	}

	private void _OnButtonClicked()
	{
		UIBase.gameState.UIManager.wastePileArea.ShowPanel();
	}
}
