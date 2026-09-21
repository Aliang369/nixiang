using System;

public class AdSetter : DebugPanel.Setter
{
	public override string name => "广告";

	public AdSetter(DebugPanel debugPanel)
		: base(debugPanel)
	{
	}

	public override void OnSwitchGroup()
	{
	}

	private void AddAdControls(string displayName, string adName)
	{
		debugPanel.AddValueCell(displayName + "(" + adName + ") Ready", delegate
		{
		}, delegate
		{
		}, () => IsAdReady(adName) ? 1 : 0, 2);
		debugPanel.AddInvokeCell(displayName + "(" + adName + ") 获取广告", delegate
		{
			LoadAd(adName);
		}, 2);
		debugPanel.AddInvokeCell(displayName + "(" + adName + ") 播放广告", delegate
		{
			ShowAd(adName);
		}, 2);
	}

	private bool IsAdReady(string adName)
	{
		if (Bootstrap.instance.adsManager == null)
		{
			return false;
		}
		try
		{
			return Bootstrap.instance.adsManager.IsAdReady(adName);
		}
		catch (Exception arg)
		{
			TraceLog.Error($"DebugPanel检查广告状态失败: {adName}, {arg}");
			return false;
		}
	}

	private void LoadAd(string adName)
	{
		if (Bootstrap.instance.adsManager != null)
		{
			Bootstrap.instance.adsManager.EnterAdScene(adName);
			Bootstrap.instance.adsManager.LoadAll();
		}
	}

	private void ShowAd(string adName)
	{
		if (Bootstrap.instance.adsManager != null)
		{
			Bootstrap.instance.adsManager.EnterAdScene(adName);
			Bootstrap.instance.adsManager.ShowAd(adName, delegate
			{
				TraceLog.Debug("DebugPanel广告播放完成: " + adName);
			}, delegate
			{
				TraceLog.Error("DebugPanel广告播放失败: " + adName);
			}, delegate
			{
				TraceLog.Debug("DebugPanel广告关闭: " + adName);
			});
		}
	}
}
