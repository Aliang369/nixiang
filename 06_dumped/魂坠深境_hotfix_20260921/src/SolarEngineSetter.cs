using System;

public class SolarEngineSetter : DebugPanel.Setter
{
	private const string MockProductName = "调试测试商品";

	private const string MockProductId = "com.chillyroom.containerofsoul.test";

	private const int MockPriceCents = 600;

	public override string name => "热力上报测试";

	public SolarEngineSetter(DebugPanel debugPanel)
		: base(debugPanel)
	{
	}

	public override void OnSwitchGroup()
	{
		debugPanel.AddInvokeCell("模拟购买-成功", TrackSuccess, 2);
		debugPanel.AddInvokeCell("模拟购买-失败", TrackFail, 2);
		debugPanel.AddInvokeCell("模拟购买-幂等已认领(视作成功)", TrackClaimed, 2);
	}

	private void TrackSuccess()
	{
		string text = Guid.NewGuid().ToString();
		SolarEngineManager.TrackPurchase("调试测试商品", "com.chillyroom.containerofsoul.test", 600, success: true, text);
		TraceLog.Debug("[热力测试] 模拟购买-成功 order=" + text);
	}

	private void TrackFail()
	{
		string text = Guid.NewGuid().ToString();
		SolarEngineManager.TrackPurchase("调试测试商品", "com.chillyroom.containerofsoul.test", 600, success: false, text, "weixin");
		TraceLog.Debug("[热力测试] 模拟购买-失败 order=" + text);
	}

	private void TrackClaimed()
	{
		string text = Guid.NewGuid().ToString();
		SolarEngineManager.TrackPurchase("调试测试商品", "com.chillyroom.containerofsoul.test", 600, success: true, text, "weixin");
		TraceLog.Debug("[热力测试] 模拟购买-幂等已认领(视作成功) order=" + text);
	}
}
