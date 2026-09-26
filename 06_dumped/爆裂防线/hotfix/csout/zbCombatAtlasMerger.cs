using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbCombatAtlasMerger
{
	private class MergedGroup
	{
		public string key;

		public readonly Dictionary<string, SkeletonDataAsset> mergedByPath = new Dictionary<string, SkeletonDataAsset>();

		public RenderTexture mergedTexture;

		public Material mergedMaterial;

		public Material mergedAdditiveMaterial;

		public readonly List<Object> runtimeObjects = new List<Object>();
	}

	private class MergeEntry
	{
		public string spinePath;

		public SkeletonDataAsset originalSda;

		public SpineAtlasAsset originalAtlasAsset;

		public Texture2D texture;

		public bool isLowPriority;

		public bool hasAdditive;

		public bool pageAdditive;

		public Material additiveMaterialTemplate;

		public string additivePageName;

		public bool applyAdditive;
	}

	private class MergedPageMaterialLoader : TextureLoader
	{
		private readonly Material mergedMaterial;

		public MergedPageMaterialLoader(Material mergedMaterial)
		{
			this.mergedMaterial = mergedMaterial;
		}

		public void Load(AtlasPage page, string path)
		{
			page.rendererObject = mergedMaterial;
		}

		public void Unload(object texture)
		{
		}
	}

	private const bool kEnabled = true;

	private const int kPadding = 4;

	private const int kMaxAtlasSize = 2048;

	private static zbCombatAtlasMerger s_instance;

	private MergedGroup m_activeGroup;

	private MergedGroup m_cachedGroup;

	private MergedGroup m_activeEffectGroup;

	private MergedGroup m_cachedEffectGroup;

	private int m_lookupHits;

	private int m_lookupMisses;

	private readonly HashSet<string> m_missPaths = new HashSet<string>();

	private const int kCensusThreshold = 120;

	private int m_censusSpawnCounter;

	private bool m_censusDoneByThreshold;

	private readonly Dictionary<int, int> m_spawnTidCounts = new Dictionary<int, int>();

	public static zbCombatAtlasMerger Instance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = new zbCombatAtlasMerger();
			}
			return s_instance;
		}
	}

	public void Prepare(List<string> spinePaths)
	{
		if (m_lookupHits > 0 || m_lookupMisses > 0)
		{
			zbPerfLog.Log($"[zbCombatAtlasMerger] 上一场 RefreshSpine 取材统计：合并命中 {m_lookupHits} 次，回退原资产 {m_lookupMisses} 次，回退怪型 {m_missPaths.Count} 种");
			foreach (string missPath in m_missPaths)
			{
				zbPerfLog.Log("[zbCombatAtlasMerger]   上一场回退怪型：" + missPath);
			}
		}
		m_lookupHits = 0;
		m_lookupMisses = 0;
		m_missPaths.Clear();
		m_censusSpawnCounter = 0;
		m_censusDoneByThreshold = false;
		if (m_spawnTidCounts.Count > 0)
		{
			int num = 0;
			foreach (int value in m_spawnTidCounts.Values)
			{
				num += value;
			}
			zbPerfLog.Log($"[zbCombatAtlasMerger] 上一场出生统计（含 RoundList 外来源）：{m_spawnTidCounts.Count} 怪型 / {num} 只");
			List<KeyValuePair<int, int>> list = new List<KeyValuePair<int, int>>(m_spawnTidCounts);
			list.Sort((KeyValuePair<int, int> a, KeyValuePair<int, int> b) => b.Value.CompareTo(a.Value));
			foreach (KeyValuePair<int, int> item in list)
			{
				zbPerfLog.Log($"[zbCombatAtlasMerger]   上一场出生：tid={item.Key} × {item.Value}");
			}
		}
		m_spawnTidCounts.Clear();
		m_activeGroup = null;
		if (spinePaths == null)
		{
			return;
		}
		List<string> list2 = new List<string>(new HashSet<string>(spinePaths));
		list2.RemoveAll(string.IsNullOrEmpty);
		list2.Sort();
		if (list2.Count < 2)
		{
			DestroyGroup(m_cachedGroup);
			m_cachedGroup = null;
			return;
		}
		string text = string.Join("|", list2.ToArray());
		if (m_cachedGroup != null && m_cachedGroup.key == text && (Object)(object)m_cachedGroup.mergedTexture != (Object)null)
		{
			m_activeGroup = m_cachedGroup;
			zbPerfLog.Log($"[zbCombatAtlasMerger] 复用合并组：{m_cachedGroup.mergedByPath.Count} 怪型，纹理 {((Texture)m_cachedGroup.mergedTexture).width}x{((Texture)m_cachedGroup.mergedTexture).height}");
			return;
		}
		DestroyGroup(m_cachedGroup);
		m_cachedGroup = null;
		MergedGroup mergedGroup = BuildGroup(text, list2, "怪型", 2);
		if (mergedGroup != null)
		{
			m_activeGroup = mergedGroup;
			m_cachedGroup = mergedGroup;
		}
	}

	public Material GetActiveMergedMaterial()
	{
		if (m_activeGroup == null)
		{
			return null;
		}
		return m_activeGroup.mergedMaterial;
	}

	public Material GetActiveMergedAdditiveMaterial()
	{
		if (m_activeGroup == null)
		{
			return null;
		}
		return m_activeGroup.mergedAdditiveMaterial;
	}

	public bool IsMergedAsset(SkeletonDataAsset sda)
	{
		MergedGroup activeGroup = m_activeGroup;
		if (activeGroup == null || (Object)(object)sda == (Object)null)
		{
			return false;
		}
		foreach (KeyValuePair<string, SkeletonDataAsset> item in activeGroup.mergedByPath)
		{
			if (item.Value == sda)
			{
				return true;
			}
		}
		return false;
	}

	public SkeletonDataAsset GetMerged(string spinePath)
	{
		MergedGroup activeGroup = m_activeGroup;
		if (activeGroup == null || string.IsNullOrEmpty(spinePath))
		{
			return null;
		}
		if (activeGroup.mergedByPath.TryGetValue(spinePath, out var value) && (Object)(object)value != (Object)null)
		{
			m_lookupHits++;
			return value;
		}
		m_lookupMisses++;
		m_missPaths.Add(spinePath);
		return null;
	}

	public void Clear()
	{
		if (m_activeGroup != null && m_activeGroup != m_cachedGroup)
		{
			DestroyGroup(m_activeGroup);
		}
		DestroyGroup(m_cachedGroup);
		m_activeGroup = null;
		m_cachedGroup = null;
		if (m_activeEffectGroup != null && m_activeEffectGroup != m_cachedEffectGroup)
		{
			DestroyGroup(m_activeEffectGroup);
		}
		DestroyGroup(m_cachedEffectGroup);
		m_activeEffectGroup = null;
		m_cachedEffectGroup = null;
	}

	public void PrepareEffect(List<string> spinePaths, List<string> lowPriorityPaths = null)
	{
		m_activeEffectGroup = null;
		if (spinePaths == null)
		{
			return;
		}
		List<string> list = new List<string>(new HashSet<string>(spinePaths));
		list.RemoveAll(string.IsNullOrEmpty);
		list.Sort();
		List<string> list2 = ((lowPriorityPaths != null) ? new List<string>(new HashSet<string>(lowPriorityPaths)) : null);
		list2?.RemoveAll(string.IsNullOrEmpty);
		list2?.Sort();
		if (list.Count == 0)
		{
			DestroyGroup(m_cachedEffectGroup);
			m_cachedEffectGroup = null;
			return;
		}
		string text = "effect:" + string.Join("|", list.ToArray()) + ((list2 != null && list2.Count > 0) ? ("|#low:" + string.Join("|", list2.ToArray())) : "");
		if (m_cachedEffectGroup != null && m_cachedEffectGroup.key == text && (Object)(object)m_cachedEffectGroup.mergedTexture != (Object)null)
		{
			m_activeEffectGroup = m_cachedEffectGroup;
			zbPerfLog.Log($"[zbCombatAtlasMerger] 复用特效合并组：{m_cachedEffectGroup.mergedByPath.Count} 特效型，纹理 {((Texture)m_cachedEffectGroup.mergedTexture).width}x{((Texture)m_cachedEffectGroup.mergedTexture).height}");
			return;
		}
		DestroyGroup(m_cachedEffectGroup);
		m_cachedEffectGroup = null;
		MergedGroup mergedGroup = BuildGroup(text, list, "特效型", 1, list2);
		if (mergedGroup != null)
		{
			m_activeEffectGroup = mergedGroup;
			m_cachedEffectGroup = mergedGroup;
		}
	}

	public Material GetEffectMergedMaterial()
	{
		if (m_activeEffectGroup == null)
		{
			return null;
		}
		return m_activeEffectGroup.mergedMaterial;
	}

	public Material GetEffectMergedAdditiveMaterial()
	{
		if (m_activeEffectGroup == null)
		{
			return null;
		}
		return m_activeEffectGroup.mergedAdditiveMaterial;
	}

	public bool IsEffectMergedAsset(SkeletonDataAsset sda)
	{
		MergedGroup activeEffectGroup = m_activeEffectGroup;
		if (activeEffectGroup == null || (Object)(object)sda == (Object)null)
		{
			return false;
		}
		foreach (KeyValuePair<string, SkeletonDataAsset> item in activeEffectGroup.mergedByPath)
		{
			if (item.Value == sda)
			{
				return true;
			}
		}
		return false;
	}

	public SkeletonDataAsset GetEffectMerged(string spinePath)
	{
		MergedGroup activeEffectGroup = m_activeEffectGroup;
		if (activeEffectGroup == null || string.IsNullOrEmpty(spinePath))
		{
			return null;
		}
		if (activeEffectGroup.mergedByPath.TryGetValue(spinePath, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		return null;
	}

	public void NotifyMonsterSpawned(int monsterTid)
	{
		m_spawnTidCounts.TryGetValue(monsterTid, out var value);
		m_spawnTidCounts[monsterTid] = value + 1;
		if (!m_censusDoneByThreshold && ++m_censusSpawnCounter >= 120)
		{
			m_censusDoneByThreshold = true;
			DumpRuntimeCoverage();
		}
	}

	public void DumpRuntimeCoverage()
	{
		SkeletonRenderer[] array = Object.FindObjectsOfType<SkeletonRenderer>(false);
		if (array.Length == 0)
		{
			return;
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		int num = 0;
		MergedGroup activeGroup = m_activeGroup;
		SkeletonRenderer[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			MeshRenderer component = ((Component)array2[i]).GetComponent<MeshRenderer>();
			if (!((Object)(object)component == (Object)null))
			{
				Material[] sharedMaterials = ((Renderer)component).sharedMaterials;
				Material val = ((sharedMaterials != null && sharedMaterials.Length != 0) ? sharedMaterials[0] : null);
				string key = (((Object)(object)val != (Object)null) ? ((Object)val).name : "<无材质>");
				dictionary.TryGetValue(key, out var value);
				dictionary[key] = value + 1;
				if (activeGroup != null && (val == activeGroup.mergedMaterial || val == activeGroup.mergedAdditiveMaterial))
				{
					num++;
				}
			}
		}
		zbPerfLog.Log($"[zbCombatAtlasMerger] 运行时渲染普查：活跃 SkeletonRenderer {array.Length} 个，其中使用合并材质 {num} 个（其余为英雄/特效/头像/被排除怪型，本就不在合并范围）");
		List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>(dictionary);
		list.Sort((KeyValuePair<string, int> a, KeyValuePair<string, int> b) => b.Value.CompareTo(a.Value));
		foreach (KeyValuePair<string, int> item in list)
		{
			zbPerfLog.Log($"[zbCombatAtlasMerger]   材质 {item.Key} × {item.Value} 个渲染器");
		}
	}

	private static void LogExclusions(List<KeyValuePair<string, string>> excludedReasons)
	{
		if (excludedReasons == null || excludedReasons.Count == 0)
		{
			return;
		}
		zbPerfLog.Log($"[zbCombatAtlasMerger] 排除怪型 {excludedReasons.Count} 种（回退原材质，不影响渲染正确性）：");
		foreach (KeyValuePair<string, string> excludedReason in excludedReasons)
		{
			zbPerfLog.Log("[zbCombatAtlasMerger]   排除：" + excludedReason.Key + "（" + excludedReason.Value + "）");
		}
	}

	private MergedGroup BuildGroup(string groupKey, List<string> sortedPaths, string entryLabel, int minEntries, List<string> lowPriorityPaths = null)
	{
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Expected O, but got Unknown
		//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
		Stopwatch stopwatch = Stopwatch.StartNew();
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		List<MergeEntry> list = new List<MergeEntry>();
		List<KeyValuePair<string, string>> list2 = new List<KeyValuePair<string, string>>();
		foreach (string sortedPath in sortedPaths)
		{
			MergeEntry mergeEntry = TryCreateEntry(sortedPath, out var excludeReason);
			if (mergeEntry != null)
			{
				list.Add(mergeEntry);
			}
			else if (excludeReason != null)
			{
				list2.Add(new KeyValuePair<string, string>(sortedPath, excludeReason));
			}
		}
		if (lowPriorityPaths != null)
		{
			HashSet<string> hashSet = new HashSet<string>();
			foreach (MergeEntry item in list)
			{
				hashSet.Add(item.spinePath);
			}
			foreach (string lowPriorityPath in lowPriorityPaths)
			{
				if (!string.IsNullOrEmpty(lowPriorityPath) && !hashSet.Contains(lowPriorityPath))
				{
					MergeEntry mergeEntry2 = TryCreateEntry(lowPriorityPath, out var excludeReason2);
					if (mergeEntry2 != null)
					{
						mergeEntry2.isLowPriority = true;
						hashSet.Add(mergeEntry2.spinePath);
						list.Add(mergeEntry2);
					}
					else if (excludeReason2 != null)
					{
						list2.Add(new KeyValuePair<string, string>(lowPriorityPath, excludeReason2));
					}
				}
			}
		}
		int num5 = 0;
		List<string> list3 = new List<string>();
		foreach (MergeEntry item2 in list)
		{
			if (item2.pageAdditive)
			{
				num5++;
				list3.Add((item2.spinePath != null) ? item2.spinePath.Substring(item2.spinePath.LastIndexOf('/') + 1) : "?");
			}
		}
		if (num5 > 0)
		{
			zbPerfLog.Log(string.Format("[zbCombatAtlasMerger] 页级叠加条目 {0} 个（克隆页改指合并叠加材质）：{1}", num5, string.Join(",", list3.ToArray())));
		}
		num = stopwatch.Elapsed.TotalMilliseconds;
		if (list.Count < minEntries)
		{
			zbPerfLog.Log($"[zbCombatAtlasMerger] 可合并{entryLabel}不足（{list.Count}/{sortedPaths.Count}），跳过合并");
			LogExclusions(list2);
			return null;
		}
		list.Sort((MergeEntry a, MergeEntry b) => (a.isLowPriority != b.isLowPriority) ? (a.isLowPriority ? 1 : (-1)) : ((Texture)b.texture).height.CompareTo(((Texture)a.texture).height));
		int num6 = 1024;
		RectInt[] array = TrySkylinePack(list, 1024);
		if (CountPlaced(array) < list.Count)
		{
			num6 = 2048;
			array = TrySkylinePack(list, 2048);
			for (int num7 = 0; num7 < list.Count; num7++)
			{
				if (((RectInt)(ref array[num7])).width == 0)
				{
					list2.Add(new KeyValuePair<string, string>(list[num7].spinePath, string.Format("合并纹理 {0}x{1} 放不下（图集 {2}x{3}）", new object[4]
					{
						2048,
						2048,
						((Texture)list[num7].texture).width,
						((Texture)list[num7].texture).height
					})));
				}
			}
		}
		LogExclusions(list2);
		num2 = stopwatch.Elapsed.TotalMilliseconds - num;
		MergedGroup mergedGroup = new MergedGroup
		{
			key = groupKey
		};
		mergedGroup.mergedTexture = CreateMergedTexture(num6, list, array, ((AtlasAssetBase)list[0].originalAtlasAsset).PrimaryMaterial, out var blitMaterial, out mergedGroup.mergedMaterial);
		if ((Object)(object)mergedGroup.mergedTexture == (Object)null || (Object)(object)mergedGroup.mergedMaterial == (Object)null)
		{
			if ((Object)(object)mergedGroup.mergedTexture != (Object)null)
			{
				mergedGroup.mergedTexture.Release();
				Object.Destroy((Object)(object)mergedGroup.mergedTexture);
			}
			if ((Object)(object)mergedGroup.mergedMaterial != (Object)null)
			{
				Object.Destroy((Object)(object)mergedGroup.mergedMaterial);
			}
			if ((Object)(object)blitMaterial != (Object)null)
			{
				Object.Destroy((Object)(object)blitMaterial);
			}
			zbPerfLog.LogWarning("[zbCombatAtlasMerger] 合并纹理创建失败，跳过合并");
			return null;
		}
		Material val = null;
		for (int num8 = 0; num8 < list.Count; num8++)
		{
			if (((RectInt)(ref array[num8])).width > 0 && (list[num8].hasAdditive || list[num8].pageAdditive))
			{
				val = new Material(list[num8].additiveMaterialTemplate)
				{
					name = "zbCombatMergedAtlas_Material_Additive"
				};
				val.mainTexture = (Texture)(object)mergedGroup.mergedTexture;
				mergedGroup.mergedAdditiveMaterial = val;
				break;
			}
		}
		num3 = stopwatch.Elapsed.TotalMilliseconds - num - num2;
		for (int num9 = 0; num9 < list.Count; num9++)
		{
			if (((RectInt)(ref array[num9])).width != 0)
			{
				SkeletonDataAsset val2 = BuildMergedSkeletonDataAsset(list[num9], mergedGroup.mergedMaterial, val, array[num9], num6, mergedGroup);
				if (!((Object)(object)val2 == (Object)null))
				{
					mergedGroup.mergedByPath[list[num9].spinePath] = val2;
				}
			}
		}
		Object.Destroy((Object)(object)blitMaterial);
		if (mergedGroup.mergedByPath.Count < minEntries)
		{
			zbPerfLog.LogWarning("[zbCombatAtlasMerger] 合并克隆全部失败，跳过合并");
			DestroyGroup(mergedGroup);
			return null;
		}
		zbPerfLog.Log(string.Format("[zbCombatAtlasMerger] 构建合并组：{0}/{1} {2}，纹理 {3}x{4}", new object[5]
		{
			mergedGroup.mergedByPath.Count,
			sortedPaths.Count,
			entryLabel,
			num6,
			num6
		}));
		num4 = stopwatch.Elapsed.TotalMilliseconds - num - num2 - num3;
		zbPerfLog.Log(string.Format("[zbCombatAtlasMerger] 构建耗时：加载筛选 {0:0.#} ms / 装箱 {1:0.#} ms / GL打包+材质 {2:0.#} ms / 克隆重映射 {3:0.#} ms / 合计 {4:0.#} ms", new object[5]
		{
			num,
			num2,
			num3,
			num4,
			stopwatch.Elapsed.TotalMilliseconds
		}));
		StringBuilder stringBuilder = new StringBuilder(256);
		for (int num10 = 0; num10 < list.Count; num10++)
		{
			if (((RectInt)(ref array[num10])).width > 0)
			{
				stringBuilder.Append(Path.GetFileNameWithoutExtension(list[num10].spinePath)).Append('@').Append(((RectInt)(ref array[num10])).x)
					.Append(',')
					.Append(((RectInt)(ref array[num10])).y)
					.Append('(')
					.Append(((RectInt)(ref array[num10])).width)
					.Append('x')
					.Append(((RectInt)(ref array[num10])).height)
					.Append(");");
			}
		}
		zbPerfLog.Log($"[zbCombatAtlasMerger] 布局指纹@{num6}：{stringBuilder}");
		return mergedGroup;
	}

	private MergeEntry TryCreateEntry(string spinePath, out string excludeReason)
	{
		excludeReason = null;
		SkeletonDataAsset val = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(spinePath);
		if ((Object)(object)val == (Object)null)
		{
			excludeReason = "SkeletonDataAsset 加载失败";
			return null;
		}
		if (val.atlasAssets == null || val.atlasAssets.Length != 1)
		{
			excludeReason = ((val.atlasAssets == null) ? "atlasAssets 为空" : $"引用了 {val.atlasAssets.Length} 个图集资产（仅支持 1）");
			return null;
		}
		AtlasAssetBase obj = val.atlasAssets[0];
		SpineAtlasAsset val2 = (SpineAtlasAsset)(object)((obj is SpineAtlasAsset) ? obj : null);
		if (val2 == null || (Object)(object)val2.atlasFile == (Object)null || (Object)(object)((AtlasAssetBase)val2).PrimaryMaterial == (Object)null || (Object)(object)val.skeletonJSON == (Object)null)
		{
			excludeReason = "图集资产不完整（atlasFile/PrimaryMaterial/skeletonJSON 缺失）";
			return null;
		}
		Texture mainTexture = ((AtlasAssetBase)val2).PrimaryMaterial.mainTexture;
		Texture2D val3 = (Texture2D)(object)((mainTexture is Texture2D) ? mainTexture : null);
		if ((Object)(object)val3 == (Object)null)
		{
			excludeReason = "图集材质无 Texture2D";
			return null;
		}
		if (((Texture)val3).width + 8 > 2048 || ((Texture)val3).height + 8 > 2048)
		{
			excludeReason = $"图集纹理 {((Texture)val3).width}x{((Texture)val3).height} 超过合并上限 {2048}";
			return null;
		}
		Atlas atlas = ((AtlasAssetBase)val2).GetAtlas(false);
		if (atlas == null || atlas.Regions == null || atlas.Regions.Count == 0)
		{
			excludeReason = "图集解析失败或无 region";
			return null;
		}
		AtlasPage page = atlas.Regions[0].page;
		foreach (AtlasRegion region in atlas.Regions)
		{
			if (region.page != page)
			{
				excludeReason = "多页图集（仅支持单页）";
				return null;
			}
		}
		if (!TryResolveAdditive(val, page.name, out var additiveTemplate, out var additivePageName, out var applyAdditive))
		{
			excludeReason = "混合模式材质不支持（multiply/screen 或叠加材质配置异常）";
			return null;
		}
		bool flag = false;
		Material primaryMaterial = ((AtlasAssetBase)val2).PrimaryMaterial;
		if ((Object)(object)primaryMaterial != (Object)null && (Object)(object)primaryMaterial.shader != (Object)null && !string.IsNullOrEmpty(((Object)primaryMaterial.shader).name) && ((Object)primaryMaterial.shader).name.IndexOf("Additive", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			flag = true;
			additiveTemplate = primaryMaterial;
		}
		SkeletonData skeletonData = val.GetSkeletonData(true);
		if (skeletonData == null)
		{
			excludeReason = "骨骼数据解析失败";
			return null;
		}
		if (HasSequenceAttachments(skeletonData))
		{
			excludeReason = "使用 Sequence 序列帧附件";
			return null;
		}
		return new MergeEntry
		{
			spinePath = spinePath,
			originalSda = val,
			originalAtlasAsset = val2,
			texture = val3,
			hasAdditive = ((Object)(object)additiveTemplate != (Object)null && !flag),
			pageAdditive = flag,
			additiveMaterialTemplate = additiveTemplate,
			additivePageName = additivePageName,
			applyAdditive = applyAdditive
		};
	}

	private static bool TryResolveAdditive(SkeletonDataAsset sda, string pageName, out Material additiveTemplate, out string additivePageName, out bool applyAdditive)
	{
		additiveTemplate = null;
		additivePageName = null;
		applyAdditive = false;
		BlendModeMaterials blendModeMaterials = sda.blendModeMaterials;
		if (blendModeMaterials == null)
		{
			return true;
		}
		if ((blendModeMaterials.multiplyMaterials != null && blendModeMaterials.multiplyMaterials.Count > 0) || (blendModeMaterials.screenMaterials != null && blendModeMaterials.screenMaterials.Count > 0))
		{
			return false;
		}
		List<ReplacementMaterial> additiveMaterials = blendModeMaterials.additiveMaterials;
		if (additiveMaterials == null || additiveMaterials.Count == 0)
		{
			return !blendModeMaterials.RequiresBlendModeMaterials;
		}
		if (additiveMaterials.Count > 1)
		{
			return false;
		}
		ReplacementMaterial val = additiveMaterials[0];
		if ((Object)(object)val.material == (Object)null || val.pageName != pageName)
		{
			return false;
		}
		additiveTemplate = val.material;
		additivePageName = val.pageName;
		applyAdditive = blendModeMaterials.applyAdditiveMaterial;
		return true;
	}

	private static bool HasSequenceAttachments(SkeletonData skeletonData)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		List<SkinEntry> list = new List<SkinEntry>();
		Enumerator<Skin> enumerator = skeletonData.Skins.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Skin current = enumerator.Current;
				int i = 0;
				for (int count = skeletonData.Slots.Count; i < count; i++)
				{
					list.Clear();
					current.GetAttachments(i, list);
					foreach (SkinEntry item in list)
					{
						SkinEntry current2 = item;
						Attachment attachment = ((SkinEntry)(ref current2)).Attachment;
						RegionAttachment val = (RegionAttachment)(object)((attachment is RegionAttachment) ? attachment : null);
						if (val != null && val.Sequence != null)
						{
							return true;
						}
						Attachment attachment2 = ((SkinEntry)(ref current2)).Attachment;
						MeshAttachment val2 = (MeshAttachment)(object)((attachment2 is MeshAttachment) ? attachment2 : null);
						if (val2 != null && val2.Sequence != null)
						{
							return true;
						}
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return false;
	}

	private static RectInt[] TrySkylinePack(List<MergeEntry> entries, int atlasSize)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		RectInt[] array = (RectInt[])(object)new RectInt[entries.Count];
		List<int> list = new List<int> { 4 };
		List<int> list2 = new List<int> { 4 };
		int num = atlasSize - 4;
		for (int i = 0; i < entries.Count; i++)
		{
			int width = ((Texture)entries[i].texture).width;
			int height = ((Texture)entries[i].texture).height;
			if (width + 8 > atlasSize || height + 8 > atlasSize)
			{
				array[i] = new RectInt(0, 0, 0, 0);
				continue;
			}
			int num2 = width + 4;
			int num3 = height + 4;
			int num4 = -1;
			int num5 = int.MaxValue;
			for (int j = 0; j < list.Count; j++)
			{
				int num6 = list[j];
				if (num6 + num2 > num)
				{
					break;
				}
				int num7 = 0;
				for (int k = j; k < list.Count && list[k] < num6 + num2; k++)
				{
					if (list2[k] > num7)
					{
						num7 = list2[k];
					}
				}
				if (num7 + num3 <= num && num7 < num5)
				{
					num5 = num7;
					num4 = num6;
				}
			}
			if (num4 < 0)
			{
				array[i] = new RectInt(0, 0, 0, 0);
				continue;
			}
			array[i] = new RectInt(num4, num5, width, height);
			int num8 = num4 + num2;
			int y = num5 + num3;
			List<int> list3 = new List<int>();
			List<int> list4 = new List<int>();
			bool flag = false;
			for (int l = 0; l < list.Count; l++)
			{
				int num9 = list[l];
				int num10 = ((l + 1 < list.Count) ? list[l + 1] : num);
				if (!flag && num10 > num4)
				{
					if (num9 < num4)
					{
						AppendSeg(list3, list4, num9, list2[l]);
					}
					AppendSeg(list3, list4, num4, y);
					flag = true;
					if (num10 > num8)
					{
						AppendSeg(list3, list4, num8, list2[l]);
					}
				}
				else if (!flag)
				{
					AppendSeg(list3, list4, num9, list2[l]);
				}
				else if (num10 > num8)
				{
					AppendSeg(list3, list4, (num9 < num8) ? num8 : num9, list2[l]);
				}
			}
			list = list3;
			list2 = list4;
		}
		return array;
	}

	private static void AppendSeg(List<int> segX, List<int> segY, int x, int y)
	{
		if (segX.Count <= 0 || segY[segY.Count - 1] != y)
		{
			segX.Add(x);
			segY.Add(y);
		}
	}

	private static int CountPlaced(RectInt[] placements)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < placements.Length; i++)
		{
			RectInt val = placements[i];
			if (((RectInt)(ref val)).width > 0)
			{
				num++;
			}
		}
		return num;
	}

	private static RenderTexture CreateMergedTexture(int atlasSize, List<MergeEntry> entries, RectInt[] placements, Material materialTemplate, out Material blitMaterial, out Material mergedMaterial)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		blitMaterial = null;
		mergedMaterial = null;
		RenderTexture val = new RenderTexture(atlasSize, atlasSize, 0, (RenderTextureFormat)0)
		{
			name = "zbCombatMergedAtlas",
			filterMode = (FilterMode)1,
			wrapMode = (TextureWrapMode)1,
			anisoLevel = 1,
			useMipMap = false
		};
		val.Create();
		mergedMaterial = new Material(materialTemplate)
		{
			name = "zbCombatMergedAtlas_Material"
		};
		mergedMaterial.mainTexture = (Texture)(object)val;
		blitMaterial = new Material(mergedMaterial);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = val;
		GL.Clear(false, true, Color.clear);
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, (float)atlasSize, 0f, (float)atlasSize);
		for (int i = 0; i < entries.Count; i++)
		{
			if (((RectInt)(ref placements[i])).width != 0)
			{
				blitMaterial.mainTexture = (Texture)(object)entries[i].texture;
				blitMaterial.SetPass(0);
				RectInt val2 = placements[i];
				GL.Begin(7);
				GL.Color(new Color(1f, 1f, 1f, 1f));
				GL.TexCoord2(0f, 0f);
				GL.Vertex3((float)((RectInt)(ref val2)).x, (float)((RectInt)(ref val2)).y, 0f);
				GL.TexCoord2(1f, 0f);
				GL.Vertex3((float)(((RectInt)(ref val2)).x + ((RectInt)(ref val2)).width), (float)((RectInt)(ref val2)).y, 0f);
				GL.TexCoord2(1f, 1f);
				GL.Vertex3((float)(((RectInt)(ref val2)).x + ((RectInt)(ref val2)).width), (float)(((RectInt)(ref val2)).y + ((RectInt)(ref val2)).height), 0f);
				GL.TexCoord2(0f, 1f);
				GL.Vertex3((float)((RectInt)(ref val2)).x, (float)(((RectInt)(ref val2)).y + ((RectInt)(ref val2)).height), 0f);
				GL.End();
			}
		}
		GL.PopMatrix();
		RenderTexture.active = active;
		return val;
	}

	private static SkeletonDataAsset BuildMergedSkeletonDataAsset(MergeEntry entry, Material mergedMaterial, Material mergedAdditiveMaterial, RectInt placement, int atlasSize, MergedGroup group)
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Expected O, but got Unknown
		SpineAtlasAsset val;
		try
		{
			bool flag = entry.pageAdditive && (Object)(object)mergedAdditiveMaterial != (Object)null;
			if (entry.pageAdditive && !flag)
			{
				zbPerfLog.LogWarning("[zbCombatAtlasMerger] 页级叠加条目无合并叠加材质，回退原资产：" + entry.spinePath);
				return null;
			}
			Material pageTargetMaterial = (flag ? mergedAdditiveMaterial : mergedMaterial);
			val = SpineAtlasAsset.CreateRuntimeInstance(entry.originalAtlasAsset.atlasFile, (Material[])(object)new Material[1] { pageTargetMaterial }, false, (Func<SpineAtlasAsset, TextureLoader>)((SpineAtlasAsset a) => (TextureLoader)(object)new MergedPageMaterialLoader(pageTargetMaterial)));
			Atlas atlas = ((AtlasAssetBase)val).GetAtlas(false);
			if (atlas == null)
			{
				Object.Destroy((Object)(object)val);
				return null;
			}
			RemapAtlasRegions(atlas, placement, atlasSize);
		}
		catch (Exception ex)
		{
			zbPerfLog.LogError("[zbCombatAtlasMerger] 克隆图集资产失败：" + entry.spinePath + "，" + ex.Message);
			return null;
		}
		group.runtimeObjects.Add((Object)(object)val);
		try
		{
			SkeletonDataAsset val2 = SkeletonDataAsset.CreateRuntimeInstance(entry.originalSda.skeletonJSON, (AtlasAssetBase[])(object)new AtlasAssetBase[1] { (AtlasAssetBase)val }, false, entry.originalSda.scale);
			val2.defaultMix = entry.originalSda.defaultMix;
			val2.fromAnimation = entry.originalSda.fromAnimation;
			val2.toAnimation = entry.originalSda.toAnimation;
			val2.duration = entry.originalSda.duration;
			if (entry.hasAdditive && (Object)(object)mergedAdditiveMaterial != (Object)null)
			{
				BlendModeMaterials blendModeMaterials = val2.blendModeMaterials;
				blendModeMaterials.RequiresBlendModeMaterials = true;
				blendModeMaterials.applyAdditiveMaterial = entry.applyAdditive;
				blendModeMaterials.additiveMaterials = new List<ReplacementMaterial>
				{
					new ReplacementMaterial
					{
						pageName = entry.additivePageName,
						material = mergedAdditiveMaterial
					}
				};
			}
			if (val2.GetSkeletonData(true) == null)
			{
				zbPerfLog.LogError("[zbCombatAtlasMerger] 合并版骨骼解析失败：" + entry.spinePath);
				Object.Destroy((Object)(object)val2);
				return null;
			}
			return val2;
		}
		catch (Exception ex2)
		{
			zbPerfLog.LogError("[zbCombatAtlasMerger] 克隆骨骼资产失败：" + entry.spinePath + "，" + ex2.Message);
			return null;
		}
	}

	private static void RemapAtlasRegions(Atlas atlas, RectInt placement, int atlasSize)
	{
		float num = ((RectInt)(ref placement)).x;
		float num2 = ((RectInt)(ref placement)).y;
		float num3 = ((RectInt)(ref placement)).width;
		float num4 = ((RectInt)(ref placement)).height;
		float num5 = atlasSize;
		foreach (AtlasRegion region in atlas.Regions)
		{
			((TextureRegion)region).u = (num + ((TextureRegion)region).u * num3) / num5;
			((TextureRegion)region).u2 = (num + ((TextureRegion)region).u2 * num3) / num5;
			((TextureRegion)region).v = (num2 + ((TextureRegion)region).v * num4) / num5;
			((TextureRegion)region).v2 = (num2 + ((TextureRegion)region).v2 * num4) / num5;
		}
	}

	private void DestroyGroup(MergedGroup group)
	{
		if (group == null)
		{
			return;
		}
		foreach (Object runtimeObject in group.runtimeObjects)
		{
			if (runtimeObject != (Object)null)
			{
				Object.Destroy(runtimeObject);
			}
		}
		group.runtimeObjects.Clear();
		group.mergedByPath.Clear();
		if ((Object)(object)group.mergedMaterial != (Object)null)
		{
			Object.Destroy((Object)(object)group.mergedMaterial);
			group.mergedMaterial = null;
		}
		if ((Object)(object)group.mergedAdditiveMaterial != (Object)null)
		{
			Object.Destroy((Object)(object)group.mergedAdditiveMaterial);
			group.mergedAdditiveMaterial = null;
		}
		if ((Object)(object)group.mergedTexture != (Object)null)
		{
			group.mergedTexture.Release();
			Object.Destroy((Object)(object)group.mergedTexture);
			group.mergedTexture = null;
		}
		zbMonsterSpineMaterialUtil.ClearCache();
	}
}
