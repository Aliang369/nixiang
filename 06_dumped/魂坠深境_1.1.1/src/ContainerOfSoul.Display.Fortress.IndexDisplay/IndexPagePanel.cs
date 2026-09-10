using System;
using System.Collections.Generic;
using System.Linq;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public abstract class IndexPagePanel : UIBase
{
	public abstract string TASubPanelTagName { get; }

	public IndexBookConfig CurrentPageConfig => ScriptableObjectSingleton<FortressDataManager>.instance.indexBookConfigs.FirstOrDefault((IndexBookConfig c) => c.page == GetIndexPageType());

	public abstract IndexBookPage GetIndexPageType();

	public virtual bool WithIndexBookRewardToGet()
	{
		return UIBase.applicationState.discoverManager.CheckPageWithAnyRewardToGet(GetIndexPageType());
	}

	public virtual void ClearTransientSelection()
	{
	}

	public virtual bool TrySelectFirstRewardPage()
	{
		return UIBase.applicationState.discoverManager.CheckPageWithAnyRewardToGet(GetIndexPageType());
	}

	public virtual bool EmphasizeNextRewardTarget()
	{
		return false;
	}

	public virtual bool TryFocusFirstClaimableSingleReward()
	{
		return false;
	}

	public bool CurrentPageHasReadyProgressReward()
	{
		return UIBase.applicationState.discoverManager.GetPageReadyProgressRewardCount(GetIndexPageType()) > 0;
	}

	public bool CurrentPageHasClaimableSingleReward()
	{
		return UIBase.applicationState.discoverManager.GetPageReadySingleRewardCount(GetIndexPageType()) > 0;
	}

	protected bool TryFocusFirstClaimableSingleReward(ScrollRectGridLayoutGroup layoutGroup, Func<object, ICTAsset> getTargetAsset)
	{
		if (layoutGroup == null || getTargetAsset == null || CurrentPageConfig?.singleDiscoverReward == null)
		{
			return false;
		}
		int num = layoutGroup.displayElements.FindIndex(delegate(ScrollRectGridLayoutGroup.DisplayElement element)
		{
			ICTAsset iCTAsset = getTargetAsset(element.data);
			return iCTAsset != null && UIBase.applicationState.discoverManager.IsDiscovered(iCTAsset) && !UIBase.applicationState.discoverManager.IsRewarded(iCTAsset);
		});
		if (num >= 0)
		{
			return layoutGroup.ScrollToIndex(num);
		}
		return false;
	}

	public IEnumerable<T> GetIndexPageAllContent<T>()
	{
		return DiscoverManager.GetAllIndexPageContent<T>(GetIndexPageType(), UIBase.applicationState.assetCatalog);
	}
}
