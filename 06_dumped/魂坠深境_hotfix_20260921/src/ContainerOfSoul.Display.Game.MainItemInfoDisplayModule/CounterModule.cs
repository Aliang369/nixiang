using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.MainItemInfoDisplayModule;

public class CounterModule : MainItemInfoDisplayBase.DisplayModule, IFrameUpdate
{
	[SerializeField]
	private Image counterImage;

	[SerializeField]
	private TextMeshProUGUI counterText;

	[NonSerialized]
	private bool showCounter;

	[NonSerialized]
	private bool isUpgradeModify;

	[NonSerialized]
	private Color color;

	public override void Setup()
	{
		if (base.item == null)
		{
			return;
		}
		showCounter = base.item.withCounter;
		counterImage.gameObject.SetActive(showCounter);
		if (!showCounter)
		{
			return;
		}
		counterImage.sprite = base.item.GetCounterImageLarge();
		color = base.item.GetCounterTextColor();
		counterText.color = color;
		counterText.text = base.item.GetSpriteCounterText();
		if (base.mainItemInfoDisplay.isUpgradeDisplay)
		{
			Item obj = new Item(base.item.GetDef()).WithSkin(base.item.skin);
			Item item = base.item;
			ProtectedInt counter = obj.counter;
			ProtectedInt counter2 = item.counter;
			isUpgradeModify = (int)counter != (int)counter2;
			if (isUpgradeModify)
			{
				RefreshColor();
			}
		}
	}

	void IFrameUpdate.FrameUpdate()
	{
		RefreshColor();
	}

	private void RefreshColor()
	{
		if (!base.gameObject.activeInHierarchy || !isUpgradeModify || !showCounter || counterText.textInfo.meshInfo == null)
		{
			return;
		}
		for (int i = 0; i < counterText.textInfo.meshInfo.Length; i++)
		{
			if (counterText.textInfo.meshInfo[i].colors32 != null)
			{
				for (int j = 0; j < counterText.textInfo.meshInfo[i].colors32.Length; j++)
				{
					counterText.textInfo.meshInfo[i].colors32[j] = TextmeshproEffect.EffectParam.instance.GetColor(color);
				}
			}
		}
		counterText.UpdateVertexData();
	}
}
