using System;
using Common.UI;
using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Game.MainItemInfoDisplayModule;

public class YinYangModule : MainItemInfoDisplayBase.DisplayModule, IFrameUpdate
{
	[SerializeField]
	private RectTransform yinYangPivot;

	[SerializeField]
	private YinYangAddDisplay yinAddDisplayPrefab;

	[SerializeField]
	private YinYangAddDisplay yangAddDisplayPrefab;

	[SerializeField]
	private YinYangConsumeDisplay yinConsumeDisplayPrefab;

	[SerializeField]
	private YinYangConsumeDisplay yangConsumeDisplayPrefab;

	[SerializeField]
	private YinYangConvertDisplay yinConvertDisplayPrefab;

	[SerializeField]
	private YinYangConvertDisplay yangConvertDisplayPrefab;

	private YinYangAddDisplay yinAddDisplayLazy;

	private YinYangAddDisplay yangAddDisplayLazy;

	private YinYangConsumeDisplay yinConsumeDisplayLazy;

	private YinYangConsumeDisplay yangConsumeDisplayLazy;

	private YinYangConvertDisplay yinConvertDisplayLazy;

	private YinYangConvertDisplay yangConvertDisplayLazy;

	[NonSerialized]
	private YinYangFeatureAbility.DisplayInfos displayInfo;

	[NonSerialized]
	private bool upgradeModified;

	private bool isBattleDisplay => base.mainItemInfoDisplay.itemInfoDisplay.isBattleDisplay;

	public override void Init()
	{
		yinAddDisplayLazy = UnityEngine.Object.Instantiate(yinAddDisplayPrefab, yinYangPivot).GetComponent<YinYangAddDisplay>();
		yangAddDisplayLazy = UnityEngine.Object.Instantiate(yangAddDisplayPrefab, yinYangPivot).GetComponent<YinYangAddDisplay>();
		yinConsumeDisplayLazy = UnityEngine.Object.Instantiate(yinConsumeDisplayPrefab, yinYangPivot).GetComponent<YinYangConsumeDisplay>();
		yangConsumeDisplayLazy = UnityEngine.Object.Instantiate(yangConsumeDisplayPrefab, yinYangPivot).GetComponent<YinYangConsumeDisplay>();
		yinConvertDisplayLazy = UnityEngine.Object.Instantiate(yinConvertDisplayPrefab, yinYangPivot).GetComponent<YinYangConvertDisplay>();
		yangConvertDisplayLazy = UnityEngine.Object.Instantiate(yangConvertDisplayPrefab, yinYangPivot).GetComponent<YinYangConvertDisplay>();
	}

	public override void Setup()
	{
		upgradeModified = false;
		yinAddDisplayLazy.gameObject.SetActive(value: false);
		yangAddDisplayLazy.gameObject.SetActive(value: false);
		yinConsumeDisplayLazy.gameObject.SetActive(value: false);
		yangConsumeDisplayLazy.gameObject.SetActive(value: false);
		yinConvertDisplayLazy.gameObject.SetActive(value: false);
		yangConvertDisplayLazy.gameObject.SetActive(value: false);
		YinYangFeatureAbility yinYangDisplayModule = YinYangFeatureAbility.GetYinYangDisplayModule(base.item);
		if (yinYangDisplayModule == null)
		{
			displayInfo = null;
			return;
		}
		displayInfo = yinYangDisplayModule.GetDisplayInfo(base.item, UIDisplayModule.gameState);
		upgradeModified = yinYangDisplayModule.upgradeModified;
		if (displayInfo.yinAddInfo != null)
		{
			yinAddDisplayLazy.gameObject.SetActive(displayInfo.yinAddInfo != null);
		}
		if (displayInfo.yangAddInfo != null)
		{
			yangAddDisplayLazy.gameObject.SetActive(displayInfo.yangAddInfo != null);
		}
		if (displayInfo.yinConsumeInfo != null)
		{
			yinConsumeDisplayLazy.gameObject.SetActive(displayInfo.yinConsumeInfo != null);
		}
		if (displayInfo.yangConsumeInfo != null)
		{
			yangConsumeDisplayLazy.gameObject.SetActive(displayInfo.yangConsumeInfo != null);
		}
		if (displayInfo.yinConvertInfo != null)
		{
			yinConvertDisplayLazy.gameObject.SetActive(displayInfo.yinConvertInfo != null);
		}
		if (displayInfo.yangConvertInfo != null)
		{
			yangConvertDisplayLazy.gameObject.SetActive(displayInfo.yangConvertInfo != null);
		}
	}

