using System.Linq;
using UnityEngine;

public class CutsceneDebugSetter : DebugPanel.Setter
{
	private string filterText;

	public override string name => "CG演出调试";

	public override void OnSwitchGroup()
	{
	}

	public CutsceneDebugSetter(DebugPanel debugPanel)
		: base(debugPanel)
	{
	}

	private string GetCutSceneFullId(CutsceneInfoData infodata)
	{
		return "#CUT#" + ((ICTAsset)infodata).GetPaddingID() + "#";
	}

	private void BuildnvokeCells()
	{
		if (DebugPanel.Setter.applicationState?.assetCatalog == null)
		{
			return;
		}
		debugPanel.ClearCells(debugPanel.currentCells.Where((GameObject t) => !t.GetComponent<DebugInputTextCell>()));
		foreach (CutsceneInfoData cutscene in DebugPanel.Setter.applicationState.assetCatalog.cutsceneCatalog.ValidAssets)
		{
			if (!filterText.IsNullOrEmpty() && !cutscene.name.Contains(filterText) && !GetCutSceneFullId(cutscene).Contains(filterText))
			{
				continue;
			}
			string text = GetCutSceneFullId(cutscene) + " " + cutscene.name;
			debugPanel.AddInvokeCell(text, delegate
			{
				if (DebugPanel.Setter.applicationState.fortressState != null)
				{
					PlayCutSceneState playCutSceneState = new PlayCutSceneState(cutscene, DebugPanel.Setter.applicationState.fortressState, stayCutscene: false, skipFadeIn: false);
					DebugPanel.Setter.applicationState.fortressState.StartCoroutine(playCutSceneState.PlayCutsceneCor);
				}
				if (DebugPanel.Setter.applicationState.runningGameState != null)
				{
					DebugPanel.Setter.applicationState.runningGameState.actionManager.EnqueueAction(new PlayCutsceneAction(cutscene));
				}
			}, 2);
		}
	}
}
