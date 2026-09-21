using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestDifficultySelector : MonoBehaviour
{
	public TMP_Dropdown dropdown;

	private List<DifficultyInfo> difficulties;

	private void Start()
	{
		difficulties = TestMenuManager.instance.testAssetCatalog.difficultyCatalog.ValidAssets;
		difficulties.Sort((DifficultyInfo a, DifficultyInfo b) => a.difficultyLevel.CompareTo(b.difficultyLevel));
		dropdown.options = difficulties.ConvertAll((DifficultyInfo di) => new TMP_Dropdown.OptionData(di.difficultyName));
		if (difficulties.Count != 0)
		{
			int num = difficulties.FindIndex((DifficultyInfo di) => di.difficultyLevel == TestMenuManager.instance.difficulty);
			if (num < 0)
			{
				num = 0;
				TestMenuManager.instance.difficulty = difficulties[num].difficultyLevel;
			}
			dropdown.SetValueWithoutNotify(num);
			dropdown.onValueChanged.AddListener(_OnSetDifficulty);
		}
	}

	private void _OnSetDifficulty(int index)
	{
		if (index >= 0 && index < difficulties.Count)
		{
			TestMenuManager.instance.difficulty = difficulties[index].difficultyLevel;
		}
	}
}
