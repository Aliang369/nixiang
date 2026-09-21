public interface ILimitedTimeActivityService
{
	bool IsReady { get; }

	bool IsOpen(string activityId);

	bool TryGetRuntimeConfig(string activityId, out LimitedTimeActivityRuntimeConfig config);
}
