namespace RpcFramework;

public interface IRpcFeatureModule
{
	void Register(RpcCommandRouter router);
}
