using System;
using DamageNumbersPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupTextAction : ExecuteAction
{
	private PivotManager.IWithPivot pivot;

	public Vector3? worldPosition;

	public string text;

	public int number;

	public DamageNumberGUI dnPrefab;

	public Func<string, string> customNumberConverter;

	public bool positiveSign;

	public Color? manualSetColor;

	public Sprite popupIcon;

	public PopupTextAction(PivotManager.IWithPivot pivot, string text, DamageNumberGUI damageNumbersPrefab)
	{
		this.text = text;
		this.pivot = pivot;
		dnPrefab = damageNumbersPrefab;
	}

	public PopupTextAction(PivotManager.IWithPivot pivot, int number, DamageNumberGUI damageNumbersPrefab, Func<string, string> customNumberConverter = null, bool positiveSign = false)
	{
		this.number = number;
		this.pivot = pivot;
		dnPrefab = damageNumbersPrefab;
		this.customNumberConverter = customNumberConverter;
		this.positiveSign = positiveSign;
	}

	private void TrySetItemDisplayPopupPosition()
	{
		if (pivot != null)
		{
			ConvertablePivot convertablePivot = pivot.GetPivot(gameState);
			worldPosition = ((convertablePivot != null) ? new Vector3?(convertablePivot.ToCanvasWorldPosition(gameState.UIManager.mainCanvas)) : worldPosition);
		}
	}

	protected override void Update()
	{
		TrySetItemDisplayPopupPosition();
		InstantiatePopupText();
	}

	private void InstantiatePopupText()
	{
		if (worldPosition.HasValue)
		{
			Vector3 vector = gameState.UIManager.uiVFXRoot.WorldPositionToAnchored(worldPosition.Value);
			DamageNumber damageNumber = null;
			damageNumber = ((text == null) ? dnPrefab.SpawnGUI(gameState.UIManager.uiVFXRoot, vector, number) : dnPrefab.SpawnGUI(gameState.UIManager.uiVFXRoot, vector, text));
			Image componentInChildren = damageNumber.GetComponentInChildren<Image>(includeInactive: true);
			if (componentInChildren != null)
			{
				componentInChildren.gameObject.SetActive(popupIcon != null);
				componentInChildren.sprite = popupIcon;
			}
			damageNumber.customNumberConverter = customNumberConverter;
			damageNumber.positiveSign = positiveSign;
			if (manualSetColor.HasValue)
			{
				damageNumber.SetColor(manualSetColor.Value);
			}
		}
	}
}
