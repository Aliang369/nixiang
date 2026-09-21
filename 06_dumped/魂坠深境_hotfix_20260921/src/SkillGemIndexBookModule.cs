using System.Collections.Generic;
using ContainerOfSoul.Display.Fortress.IndexDisplay;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class SkillGemIndexBookModule : SkillGemDisplay.DisplayModule
{
	public Color darkenColor;

	public GameObject maskObject;

	public IndexBookContentMask indexBookContentMask;

	public override void Init()
	{
		base.display.InitUI(indexBookContentMask);
	}

	public override void Setup()
	{
		base.display.CenterHexShape();
		indexBookContentMask.Setup(base.display.setupData.def);
	}

	public override void UpdateDisplay()
	{
		if (UIDisplayModule.applicationState.discoverManager.IsDiscovered(base.display.setupData.def))
		{
			maskObject.gameObject.SetActive(value: false);
			base.display._hexImages.ForEach(delegate(KeyValuePair<Vector2Int, Image> p)
			{
				p.Value.color = Color.white;
			});
		}
		else
		{
			maskObject.gameObject.SetActive(value: true);
			base.display._hexImages.ForEach(delegate(KeyValuePair<Vector2Int, Image> p)
			{
				p.Value.color = darkenColor;
			});
		}
	}
}
