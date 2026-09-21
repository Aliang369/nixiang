using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DungeonRoom : CTPrefabAsset
{
	[Serializable]
	public struct RoomRenderSettings(int a = 0)
	{
		public bool fog = false;

		public bool DepthFogColor = false;

		public Color fogColor = default(Color);

		public float fogStartDistance = 0f;

		public float fogMiddleDistance1 = 0f;

		public float fogMiddleDistance2 = 0f;

		public float fogEndDistance = 0f;

		public FogMode fogMode = (FogMode)0;

		public float fogDensity = 0f;

		public float HeightFogTop = 1f;

		public float HeightFogBottom = 0.9f;

		public Color nearFogColor = default(Color);

		public Color middleFogColor1 = default(Color);

		public Color middleFogColor2 = default(Color);

		public Color farFogColor = default(Color);

		[ColorUsage(true, true)]
		public Color baseColorMultiply = default(Color);

		[ColorUsage(true, true)]
		public Color roomAmbient = default(Color);

		[ColorUsage(true, true)]
		public Color roomAmbient2 = default(Color);

		public float roomAmbient2Occlusion = 0f;

		public bool enableOverrideFastPostProcessSettings = false;

		public Material skybox = null;

		public CameraConfig cameraConfig = null;

		public FastPostProcessRenderFeature.FastPostProcessSettings fastPostProcessSettings = default(FastPostProcessRenderFeature.FastPostProcessSettings);

		public void Update()
		{
			RenderSettings.fogColor = fogColor;
			RenderSettings.fogStartDistance = fogStartDistance;
			RenderSettings.fogEndDistance = fogEndDistance;
			RenderSettings.fogMode = fogMode;
			RenderSettings.fogDensity = fogDensity;
			RenderSettings.fog = fog;
			RenderSettings.skybox = skybox;
			CTRenderSetting.DepthFogColor = DepthFogColor;
			CTRenderSetting.HeightFogBottom = HeightFogBottom;
			CTRenderSetting.HeightFogTop = HeightFogTop;
			CTRenderSetting.NearFogColor = nearFogColor;
			CTRenderSetting.MiddleFogColor1 = middleFogColor1;
			CTRenderSetting.MiddleFogColor2 = middleFogColor2;
			CTRenderSetting.FarFogColor = farFogColor;
			CTRenderSetting.MiddleFogFactor1 = (fogMiddleDistance1 - fogStartDistance) / (fogEndDistance - fogStartDistance);
			CTRenderSetting.MiddleFogFactor2 = (fogMiddleDistance2 - fogStartDistance) / (fogEndDistance - fogStartDistance);
			CTRenderSetting.BaseColorMultiply = baseColorMultiply;
			CTRenderSetting.RoomAmbient = roomAmbient;
			CTRenderSetting.RoomAmbient2 = roomAmbient2;
			CTRenderSetting.RoomAmbient2Occlusion = roomAmbient2Occlusion;
			ApplicationStateInfo currentBootTarget = BootSetter<ApplicationStateInfo>.GetCurrentBootTarget();
			if (currentBootTarget == null)
			{
				return;
			}
			RenderingManager renderingManager = currentBootTarget.renderingManager;
			if (renderingManager != null)
			{
				if (enableOverrideFastPostProcessSettings)
				{
					renderingManager.OverrideFastPostProcessSettings(fastPostProcessSettings);
				}
				else
				{
					renderingManager.RestoreFastPostProcessSettings();
				}
			}
			if (cameraConfig != null && currentBootTarget.fortressState != null && cameraConfig.cameraState != VirtualCamera.State.None)
			{
				currentBootTarget.fortressState.uiManager.fortressCamera.SetState(cameraConfig.cameraState);
			}
		}
	}

	[Serializable]
	public class CameraConfig
	{
		public VirtualCamera.State cameraState;
	}

	[Serializable]
	public class BakedInfo
	{
		[SerializeField]
		public List<Material> bakeMeshMaterials;

		[SerializeField]
		public List<MeshRenderer> meshs;
	}

	[SerializeField]
	public Transform enemySpawnPivot;

	[SerializeField]
	public List<Light> minorLights;

	[SerializeField]
	public RoomRenderSettings renderSettings;

	[SerializeField]
	public DialoguePlayer roomDialoguePlayer;

	[SerializeField]
	public bool isBakeRoom;

	[SerializeField]
	public bool baked;

	[SerializeField]
	public string linkedRoom;

	[SerializeField]
	public BakedInfo bakedInfo;

	[NonSerialized]
	private FPSDetector fpsDetector;

	public Vector3 cameraRotatePivotPos => enemySpawnPivot.position + 1.5f * Vector3.up;

	public override string CTAssetIDPrepend => "ROOM";

	private void Awake()
	{
		fpsDetector = new FPSDetector();
		fpsDetector.StartSample();
	}

	private void Update()
	{
		fpsDetector?.FrameUpdate();
	}

	public float GetFPS()
	{
		return fpsDetector.EndSample();
	}

	public void ApplyRoomRenderSetting(ApplicationStateInfo applicationState)
	{
		renderSettings.Update();
		applicationState.renderingManager.ApplyPerformanceModeToDungeonRoom(this);
		EncounterInfo encounterInfo = applicationState.runningGameState?.currentEncounter;
		EnvironmentInfluenceDef environmentInfluenceDef = ((!(encounterInfo?.room == this)) ? null : encounterInfo.environmentInfluence?.def);
		applicationState.renderingManager.ApplyEnvironmentRenderOverride(this, environmentInfluenceDef);
	}
}
