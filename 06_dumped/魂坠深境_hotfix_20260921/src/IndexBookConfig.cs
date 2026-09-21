using System.Collections.Generic;

public class IndexBookConfig
{
	public class IndexBookProgressLayer
	{
		public int needDiscoverCount;

		public GlobalRewardSetup reward;
	}

	public IndexBookPage page;

	[LocalizeText("图鉴页名称")]
	public string pageTitle;

	public GlobalRewardSetup singleDiscoverReward;

	public List<IndexBookProgressLayer> indexProgressLayer;
}
