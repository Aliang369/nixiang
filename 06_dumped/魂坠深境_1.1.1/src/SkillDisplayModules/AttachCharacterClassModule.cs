using System;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SkillDisplayModules;

public class AttachCharacterClassModule : SkillDisplay.DisplayModule
{
	[SerializeField]
	private Image prefab;

	[NonSerialized]
	private PrefabPool<Image> pool;

	private SkillDef skillDef => base.gameSkill?.skillDef;

	public override void Init()
	{
		pool = new PrefabPool<Image>(prefab);
	}

	public override void UpdateDisplay()
	{
		pool.DespawnAll();
		if (!(skillDef.attachCharacterClass == null))
		{
			pool.Spawn().sprite = skillDef.attachCharacterClass.professionIcon;
		}
	}
}
