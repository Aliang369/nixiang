using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RenderSetter : DebugPanel.Setter
{
	private const long Kb = 1024L;

	private const long Mb = 1048576L;

	private readonly TimeSpan _curCpuTime;

	private float _curTime;

	private float _fps;

	private int _index = 1;

	private readonly int _indexCount = 50;

	private float _lastTime;

	private TimeSpan _preCpuTime;

	public List<UniversalRendererData> rs;

	private static bool bakeTransparentEnable = true;

	private static bool bakeCutoutEnable = true;

	private static bool particleEnable = true;

	private static bool noiseFogEnable = true;

	private static bool terrainEnable = true;

	private static bool skyboxEnable = true;

	private static bool postprocessEnable = true;

	private static bool roomEnable = true;

	private static bool tyndallEnable = true;

	private static bool enemyEnable = true;

	private static bool enemyOutlineEnable = true;

	private static bool UIEnable = true;

	private static bool OverlayUIEnable = true;

	private static bool NonOverlayEnvUIEnable = true;

	private static bool LightEnable = true;

	private static bool depthFogEnable = true;

	public override string name => "渲染";

	public RenderSetter(DebugPanel debugPanel)
		: base(debugPanel)
	{
		_curCpuTime = new TimeSpan(0L);
		_preCpuTime = _curCpuTime;
		_curTime = Time.realtimeSinceStartup;
		_lastTime = _curTime;
	}

	public override void OnSwitchGroup()
	{
	}

	private void ReplaceTerrain(int i)
	{
		DebugPanel.Setter.applicationState.runningGameState.currentEncounter.room.transform.GetChild(2).GetChild(3).GetComponent<TerrainReplacer>()
			.Replace(i);
	}
}
