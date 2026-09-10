using System;
using System.Collections.Generic;
using Common.UI;
using StringMatch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellTextModule : MainItemInfoDisplayBase.DisplayModule, IFrameUpdate
{
	public class UpgradeTextGenerator
	{
		private class FlashInfo
		{
			public int endIndex;

			public int startIndex;
		}

		private class Block
		{
			public int endIndex;

			public int l;

			public bool marked;

			public int startIndex;

			public bool InBlock(int index)
			{
				if (index >= startIndex)
				{
					return index < endIndex;
				}
				return false;
			}
		}

		public SpellTextModule after;

		public SpellTextModule before;

		private bool inited;

		private List<FlashInfo> spellIndexes = new List<FlashInfo>();

		private List<FlashInfo> passiveIndexes = new List<FlashInfo>();

		private List<FlashInfo> updatedSpellIndexes = new List<FlashInfo>();

		private List<FlashInfo> updatedPassiveIndexes = new List<FlashInfo>();

		private string spellText => before.mainSpellText.text;

		private string passiveText => before.mainPassiveText.text;

		private string upgradedSpellText => after.mainSpellText.text;

		private string upgradedPassiveText => after.mainPassiveText.text;

		public bool valid
		{
			get
			{
				if (before != null && after != null)
				{
					return after.mainItemInfoDisplay.isUpgradeDisplay;
				}
				return false;
			}
		}

		public void SetupText()
		{
			Clear();
			if (before != null)
			{
				before.SetSpellText();
				before.SetPassiveText();
			}
			if (before != null)
			{
				after.SetSpellText();
				after.SetPassiveText();
			}
			if (valid)
			{
				SetupSpellText();
				SetupPassiveText();
				inited = true;
			}
		}

		private void SetupSpellText()
		{
			if (spellText != null && upgradedSpellText != null)
			{
				try
				{
					SetupText(spellText, upgradedSpellText, spellIndexes, updatedSpellIndexes);
				}
				catch
				{
				}
			}
		}

		private void SetupPassiveText()
		{
			if (passiveText != null && upgradedPassiveText != null)
			{
				try
				{
					SetupText(passiveText, upgradedPassiveText, passiveIndexes, updatedPassiveIndexes);
				}
				catch
				{
				}
			}
		}

		private void SetupText(string origin, string updated, List<FlashInfo> originIndexes, List<FlashInfo> upgradeIndexes)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Invalid comparison between Unknown and I4
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Invalid comparison between Unknown and I4
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Invalid comparison between Unknown and I4
			List<Diff> list = new diff_match_patch().diff_main(origin, updated);
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if ((int)list[i].operation == 2)
				{
					num += list[i].text.Length;
				}
				else if ((int)list[i].operation == 0)
				{
					if (list.Count > i + 2 && (int)list[i + 1].operation == 1)
					{
						string text = list[i + 1].text;
						upgradeIndexes.Add(new FlashInfo
						{
							startIndex = num,
							endIndex = num + text.Length
						});
						num += text.Length;
						i++;
					}
					else
					{
						string text2 = list[i].text;
						originIndexes.Add(new FlashInfo
						{
							startIndex = num,
							endIndex = num + text2.Length
						});
					}
				}
				else if ((int)list[i].operation == 1)
				{
					string text3 = list[i].text;
					upgradeIndexes.Add(new FlashInfo
					{
						startIndex = num,
						endIndex = num + text3.Length
					});
					num += text3.Length;
				}
			}
			List<Block> blocks = GetBlocks(origin);
			List<Block> blocks2 = GetBlocks(updated);
			int j;
			for (j = 0; j < originIndexes.Count; j++)
			{
				if (originIndexes[j] == null || blocks == null)
				{
					continue;
				}
				Block block = blocks.Find((Block b) => b.InBlock(originIndexes[j].startIndex));
				if (block != null && block.InBlock(originIndexes[j].endIndex))
				{
					if (block.marked)
					{
						originIndexes[j] = null;
						continue;
					}
					block.marked = true;
					originIndexes[j].startIndex = block.startIndex;
					originIndexes[j].endIndex = block.endIndex + 7;
				}
			}
			int i2;
			for (i2 = 0; i2 < upgradeIndexes.Count; i2++)
			{
				if (upgradeIndexes[i2] != null && blocks2 != null)
				{
					Block block2 = blocks2.Find((Block b) => b.InBlock(upgradeIndexes[i2].startIndex));
					if (block2 != null && block2.InBlock(upgradeIndexes[i2].endIndex))
					{
						upgradeIndexes[i2].startIndex = block2.startIndex;
						upgradeIndexes[i2].endIndex = block2.endIndex + 7;
					}
				}
			}
		}

		private List<Block> GetBlocks(string str)
		{
			List<Block> list = new List<Block>();
			if (str.IsNullOrEmpty())
			{
				return list;
			}
			int[] array = str.AllIndexesOf("<nobr>");
			int[] array2 = str.AllIndexesOf("</nobr>");
			int[] array3 = new int[str.Length];
			int j = 0;
			for (int i = 0; i < array3.Length; i++)
			{
				if (array.Contains(i))
				{
					j++;
					list.Add(new Block
					{
						startIndex = i,
						endIndex = -1,
						l = j
					});
				}
				else if (array2.Contains(i))
				{
					Block block = list.Find((Block b) => b.l == j && b.endIndex == -1);
					if (block != null)
					{
						block.endIndex = i;
					}
					j--;
				}
				array3[i] = j;
			}
			return list;
		}

		public void Clear()
		{
			inited = false;
			spellIndexes.Clear();
			passiveIndexes.Clear();
			updatedSpellIndexes.Clear();
			updatedPassiveIndexes.Clear();
		}

		public void UpdateText(Item item)
		{
			if (inited)
			{
				if (item == before.item)
				{
					UpdateText(before.mainSpellText, spellIndexes);
					UpdateText(before.mainPassiveText, passiveIndexes);
				}
				if (item == after.item)
				{
					UpdateText(after.mainSpellText, updatedSpellIndexes);
					UpdateText(after.mainPassiveText, updatedPassiveIndexes);
				}
			}
		}

		private void UpdateText(TextMeshProUGUI text, List<FlashInfo> indexes)
		{
			if (indexes == null)
			{
				return;
			}
			foreach (FlashInfo index in indexes)
			{
				if (index == null)
				{
					continue;
				}
				int characterInfo = GetCharacterInfo(text.textInfo, index.startIndex);
				int characterInfo2 = GetCharacterInfo(text.textInfo, index.endIndex);
				for (int i = characterInfo; i < characterInfo2; i++)
				{
					if (text.textInfo.characterInfo[i].isVisible && text.textInfo.characterInfo[i].elementType == TMP_TextElementType.Character)
					{
						for (int j = 0; j < 4; j++)
						{
							text.textInfo.meshInfo[text.textInfo.characterInfo[i].materialReferenceIndex].colors32[text.textInfo.characterInfo[i].vertexIndex + j] = TextmeshproEffect.EffectParam.instance.GetColor(text.textInfo.characterInfo[i].color);
						}
					}
				}
			}
			text.UpdateVertexData();
		}

		private int GetCharacterInfo(TMP_TextInfo textInfo, int index)
		{
			for (int i = 0; i < textInfo.characterCount && i < textInfo.characterInfo.Length; i++)
			{
				if (textInfo.characterInfo[i].index >= index)
				{
					return i;
				}
			}
			return textInfo.characterCount;
		}
	}

	[SerializeField]
	private Image mainSpellBackground;

	[SerializeField]
	private TextMeshProUGUI mainSpellText;

	[SerializeField]
	private TextMeshProUGUI mainPassiveText;

	[NonSerialized]
	private TextDataGenerator passiveGenerator;

	[NonSerialized]
	private TextDataGenerator spellGenerator;

	[NonSerialized]
	private UpgradeTextGenerator upgradeTextGenerator;

	[NonSerialized]
	private TextLinkObjectDisplayer spellTextLinkObjectDisplayer;

	[NonSerialized]
	private TextLinkObjectDisplayer passiveTextLinkObjectDisplayer;

	public override void Init()
	{
		spellTextLinkObjectDisplayer = new ItemTextLinkObjectDisplayer(UIDisplayModule.applicationState, mainSpellText);
		passiveTextLinkObjectDisplayer = new ItemTextLinkObjectDisplayer(UIDisplayModule.applicationState, mainPassiveText);
	}

	public override void Setup()
	{
		upgradeTextGenerator = base.mainItemInfoDisplay.itemInfoDisplay.upgradeTextGenerator;
		upgradeTextGenerator.Clear();
	}

	public override void Unset()
	{
		base.mainItemInfoDisplay.itemInfoDisplay.upgradeTextGenerator.after = null;
		base.mainItemInfoDisplay.itemInfoDisplay.upgradeTextGenerator.before = null;
	}

	public override void UpdateDisplay()
	{
		mainSpellBackground.sprite = base.item.GetSpellTextBackgroundImage();
		if (upgradeTextGenerator.valid)
		{
			upgradeTextGenerator.SetupText();
		}
		else
		{
			SetSpellText();
			SetPassiveText();
		}
		string text = mainPassiveText.text;
		bool flag = text != null && text.Length > 0;
		mainPassiveText.gameObject.SetActive(flag);
		text = mainSpellText.text;
		bool flag2 = text != null && text.Length > 0;
		mainSpellText.gameObject.SetActive(flag2);
		if (flag2 & flag)
		{
			mainSpellBackground.rectTransform.SetHeight(115f);
		}
		else if (flag2)
		{
			mainSpellBackground.rectTransform.SetHeight(210f);
		}
		else
		{
			mainSpellBackground.rectTransform.SetHeight(0f);
		}
	}

	void IFrameUpdate.FrameUpdate()
	{
		RefreshColor();
	}

	private void RefreshColor()
	{
		if (base.gameObject.activeInHierarchy && upgradeTextGenerator.valid)
		{
			upgradeTextGenerator.UpdateText(base.item);
		}
	}

	private void SetSpellText()
	{
		mainSpellText.text = TextDataPreset.FormatItemSpellDescription(UIDisplayModule.applicationState, base.item, out spellGenerator, GetSpellInfo(), base.mainItemInfoDisplay.itemInfoDisplay.isBattleDisplay, base.mainItemInfoDisplay.itemInfoDisplay.showUncollectedStatus);
		spellTextLinkObjectDisplayer.SetGenerator(spellGenerator);
	}

	private void SetPassiveText()
	{
		mainPassiveText.text = TextDataPreset.FormatItemPassiveDescription(UIDisplayModule.applicationState, base.item, out passiveGenerator, base.mainItemInfoDisplay.itemInfoDisplay.isBattleDisplay, base.mainItemInfoDisplay.itemInfoDisplay.showUncollectedStatus);
		passiveTextLinkObjectDisplayer.SetGenerator(passiveGenerator);
	}

	private SpellCastInfo GetSpellInfo()
	{
		DisplaySpellCastInfo displaySpellCastInfo = new DisplaySpellCastInfo(UIDisplayModule.gameState)
		{
			spellItem = base.item
		};
		if (base.mainItemInfoDisplay.itemInfoDisplay.isBattleDisplay)
		{
			displaySpellCastInfo.SetDefaultTargetItem();
			displaySpellCastInfo.AddCost(base.mainItemInfoDisplay.itemInfoDisplay.triggerType);
			return displaySpellCastInfo;
		}
		return displaySpellCastInfo;
	}
}
