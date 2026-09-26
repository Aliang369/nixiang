using System;
using UnityEngine;

public class zbSortMgr
{
	public struct LayerConfig
	{
		public ESortLayerType layer;

		public float zDepth;
	}

	private static zbSortMgr sInstance;

	private LayerConfig[] layerConfigArray;

	public const bool kShadowZRebandEnabled = true;

	private const float kShadowZBandCenter = 60f;

	public static zbSortMgr Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbSortMgr();
				sInstance.Init();
			}
			return sInstance;
		}
	}

	public void Init()
	{
		int length = Enum.GetValues(typeof(ESortLayerType)).Length;
		layerConfigArray = new LayerConfig[length];
		layerConfigArray[0] = new LayerConfig
		{
			layer = ESortLayerType.Background,
			zDepth = 80f
		};
		layerConfigArray[1] = new LayerConfig
		{
			layer = ESortLayerType.BackgroundUp,
			zDepth = 75f
		};
		layerConfigArray[2] = new LayerConfig
		{
			layer = ESortLayerType.MonsterDie,
			zDepth = 70f
		};
		layerConfigArray[3] = new LayerConfig
		{
			layer = ESortLayerType.BurnLayer,
			zDepth = 60f
		};
		layerConfigArray[4] = new LayerConfig
		{
			layer = ESortLayerType.Monster,
			zDepth = 50f
		};
		layerConfigArray[5] = new LayerConfig
		{
			layer = ESortLayerType.Caocong,
			zDepth = 40f
		};
		layerConfigArray[6] = new LayerConfig
		{
			layer = ESortLayerType.Zhuangjiache,
			zDepth = 30f
		};
		layerConfigArray[7] = new LayerConfig
		{
			layer = ESortLayerType.FlyMonster,
			zDepth = 20f
		};
		layerConfigArray[8] = new LayerConfig
		{
			layer = ESortLayerType.Wall,
			zDepth = 10f
		};
		layerConfigArray[10] = new LayerConfig
		{
			layer = ESortLayerType.TeamHp,
			zDepth = 1f
		};
		layerConfigArray[9] = new LayerConfig
		{
			layer = ESortLayerType.Projectile,
			zDepth = 0f
		};
		layerConfigArray[11] = new LayerConfig
		{
			layer = ESortLayerType.Hero,
			zDepth = -1f
		};
		layerConfigArray[12] = new LayerConfig
		{
			layer = ESortLayerType.Hero,
			zDepth = -2f
		};
	}

	public void ApplyShadowReband(Transform shadowTf)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)shadowTf == (Object)null))
		{
			float num = 60f - layerConfigArray[4].zDepth;
			Vector3 localPosition = shadowTf.localPosition;
			if (!Mathf.Approximately(localPosition.z, num))
			{
				shadowTf.localPosition = new Vector3(localPosition.x, localPosition.y, num);
			}
		}
	}

	private LayerConfig GetLayerConfig(ESortLayerType layerType)
	{
		return layerConfigArray[(int)layerType];
	}

	public void InitSortOrder(ESortLayerType layerType, GameObject obj)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		LayerConfig layerConfig = GetLayerConfig(layerType);
		Vector3 localPosition = obj.transform.localPosition;
		obj.transform.localPosition = new Vector3(localPosition.x, localPosition.y, layerConfig.zDepth);
		UpdateSortOrder(layerType, obj);
	}

	public void UpdateSortOrder(ESortLayerType layerType, GameObject obj, zbMonster monster = null)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		LayerConfig layerConfig = GetLayerConfig(layerType);
		Transform transform = obj.transform;
		Vector3 position = transform.position;
		Vector3 localPosition = transform.localPosition;
		float num = position.y * 0.01f;
		float num2 = 0f;
		if ((Object)(object)monster != (Object)null)
		{
			num2 = (float)monster.GetUUID() * 1E-06f;
		}
		float num3 = layerConfig.zDepth + num + num2;
		if (Mathf.Abs(localPosition.z - num3) > 0.001f)
		{
			Vector3 localPosition2 = localPosition;
			localPosition2.z = num3;
			transform.localPosition = localPosition2;
		}
	}
}
