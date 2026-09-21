using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IndexDisplay : CellUI<ItemDisplayBase>
{
	[SerializeField]
	private TextMeshProUGUI indexText;

	[SerializeField]
	private Image background;

	[NonSerialized]
	public ItemDisplayBase itemDisplay;

	[NonSerialized]
	private PlayerOverloadDisplay overloadDisplay;

	[NonSerialized]
	private PlayerTableDisplayStateModule stateModule;

	protected override bool shouldUpdate => parent.gameObject.activeInHierarchy;

	protected override void Init()
	{
		overloadDisplay = parent as PlayerOverloadDisplay;
	}

	protected override void OnSetup(ItemDisplayBase dailyContract)
	{
		itemDisplay = dailyContract;
		stateModule = dailyContract.GetModule<PlayerTableDisplayStateModule>();
	}

	protected override void OnUnset()
	{
		itemDisplay = null;
		stateModule = null;
	}

	protected override void UpdateDisplay()
	{
		if (stateModule == null)
		{
			overloadDisplay.SetDirty();
			return;
		}
		PlayerOverloadDisplay.Config playerOverloadDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.playerOverloadDisplayConfig;
		if (stateModule.currentState.IsAny(PlayerTableDisplayStateModule.State.Added, PlayerTableDisplayStateModule.State.UnDraw) || (stateModule.currentState == PlayerTableDisplayStateModule.State.PlayingMutualExcludeAnim && stateModule.currentAnimType.IsAny(PlayerTableDisplayStateModule.AnimType.PlayTween_AddNew, PlayerTableDisplayStateModule.AnimType.PlayTween_Exile, PlayerTableDisplayStateModule.AnimType.PlayTween_Remove)))
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		int previewIndex = GetPreviewIndex(itemDisplay.item);
		if (previewIndex != -1)
		{
			base.gameObject.SetActive(value: true);
			indexText.text = $"{previewIndex + 1}";
			Color color = ((previewIndex >= overloadDisplay.displayInfo.overload) ? playerOverloadDisplayConfig.overFlowColor : playerOverloadDisplayConfig.stayColor);
			indexText.color = color;
			background.color = color;
		}
	}

	public void RefreshIndexDisplay()
	{
		if (!(stateModule == null) && !stateModule.currentState.IsAny(PlayerTableDisplayStateModule.State.Added, PlayerTableDisplayStateModule.State.Selecting) && (stateModule.currentState != PlayerTableDisplayStateModule.State.PlayingMutualExcludeAnim || stateModule.currentAnimType != PlayerTableDisplayStateModule.AnimType.PlayTween_Remove))
		{
			Item item = itemDisplay.item;
			if (item != null && item.owner?.type == ItemContainer.ContainerType.Table)
			{
				base.transform.position = itemDisplay.transform.position;
				RectTransform obj = base.rectTransform;
				float? y = 0f;
				obj.SetAnchoredPosition(null, y);
			}
		}
	}

	private int GetPreviewIndex(Item item)
	{
		if (item.owner == null || item.owner.type != ItemContainer.ContainerType.Table)
		{
			return -1;
		}
		return item.owner.GetRawIndex(item);
	}
}
