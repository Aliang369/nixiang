using UnityEngine;

namespace SkillDisplayModules;

public class FavoriteSkillModule : SkillDisplay.DisplayModule
{
	public GameObject favoriteHint;

	public override void UpdateDisplay()
	{
		if (base.profileSkill != null && base.profileSkill.isFavorite)
		{
			favoriteHint.gameObject.SetActive(value: true);
		}
		else
		{
			favoriteHint.gameObject.SetActive(value: false);
		}
	}
}
