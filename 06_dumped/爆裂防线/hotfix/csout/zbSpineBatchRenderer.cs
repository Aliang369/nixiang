using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Rendering;

public class zbSpineBatchRenderer
{
	private class Entry
	{
		public int id;

		public SkeletonRenderer spineRenderer;

		public zbSpineBatchRenderer owner;

		public zbMonster monster;

		public bool hasMonster;

		public Transform spineTf;

		public MeshFilter meshFilter;

		public Vector3[] vertexBuffer;

		public Vector2[] uvBuffer;

		public Color32[] colorBuffer;

		public int vertexCount;

		public bool hasVertexData;

		public readonly List<int> triangles = new List<int>(256);

		public float sortZ;

		public void OnMeshOverride(SkeletonRendererInstruction instruction)
		{
		}

		public void HandlePostProcessVertices(MeshGeneratorBuffers buffers)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			vertexBuffer = buffers.vertexBuffer;
			uvBuffer = buffers.uvBuffer;
			colorBuffer = buffers.colorBuffer;
			vertexCount = buffers.vertexCount;
			hasVertexData = true;
			owner?.MarkBuildDirty();
		}

		public void HandleMeshAndMaterialsUpdated(SkeletonRenderer skeletonRenderer)
		{
			Mesh val = (((Object)(object)meshFilter != (Object)null) ? meshFilter.sharedMesh : null);
			if ((Object)(object)val != (Object)null)
			{
				val.GetTriangles(triangles, 0);
			}
			owner?.MarkBuildDirty();
		}
	}

	public static readonly bool kEnabled = true;

	public static readonly bool kPvpEnabled = true;

	private const float kMonsterLayerZDepth = 50f;

	private const string kCounterSubscribed = "合并订阅";

	private const string kCounterExcludedPvpGate = "合并排除-PVP阶段闸";

	private const string kCounterExcludedElite = "合并排除-精英boss";

	private const string kCounterExcludedWater = "合并排除-水场景";

	private const string kCounterExcludedSprite = "合并排除-换图模式";

	private const string kCounterExcludedInvalid = "合并排除-骨架无效";

	private const string kCounterExcludedMultiMaterial = "合并排除-多材质";

	private const string kCounterExcludedNotMerged = "合并排除-非合并资产";

	private const string kCounterExcludedSeparator = "合并排除-separator";

	private const string kCounterWritten = "合并写入怪数";

	private const string kCounterVertices = "合并写入顶点";

	private const string kCounterSkipped = "合并跳写数";

	private const string kCounterMismatch = "合并快照错配跳写";

	private const string kCounterBuildMicros = "合并聚合us";

	private const string kCounterBuildException = "合并Build异常次数";

	private const string kCounterSleepFrames = "合并休眠帧数";

	private static readonly Comparison<Entry> kEntryComparison = CompareEntries;

	private static int s_nextEntryId = 1;

	private readonly GameObject m_root;

	private readonly Mesh m_mesh;

	private readonly MeshRenderer m_renderer;

	private readonly zbSpineBatchRendererHost m_host;

	private bool m_valid;

	private bool m_rendererActive;

	private bool m_buildSleeping;

	private bool m_buildDirty;

	private readonly List<Entry> m_entries;

	private readonly Dictionary<SkeletonRenderer, Entry> m_entryByRenderer;

	private readonly List<Vector3> m_vertices;

	private readonly List<Vector2> m_uvs;

	private readonly List<Color32> m_colors;

	private readonly List<int> m_indices;

	private static readonly int[] kEmptyTriangles = new int[0];

	private const double kDumpLogIntervalSec = 10.0;

	private const int kMaxDumpEntries = 8;

	private double m_lastExceptionLogTime;

	public int SubscribedCount => m_entries.Count;

	public int LastWrittenCount { get; private set; }

	public int LastSkippedCount { get; private set; }

	public int LastVertexCount { get; private set; }

	public int LastTriangleCount { get; private set; }

	public double LastBuildMs { get; private set; }

	public double AccumBuildMs { get; private set; }

	public int BuildSampleCount { get; private set; }

	public bool isValid => m_valid;

	private static int CompareEntries(Entry a, Entry b)
	{
		int num = b.sortZ.CompareTo(a.sortZ);
		if (num == 0)
		{
			return a.id.CompareTo(b.id);
		}
		return num;
	}

	private void MarkBuildDirty()
	{
		m_buildDirty = true;
	}

	public zbSpineBatchRenderer(Transform parent, Material material, string name, int layer)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Expected O, but got Unknown
		m_valid = true;
		m_buildDirty = true;
		m_entries = new List<Entry>(128);
		m_entryByRenderer = new Dictionary<SkeletonRenderer, Entry>(128);
		m_vertices = new List<Vector3>(4096);
		m_uvs = new List<Vector2>(4096);
		m_colors = new List<Color32>(4096);
		m_indices = new List<int>(8192);
		m_lastExceptionLogTime = -10.0;
		base._002Ector();
		m_root = new GameObject(name)
		{
			layer = layer
		};
		m_root.transform.SetParent(parent, false);
		m_root.transform.localPosition = new Vector3(0f, 0f, 50f);
		m_root.transform.localRotation = Quaternion.identity;
		m_root.transform.localScale = Vector3.one;
		m_mesh = new Mesh
		{
			name = name + "_Mesh",
			indexFormat = (IndexFormat)1
		};
		m_mesh.MarkDynamic();
		m_root.AddComponent<MeshFilter>().sharedMesh = m_mesh;
		m_renderer = m_root.AddComponent<MeshRenderer>();
		((Renderer)m_renderer).sharedMaterial = material;
		((Renderer)m_renderer).shadowCastingMode = (ShadowCastingMode)0;
		((Renderer)m_renderer).receiveShadows = false;
		((Renderer)m_renderer).lightProbeUsage = (LightProbeUsage)0;
		((Renderer)m_renderer).reflectionProbeUsage = (ReflectionProbeUsage)0;
		((Renderer)m_renderer).motionVectorGenerationMode = (MotionVectorGenerationMode)2;
		((Renderer)m_renderer).enabled = false;
		m_rendererActive = false;
		m_host = m_root.AddComponent<zbSpineBatchRendererHost>();
		m_host.owner = this;
	}

	public bool Subscribe(SkeletonRenderer spineRenderer, zbMonster monster)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		if (!m_valid || (Object)(object)spineRenderer == (Object)null)
		{
			return false;
		}
		if (m_entryByRenderer.ContainsKey(spineRenderer))
		{
			return true;
		}
		MeshFilter component = ((Component)spineRenderer).GetComponent<MeshFilter>();
		if ((Object)(object)component == (Object)null)
		{
			return false;
		}
		Entry entry = new Entry
		{
			id = s_nextEntryId++,
			spineRenderer = spineRenderer,
			owner = this,
			monster = monster,
			hasMonster = ((Object)(object)monster != (Object)null),
			spineTf = ((Component)spineRenderer).transform,
			meshFilter = component
		};
		spineRenderer.GenerateMeshOverride += new InstructionDelegate(entry.OnMeshOverride);
		spineRenderer.OnPostProcessVertices += new MeshGeneratorDelegate(entry.HandlePostProcessVertices);
		spineRenderer.OnMeshAndMaterialsUpdated += new SkeletonRendererDelegate(entry.HandleMeshAndMaterialsUpdated);
		m_entryByRenderer.Add(spineRenderer, entry);
		m_entries.Add(entry);
		MarkBuildDirty();
		return true;
	}

	public bool Unsubscribe(SkeletonRenderer spineRenderer)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		if ((Object)(object)spineRenderer == (Object)null || !m_entryByRenderer.TryGetValue(spineRenderer, out var value))
		{
			return false;
		}
		m_entryByRenderer.Remove(spineRenderer);
		m_entries.Remove(value);
		spineRenderer.GenerateMeshOverride -= new InstructionDelegate(value.OnMeshOverride);
		spineRenderer.OnPostProcessVertices -= new MeshGeneratorDelegate(value.HandlePostProcessVertices);
		spineRenderer.OnMeshAndMaterialsUpdated -= new SkeletonRendererDelegate(value.HandleMeshAndMaterialsUpdated);
		MarkBuildDirty();
		return true;
	}

	public void Free()
	{
		if (m_valid)
		{
			m_valid = false;
			for (int num = m_entries.Count - 1; num >= 0; num--)
			{
				Unsubscribe(m_entries[num].spineRenderer);
			}
			m_mesh.Clear();
			Object.Destroy((Object)(object)m_mesh);
			Object.Destroy((Object)(object)m_root);
		}
	}

	public void Build()
	{
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		if (!m_valid)
		{
			return;
		}
		if (m_entries.Count == 0)
		{
			SetRendererActive(active: false);
			return;
		}
		if (m_buildSleeping && !m_buildDirty)
		{
			LastWrittenCount = 0;
			LastSkippedCount = m_entries.Count;
			LastVertexCount = 0;
			LastTriangleCount = 0;
			zbPerfLog.Set("合并写入怪数", 0L);
			zbPerfLog.Set("合并写入顶点", 0L);
			zbPerfLog.Set("合并跳写数", m_entries.Count);
			Inc("合并休眠帧数");
			return;
		}
		m_buildDirty = false;
		double num = Time.realtimeSinceStartup;
		for (int i = 0; i < m_entries.Count; i++)
		{
			Entry entry = m_entries[i];
			entry.sortZ = (entry.hasMonster ? (50f + entry.monster.cachPositionY * 0.1f) : entry.spineTf.position.z);
		}
		m_entries.Sort(kEntryComparison);
		m_vertices.Clear();
		m_uvs.Clear();
		m_colors.Clear();
		m_indices.Clear();
		float num2 = float.MaxValue;
		float num3 = float.MaxValue;
		float num4 = float.MaxValue;
		float num5 = float.MinValue;
		float num6 = float.MinValue;
		float num7 = float.MinValue;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		int num11 = 0;
		Matrix4x4 worldToLocalMatrix = m_root.transform.worldToLocalMatrix;
		for (int j = 0; j < m_entries.Count; j++)
		{
			Entry entry2 = m_entries[j];
			if (!entry2.hasVertexData || entry2.vertexCount == 0 || entry2.triangles.Count == 0 || !((Component)entry2.spineTf).gameObject.activeInHierarchy)
			{
				num9++;
				continue;
			}
			if (entry2.hasMonster)
			{
				if (entry2.monster.isGoDestroyed)
				{
					num9++;
					continue;
				}
				if (entry2.monster.GetCurrentSpineTier() == zbMonster.ESpineUpdateTier.Culled)
				{
					num9++;
					continue;
				}
			}
			Matrix4x4 val = worldToLocalMatrix * entry2.spineTf.localToWorldMatrix;
			int count = m_vertices.Count;
			Vector3[] vertexBuffer = entry2.vertexBuffer;
			Vector2[] uvBuffer = entry2.uvBuffer;
			Color32[] colorBuffer = entry2.colorBuffer;
			List<int> triangles = entry2.triangles;
			int count2 = triangles.Count;
			int num12 = entry2.vertexCount;
			if (vertexBuffer == null || uvBuffer == null || colorBuffer == null)
			{
				Inc("合并快照错配跳写");
				num9++;
				continue;
			}
			if (num12 > vertexBuffer.Length)
			{
				num12 = vertexBuffer.Length;
			}
			if (num12 > uvBuffer.Length)
			{
				num12 = uvBuffer.Length;
			}
			if (num12 > colorBuffer.Length)
			{
				num12 = colorBuffer.Length;
			}
			int num13 = -1;
			for (int k = 0; k < count2; k++)
			{
				int num14 = triangles[k];
				if (num14 > num13)
				{
					num13 = num14;
				}
			}
			if (num13 >= num12)
			{
				Inc("合并快照错配跳写");
				num9++;
				continue;
			}
			int num15 = num12;
			for (int l = 0; l < num15; l++)
			{
				Vector3 val2 = ((Matrix4x4)(ref val)).MultiplyPoint3x4(vertexBuffer[l]);
				if (val2.x < num2)
				{
					num2 = val2.x;
				}
				if (val2.x > num5)
				{
					num5 = val2.x;
				}
				if (val2.y < num3)
				{
					num3 = val2.y;
				}
				if (val2.y > num6)
				{
					num6 = val2.y;
				}
				if (val2.z < num4)
				{
					num4 = val2.z;
				}
				if (val2.z > num7)
				{
					num7 = val2.z;
				}
				m_vertices.Add(val2);
				m_uvs.Add(uvBuffer[l]);
				m_colors.Add(colorBuffer[l]);
			}
			for (int m = 0; m < count2; m++)
			{
				m_indices.Add(triangles[m] + count);
			}
			num8++;
			num10 += num15;
			num11 += count2;
		}
		if (num8 > 0)
		{
			if (m_vertices.Count < m_mesh.vertexCount)
			{
				m_mesh.SetTriangles(kEmptyTriangles, 0, false);
			}
			m_mesh.SetVertices(m_vertices);
			m_mesh.SetUVs(0, m_uvs);
			m_mesh.SetColors(m_colors);
			m_mesh.SetTriangles(m_indices, 0, false);
			m_mesh.bounds = new Bounds(new Vector3((num2 + num5) * 0.5f, (num3 + num6) * 0.5f, (num4 + num7) * 0.5f), new Vector3(num5 - num2, num6 - num3, num7 - num4));
			SetRendererActive(active: true);
		}
		else
		{
			SetRendererActive(active: false);
		}
		m_buildSleeping = num8 == 0;
		LastBuildMs = ((double)Time.realtimeSinceStartup - num) * 1000.0;
		AccumBuildMs += LastBuildMs;
		int buildSampleCount = BuildSampleCount + 1;
		BuildSampleCount = buildSampleCount;
		LastWrittenCount = num8;
		LastSkippedCount = num9;
		LastVertexCount = num10;
		LastTriangleCount = num11 / 3;
		zbPerfLog.Set("合并写入怪数", num8);
		zbPerfLog.Set("合并写入顶点", num10);
		zbPerfLog.Set("合并跳写数", num9);
		zbPerfLog.Inc("合并聚合us", (long)(LastBuildMs * 1000.0));
	}

	private void SetRendererActive(bool active)
	{
		if (m_rendererActive != active)
		{
			m_rendererActive = active;
			((Renderer)m_renderer).enabled = active;
		}
	}

	public void OnBuildException(Exception exception)
	{
		Inc("合并Build异常次数");
		try
		{
			double num = Time.realtimeSinceStartup;
			if (num - m_lastExceptionLogTime < 10.0)
			{
				return;
			}
			m_lastExceptionLogTime = num;
			int num2 = 0;
			for (int i = 0; i < m_entries.Count; i++)
			{
				Entry entry = m_entries[i];
				bool flag = (Object)(object)entry.spineRenderer != (Object)null;
				bool flag2 = (Object)(object)entry.spineTf != (Object)null;
				bool flag3 = !entry.hasMonster || (Object)(object)entry.monster != (Object)null;
				bool flag4 = false;
				try
				{
					_ = ((Component)entry.spineTf).gameObject.activeInHierarchy;
				}
				catch
				{
					flag4 = true;
				}
				if (!(flag & flag2 & flag3) || flag4)
				{
					num2++;
					_ = 8;
				}
			}
			if (num2 != 0)
			{
				_ = 8;
			}
		}
		catch (Exception)
		{
		}
	}

	public static bool TrySubscribeMonster(zbMonster monster)
	{
		if (!kEnabled || (Object)(object)monster == (Object)null)
		{
			return false;
		}
		zbCombatData combatData = monster.combatData;
		SkeletonRenderer skeletonRenderer = monster.m_skeletonRenderer;
		if (combatData == null || (Object)(object)skeletonRenderer == (Object)null)
		{
			return false;
		}
		zbCombatSystem instance = zbCombatSystem.Instance;
		if (instance == null)
		{
			return false;
		}
		if (!kPvpEnabled && instance.isPVPBattleType)
		{
			Inc("合并排除-PVP阶段闸");
			return false;
		}
		if (monster.isEliteBoss)
		{
			Inc("合并排除-精英boss");
			return false;
		}
		if (instance.bMonsterNeedWaterEffect)
		{
			Inc("合并排除-水场景");
			return false;
		}
		if (monster.isUsingSprite)
		{
			Inc("合并排除-换图模式");
			return false;
		}
		SkeletonAnimation val = (SkeletonAnimation)(object)((skeletonRenderer is SkeletonAnimation) ? skeletonRenderer : null);
		if (val == null || ((SkeletonRenderer)val).skeleton == null)
		{
			Inc("合并排除-骨架无效");
			return false;
		}
		if (!((SkeletonRenderer)val).singleSubmesh)
		{
			Inc("合并排除-多材质");
			return false;
		}
		if (!zbCombatAtlasMerger.Instance.IsMergedAsset(((SkeletonRenderer)val).skeletonDataAsset))
		{
			Inc("合并排除-非合并资产");
			return false;
		}
		if ((Object)(object)((Component)skeletonRenderer).GetComponent<SkeletonRenderSeparator>() != (Object)null)
		{
			Inc("合并排除-separator");
			return false;
		}
		zbSpineBatchRenderer zbSpineBatchRenderer2 = combatData.EnsureSpineBatchRenderer(((Component)skeletonRenderer).gameObject.layer);
		if (zbSpineBatchRenderer2 == null)
		{
			return false;
		}
		if (zbSpineBatchRenderer2.Subscribe(skeletonRenderer, monster))
		{
			monster.m_spineBatchTakenOver = true;
			Inc("合并订阅");
			return true;
		}
		return false;
	}

	public static void UnsubscribeMonster(zbMonster monster)
	{
		((((Object)(object)monster != (Object)null) ? monster.combatData : null)?.spineBatchRenderer)?.Unsubscribe(((Object)(object)monster != (Object)null) ? monster.m_skeletonRenderer : null);
	}

	private static void Inc(string key)
	{
		zbPerfLog.Inc(key, 1L);
	}
}
