using System.Threading.Tasks;

public sealed class YuanQiActivityServerDelegate : IYuanQiActivityServer
{
	private readonly IYuanQiActivityServer server;

	public YuanQiActivityServerDelegate(ApplicationStateInfo applicationState)
	{
		IYuanQiActivityServer yuanQiActivityServer2;
		if (!CSCommon.enableNetwork)
		{
			IYuanQiActivityServer yuanQiActivityServer = new YuanQiActivityServerMockup(applicationState);
			yuanQiActivityServer2 = yuanQiActivityServer;
		}
		else
		{
			IYuanQiActivityServer yuanQiActivityServer = new YuanQiActivityServerStub(applicationState);
			yuanQiActivityServer2 = yuanQiActivityServer;
		}
		server = yuanQiActivityServer2;
	}

	public Task<GetYuanQiActivityStateResponse> GetActivityStateAsync(GetYuanQiActivityStateRequest request)
	{
		return server.GetActivityStateAsync(request);
	}

	public Task<ReportYuanQiActivityMissionReachedResponse> ReportMissionReachedAsync(ReportYuanQiActivityMissionReachedRequest request)
	{
		return server.ReportMissionReachedAsync(request);
	}

	public Task<ClaimYuanQiActivityMissionRewardResponse> ClaimMissionRewardAsync(ClaimYuanQiActivityMissionRewardRequest request)
	{
		return server.ClaimMissionRewardAsync(request);
	}
}
