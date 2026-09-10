using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RecordSkillCell : CellUI<GameRecord.SkillRecord>, IPointerClickHandler, IEventSystemHandler
{
	public Image image;

	public Image usedHint;

	[NonSerialized]
	public Action onClick;

	public GameRecord.SkillRecord skillInfo => base.setupData;

	public void OnPointerClick(PointerEventData eventData)
	{
		onClick?.Invoke();
	}

	protected override void OnSetup(GameRecord.SkillRecord _)
	{
		onClick = null;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		SkillDef skill = skillInfo.skill;
		image.sprite = skill.Icon;
		usedHint.gameObject.SetActive(skillInfo.activated);
	}
}
