using UnityEngine;

namespace ContainerOfSoul.Display.Game.PathLocationModules;

public class ClassMatterModule : PathLocationDisplay.DisplayModule
{
	[SerializeField]
	private MatterCell classMatterCell;

	public override void Init()
	{
		parent.InitUI(classMatterCell);
	}

	public override void Setup()
	{
		classMatterCell.transform.parent.gameObject.SetActive(value: false);
		if (base.pathLocationDisplay.location.isNodePoint)
		{
			MatterInfo stageClassMatterInfo = UIDisplayModule.gameState.classMatterManager.GetStageClassMatterInfo(UIDisplayModule.gameState.contractState.stage);
			if (stageClassMatterInfo != null && stageClassMatterInfo.count != 0)
			{
				classMatterCell.transform.parent.gameObject.SetActive(value: true);
				classMatterCell.Setup(stageClassMatterInfo);
			}
		}
	}
}
