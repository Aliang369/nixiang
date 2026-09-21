using JetBrains.Annotations;

namespace ChillyRoom.Services.ContainerOfSoul;

public class NetworkConvertContext
{
	public ApplicationStateInfo applicationState;

	public NetworkConvertContext(ApplicationStateInfo applicationState)
	{
		this.applicationState = applicationState;
	}

	[CanBeNull]
	public T FullIdToCTAsset<T>(string id) where T : class, ICTAsset
	{
		if (id.IsNullOrEmpty())
		{
			return null;
		}
		T val = applicationState.assetCatalog.FullIdToCTAsset<T>(id);
		if (val == null)
		{
			TraceLog.Error("未找到【" + id + "】对应的资源");
		}
		return val;
	}
}
