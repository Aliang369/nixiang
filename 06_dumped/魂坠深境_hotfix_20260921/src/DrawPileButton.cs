using System;
using TMPro;
using UnityEngine;

public class DrawPileButton : UIBase
{
	[SerializeField]
	private GameButton button;

	[SerializeField]
	private TextMeshProUGUI counter;

	[SerializeField]
	private RectTransform flicker;

	[NonSerialized]
	private ConvertablePivot flickerPivot;

	[NonSerialized]
	public RecycleWastePileVFX recycleWastePileVFX;

	protected override void Init()
	{
		button.onClick.AddListener(_OnButtonClicked);
		flickerPivot = new ConvertablePivot(flicker);
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		int? num = UIBase.gameState.playerState.DrawPileContainer?.ItemsCount;
		if (recycleWastePileVFX != null)
		{
			num -= recycleWastePileVFX.currentCount;
		}
		counter.text = $"{num}";
	}

	public void PlayFlick()
	{
		SetDirty();
		UIBase.gameState.applicationState.nfxManager.SpawnAndPlay(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_drawPileButtonFlicker, flickerPivot.transform, flickerPivot.WorldPosition);
	}

	private void _OnButtonClicked()
	{
		UIBase.gameState.UIManager.drawPileArea.ShowPanel();
	}
}
