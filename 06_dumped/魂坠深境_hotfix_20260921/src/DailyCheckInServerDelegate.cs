using System.Threading.Tasks;

public sealed class DailyCheckInServerDelegate
{
	private readonly IDailyCheckInServer server;

	public DailyCheckInServerDelegate(ApplicationStateInfo applicationState)
	{
		IDailyCheckInServer dailyCheckInServer2;
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			IDailyCheckInServer dailyCheckInServer = new DailyCheckInServerMockup(applicationState);
			dailyCheckInServer2 = dailyCheckInServer;
		}
		else
		{
			IDailyCheckInServer dailyCheckInServer = new DailyCheckInServerStub(applicationState);
			dailyCheckInServer2 = dailyCheckInServer;
		}
		server = dailyCheckInServer2;
	}

	public Task<DailyCheckInResult> TriggerDailyCheckInAsync()
	{
		return server.TriggerDailyCheckInAsync();
	}
}
