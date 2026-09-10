using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public sealed class TestContractSelector : MonoBehaviour
{
	public TMP_Dropdown dropdown;

	public TextMeshProUGUI descriptionText;

	private readonly List<ContractDef> contracts = new List<ContractDef>();

	private ApplicationStateInfo descriptionPreviewState;

	private void Start()
	{
		TestMenuManager instance = TestMenuManager.instance;
		ContractDef defaultContractDef = ScriptableObjectSingleton<FortressDataManager>.instance.defaultContractDef;
		descriptionPreviewState = new ApplicationStateInfo
		{
			assetCatalog = instance.testAssetCatalog
		};
		contracts.Clear();
		contracts.AddRange(from contract in instance.testAssetCatalog.contractDefCatalog.ValidAssets
			where contract != null
			orderby ((ICTAsset)contract).AssetIndex
			select contract);
		if (defaultContractDef != null && !contracts.Contains(defaultContractDef))
		{
			contracts.Insert(0, defaultContractDef);
		}
		dropdown.options = contracts.Select((ContractDef contract) => new TMP_Dropdown.OptionData(contract.Name)).ToList();
		ConfigureText(dropdown.captionText, 16f, 30f);
		ConfigureText(dropdown.itemText, 14f, 26f);
		int num = contracts.IndexOf(instance.selectedContract);
		if (num < 0)
		{
			num = 0;
			instance.selectedContract = defaultContractDef;
		}
		dropdown.SetValueWithoutNotify(num);
		dropdown.RefreshShownValue();
		RefreshDescription(num);
		dropdown.onValueChanged.AddListener(SetContract);
	}

	private static void ConfigureText(TMP_Text text, float minimumSize, float maximumSize)
	{
		if (!(text == null))
		{
			text.enableAutoSizing = true;
			text.fontSizeMin = minimumSize;
			text.fontSizeMax = maximumSize;
			text.enableWordWrapping = false;
			text.overflowMode = TextOverflowModes.Ellipsis;
		}
	}

	private void SetContract(int index)
	{
		if (index >= 0 && index < contracts.Count)
		{
			TestMenuManager.instance.selectedContract = contracts[index];
			RefreshDescription(index);
		}
	}

	private void RefreshDescription(int index)
	{
		if (!(descriptionText == null) && index >= 0 && index < contracts.Count)
		{
			ContractDef contractDef = contracts[index];
			string text = contractDef.GetContractDescription(descriptionPreviewState, out var _);
			if (string.IsNullOrWhiteSpace(text))
			{
				text = contractDef.description;
			}
			descriptionText.text = (string.IsNullOrWhiteSpace(text) ? "无额外契约效果" : text);
		}
	}
}
