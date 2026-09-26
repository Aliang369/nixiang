using System;
using UnityEngine;

public class zbSortMainMgr
{
	private static zbSortMainMgr sInstance;

	private LayerConfig[] layerConfigArray;

	public static zbSortMainMgr Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbSortMainMgr();
				sInstance.Init();
			}
			return sInstance;
		}
	}

	public void Init()
	{
		int length = Enum.GetValues(typeof(ESortMainLayerType)).Length;
		layerConfigArray = new LayerConfig[length];
		layerConfigArray[0] = new LayerConfig
		{
			layer = ESortMainLayerType.Back,
			zDepth = 210f
		};
		layerConfigArray[1] = new LayerConfig
		{
			layer = ESortMainLayerType.RoomBg,
			zDepth = 70f
		};
		layerConfigArray[2] = new LayerConfig
		{
			layer = ESortMainLayerType.RoomSpine,
			zDepth = 60f
		};
		layerConfigArray[3] = new LayerConfig
		{
			layer = ESortMainLayerType.Elevator,
			zDepth = 55f
		};
		layerConfigArray[4] = new LayerConfig
		{
			layer = ESortMainLayerType.ElevatorInner,
			zDepth = 58f
		};
		layerConfigArray[5] = new LayerConfig
		{
			layer = ESortMainLayerType.Door,
			zDepth = 50f
		};
		layerConfigArray[6] = new LayerConfig
		{
			layer = ESortMainLayerType.Worker,
			zDepth = 40f
		};
		layerConfigArray[7] = new LayerConfig
		{
			layer = ESortMainLayerType.RoomGoldIcon,
			zDepth = 30f
		};
		layerConfigArray[8] = new LayerConfig
		{
			layer = ESortMainLayerType.Wall,
			zDepth = 20f
		};
		layerConfigArray[9] = new LayerConfig
		{
			layer = ESortMainLayerType.RoomGoldSpine,
			zDepth = 0f
		};
		layerConfigArray[10] = new LayerConfig
		{
			layer = ESortMainLayerType.FrontLayer,
			zDepth = -7f
		};
		layerConfigArray[11] = new LayerConfig
		{
			layer = ESortMainLayerType.Projectile,
			zDepth = -1f
		};
	}

	private LayerConfig GetLayerConfig(ESortMainLayerType layerType)
	{
		return layerConfigArray[(int)layerType];
	}

	public void InitSortOrder(ESortMainLayerType layerType, GameObject obj)
	{
		UpdateSortOrder(layerType, obj);
	}

	public void UpdateSortOrder(ESortMainLayerType layerType, GameObject obj, Vector3? overridePos = null)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		LayerConfig layerConfig = GetLayerConfig(layerType);
		Transform transform = obj.transform;
		_003F val = ((_003F?)overridePos) ?? transform.position;
		Vector3 localPosition = transform.localPosition;
		float num = ((Vector3)val).y * 0.1f;
		float num2 = ((Vector3)val).x * 0.01f;
		float num3 = layerConfig.zDepth + num + num2;
		if (Mathf.Abs(localPosition.z - num3) > 0.0001f)
		{
			Vector3 localPosition2 = localPosition;
			localPosition2.z = num3;
			transform.localPosition = localPosition2;
		}
	}
}
