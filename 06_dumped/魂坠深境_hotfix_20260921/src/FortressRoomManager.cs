using UnityEngine;

public class FortressRoomManager : FortressGameManager
{
	public DungeonRoom room;

	public RoomInfoData roomInfoData;

	public override void Init(FortressStateInfo fortressState)
	{
		SetRoom(ScriptableObjectSingleton<FortressDataManager>.instance.defaultFortressRoomRef);
	}

	public override void Dispose()
	{
		if (room != null)
		{
			Object.Destroy(room.gameObject);
		}
	}

	public void SetRoom(RoomInfoData roomInfoData)
	{
		if (roomInfoData == null)
		{
			roomInfoData = Bootstrap.applicationState.assetCatalog.roomInfoDataCatalog.assets[0];
		}
		this.roomInfoData = roomInfoData;
		if (room != null)
		{
			Object.Destroy(room.gameObject);
		}
		DungeonRoom orSyncLoadAsset = roomInfoData.GetOrSyncLoadAsset();
		if (!(orSyncLoadAsset == null))
		{
			room = orSyncLoadAsset.InstantiatePrefabIn(base.uiManager.transform);
			room.ApplyRoomRenderSetting(base.applicationState.fortressState.applicationState);
			if (room.renderSettings.cameraConfig != null && room.renderSettings.cameraConfig.cameraState != VirtualCamera.State.None)
			{
				base.uiManager.fortressCamera.SetState(room.renderSettings.cameraConfig.cameraState);
			}
			else
			{
				base.uiManager.fortressCamera.SetState(VirtualCamera.State.Fortress);
			}
		}
	}

	public void SetRoom(RoomRef roomRef)
	{
		SetRoom(roomRef.GetAssetPreData(base.applicationState.assetCatalog));
	}
}