	public override void Unset()
	{
		displayInfo = null;
	}

	public override void UpdateDisplay()
	{
		if (displayInfo != null)
		{
			if (displayInfo.yinAddInfo != null)
			{
				yinAddDisplayLazy.text.text = displayInfo.yinAddInfo.GetText(UIDisplayModule.gameState, base.item, isBattleDisplay);
			}
			if (displayInfo.yangAddInfo != null)
			{
				yangAddDisplayLazy.text.text = displayInfo.yangAddInfo.GetText(UIDisplayModule.gameState, base.item, isBattleDisplay);
			}
			if (displayInfo.yinConsumeInfo != null)
			{
				yinConsumeDisplayLazy.text.text = displayInfo.yinConsumeInfo.GetText(UIDisplayModule.gameState, base.item, isBattleDisplay);
			}
			if (displayInfo.yangConsumeInfo != null)
			{
				yangConsumeDisplayLazy.text.text = displayInfo.yangConsumeInfo.GetText(UIDisplayModule.gameState, base.item, isBattleDisplay);
			}
			if (displayInfo.yinConvertInfo != null)
			{
				yinConvertDisplayLazy.text.text = displayInfo.yinConvertInfo.GetText(UIDisplayModule.gameState, base.item, isBattleDisplay);
			}
			if (displayInfo.yangConvertInfo != null)
			{
				yangConvertDisplayLazy.text.text = displayInfo.yangConvertInfo.GetText(UIDisplayModule.gameState, base.item, isBattleDisplay);
			}
			RefreshColor();
		}
	}

	void IFrameUpdate.FrameUpdate()
	{
		RefreshColor();
	}

	private void RefreshColor()
	{
		if (base.gameObject.activeInHierarchy && upgradeModified && base.mainItemInfoDisplay.isUpgradeDisplay)
		{
			if (displayInfo.yinAddInfo != null)
			{
				RefreshColor(yinAddDisplayLazy.text);
			}
			if (displayInfo.yangAddInfo != null)
			{
				RefreshColor(yangAddDisplayLazy.text);
			}
			if (displayInfo.yinConsumeInfo != null)
			{
				RefreshColor(yinConsumeDisplayLazy.text);
			}
			if (displayInfo.yangConsumeInfo != null)
			{
				RefreshColor(yangConsumeDisplayLazy.text);
			}
			if (displayInfo.yinConvertInfo != null)
			{
				RefreshColor(yinConvertDisplayLazy.text);
			}
			if (displayInfo.yangConvertInfo != null)
			{
				RefreshColor(yangConvertDisplayLazy.text);
			}
		}
	}

	private void RefreshColor(TextMeshProUGUI text)
	{
		if (text.textInfo.meshInfo == null)
		{
			return;
		}
		for (int i = 0; i < text.textInfo.meshInfo.Length; i++)
		{
			if (text.textInfo.meshInfo[i].colors32 != null)
			{
				for (int j = 0; j < text.textInfo.meshInfo[i].colors32.Length; j++)
				{
					text.textInfo.meshInfo[i].colors32[j] = TextmeshproEffect.EffectParam.instance.GetColor(text.color);
				}
			}
		}
		text.UpdateVertexData();
	}
}
