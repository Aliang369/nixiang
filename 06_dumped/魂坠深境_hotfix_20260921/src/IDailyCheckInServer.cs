using System.Threading.Tasks;

public interface IDailyCheckInServer
{
	Task<DailyCheckInResult> TriggerDailyCheckInAsync();
}
