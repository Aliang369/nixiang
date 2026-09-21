using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public class ClavisDisplay : CellUI<ClavisDef>, IDisplayModulesController
{
	[SerializeField]
	private Image icon;

	public ClavisDef clavisDef => base.setupData;

	protected override void OnSetup(ClavisDef clavis)
	{
		bool flag = CellUI<ClavisDef>.applicationState.clavisManager.IsUnlocked(clavis);
		icon.sprite = clavis.icon;
		icon.color = (flag ? Color.white : ColorHelper.IndexUnDiscoverColor);
		GetModule<InputModule>().enabled = flag;
	}
}
