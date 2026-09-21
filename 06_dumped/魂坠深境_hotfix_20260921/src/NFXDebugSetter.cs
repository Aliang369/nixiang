using System.Linq;
using UnityEngine;

public class NFXDebugSetter : DebugPanel.Setter
{
	private string filterText;

	public override string name => "特效调试";

	public override void OnSwitchGroup()
	{
	}

	public NFXDebugSetter(DebugPanel debugPanel)
		: base(debugPanel)
	{
	}

	private void BuildnvokeCells()
	{
		if (DebugPanel.Setter.applicationState?.assetCatalog == null)
		{
			return;
		}
		debugPanel.ClearCells(debugPanel.currentCells.Where((GameObject t) => !t.GetComponent<DebugInputTextCell>()));
		foreach (NFXSetup nfx in DebugPanel.Setter.applicationState.assetCatalog.nfxCatalog.ValidAssets)
		{
			if (filterText.IsNullOrEmpty() || nfx.name.Contains(filterText) || ((ICTAsset)nfx).GetFullID().Contains(filterText))
			{
				string text = ((ICTAsset)nfx).GetFullID() + " " + nfx.name;
				debugPanel.AddInvokeCell(text, delegate
				{
					nfx.DisplayNFXForTest();
				}, 2);
			}
		}
	}
}
