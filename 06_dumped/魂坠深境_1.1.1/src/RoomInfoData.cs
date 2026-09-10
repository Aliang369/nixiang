using UnityEngine.AddressableAssets;

public class RoomInfoData : AssetPreData<DungeonRoom>
{
	public string name;

	public string bakeRoomAddress;

	public AssetReference BakeRoomRef => AssetPreData<DungeonRoom>.GetGlobalAssetRef(bakeRoomAddress);

	public AssetReference EditRoomRef => AssetPreData<DungeonRoom>.GetGlobalAssetRef(address);

	public override AssetReference TargetRef
	{
		get
		{
			if (enableBakeRoomLoad && !bakeRoomAddress.IsNullOrEmpty())
			{
				return BakeRoomRef;
			}
			return EditRoomRef;
		}
	}

	public override string CTAssetIDPrepend => "ROOMINFO";

	private bool enableBakeRoomLoad => !GlobalConfigManager.instance.disableBakeRoomLoad;

	public override bool IsValidAsset()
	{
		return TargetRef != null;
	}
}
