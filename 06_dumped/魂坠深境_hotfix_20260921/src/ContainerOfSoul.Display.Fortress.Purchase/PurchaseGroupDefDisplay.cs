using System;
using Common.UI;

namespace ContainerOfSoul.Display.Fortress.Purchase;

public class PurchaseGroupDefDisplay : CellUI<PurchaseGroupDef>, IShowAsync, IHideAsync
{
	public PurchaseGroupLayoutType layoutType;

	public Func<PurchaseInfo, bool> showChecker => (parent as FortressPurchaseUIPanel).showChecker;
}
