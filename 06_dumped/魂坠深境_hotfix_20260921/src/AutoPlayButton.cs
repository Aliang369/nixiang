using System;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public class AutoPlayButton : UIBase, IFrameUpdate
{
	[SerializeField]
	private GameButton button;

	[SerializeField]
	private GameObject autoEndTurnLabel;

	[NonSerialized]
	private BattleAutoPlayState displayedState;

	[NonSerialized]
	private AutoPlayButtonMode displayedMode;

	[NonSerialized]
	private Graphic[] buttonGraphics;

	[NonSerialized]
	private GameObject outline;

	[NonSerialized]
	private Image outlineImage;

	[NonSerialized]
	private Material outlineMaterial;

	[NonSerialized]
	private bool refreshAfterAction;

	protected override void Init()
	{
		outline = base.transform.Find("Outline")?.gameObject;
		outlineImage = outline?.GetComponent<Image>();
		if (outlineImage != null)
		{
			outlineImage.color = Color.white;
			outlineMaterial = UnityEngine.Object.Instantiate(outlineImage.material);
			outlineImage.material = outlineMaterial;
		}
		button.onClick.AddListener(OnButtonClicked);
		Transform labelTransform = autoEndTurnLabel.transform;
		buttonGraphics = Array.FindAll(button.GetComponentsInChildren<Graphic>(includeInactive: true), (Graphic graphic) => graphic.transform != labelTransform && !graphic.transform.IsChildOf(labelTransform));
		displayedState = (BattleAutoPlayState)(-1);
		displayedMode = (AutoPlayButtonMode)(-1);
		UIBase.gameState.actionManager.actionQueueCleanEvent += OnActionQueueClean;
		SetDirty();
	}

	protected override void Dispose()
	{
		UIBase.gameState.actionManager.actionQueueCleanEvent -= OnActionQueueClean;
		if (outlineMaterial != null)
		{
			UnityEngine.Object.Destroy(outlineMaterial);
		}
	}

	protected override void UpdateDisplay()
	{
		BattleAutoPlayState autoPlayState = UIBase.gameState.battleState.autoPlayState;
		AutoPlayButtonMode autoPlayButtonMode = Bootstrap.instance.globalSaveConfigData.autoPlayButtonMode;
		if (displayedMode != autoPlayButtonMode)
		{
			displayedMode = autoPlayButtonMode;
			bool flag = autoPlayButtonMode != AutoPlayButtonMode.Disabled;
			Graphic[] array = buttonGraphics;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = flag;
			}
			autoEndTurnLabel.SetActive(autoPlayButtonMode == AutoPlayButtonMode.EnabledAndEndTurn);
		}
		if (autoPlayState != displayedState)
		{
			displayedState = autoPlayState;
			button.interactable = autoPlayButtonMode != AutoPlayButtonMode.Disabled && autoPlayState == BattleAutoPlayState.Ready;
			outline?.SetActive(autoPlayState == BattleAutoPlayState.Playing);
			if (outlineMaterial != null)
			{
				outlineMaterial.SetColor("_Color", Color.yellow);
			}
		}
	}

	public void FrameUpdate()
	{
		if (refreshAfterAction || UIBase.gameState.battleState.autoPlayState != BattleAutoPlayState.Ready)
		{
			refreshAfterAction = false;
			UIBase.gameState.battleState.RefreshAutoPlayState();
		}
		AutoPlayButtonMode autoPlayButtonMode = Bootstrap.instance.globalSaveConfigData.autoPlayButtonMode;
		if (displayedState != UIBase.gameState.battleState.autoPlayState || displayedMode != autoPlayButtonMode)
		{
			SetDirty();
		}
	}

	private void OnActionQueueClean()
	{
		refreshAfterAction = true;
	}

	private void OnButtonClicked()
	{
		if (UIBase.gameState.battleState.TryStartAutoPlay())
		{
			SetDirty();
		}
	}
}
