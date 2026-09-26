using System.Collections.Generic;
using System.Text;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;

public static class zbSpineGpuAnimRuntime
{
	public static readonly bool kEnabled = true;

	public static readonly bool kPvpEnabled = true;

	public const float kAnimFps = 30f;

	public const int kPreRollLoops = 2;

	public const bool kReconvertEnabled = true;

	public const float kReconvertCooldownSeconds = 3f;

	private static readonly HashSet<int> kBodyAnimSkillIds = new HashSet<int>
	{
		30111, 30112, 30113, 30121, 30122, 30123, 30131, 30132, 30133, 30141,
		30142, 30143, 30151, 30152, 30153, 30161, 30162, 30163
	};

	private const string kCounterBakedTypes = "GPU动画烘焙怪型数";

	private const string kCounterConverted = "GPU动画转换怪数";

	private const string kCounterFallback = "GPU动画运行时回退数";

	private const string kCounterReconvert = "GPU动画重转换次数";

	private const string kCounterExcludedPvpGate = "GPU排除-PVP闸";

	private const string kCounterExcludedElite = "GPU排除-精英boss";

	private const string kCounterExcludedWater = "GPU排除-水场景";

	private const string kCounterExcludedSkill = "GPU排除-技能面";

	private const string kCounterExcludedNotBaked = "GPU排除-未烘焙型";

	private const string kCounterExcludedNotMerged = "GPU排除-非合并资产";

	private const string kCounterExcludedAdditive = "GPU排除-多材质additive";

	private const string kCounterExcludedSeparator = "GPU排除-separator";

	private const string kCounterExcludedInvalid = "GPU排除-骨架无效";

	private const string kCounterBulletConverted = "GPU弹幕转换数";

	private const string kCounterBulletExcludedPvpGate = "GPU弹幕排除-PVP闸";

	private const string kCounterBulletExcludedWater = "GPU弹幕排除-水场景";

	private const string kCounterBulletExcludedInvalid = "GPU弹幕排除-骨架无效";

	private const string kCounterBulletExcludedHost = "GPU弹幕排除-宿主无效";

	private const string kCounterBulletExcludedNotBaked = "GPU弹幕排除-未烘焙宿主型";

	private const string kCounterBulletExcludedNotMerged = "GPU弹幕排除-非合并资产";

	private const string kCounterBulletExcludedSeparator = "GPU弹幕排除-separator";

	private const string kCounterBulletExcludedAdditive = "GPU弹幕排除-多材质additive";

	private const string kCounterEffectBakedTypes = "GPU特效烘焙型数";

	private const string kCounterEffectConverted = "GPU特效转换数";

	private const string kCounterEffectExcludedPvpGate = "GPU特效排除-PVP闸";

	private const string kCounterEffectExcludedInvalid = "GPU特效排除-骨架无效";

	private const string kCounterEffectExcludedNotBaked = "GPU特效排除-未烘焙型";

	private const string kCounterEffectExcludedNotMerged = "GPU特效排除-非合并资产";

	private const string kCounterEffectExcludedSeparator = "GPU特效排除-separator";

	private const string kCounterEffectExcludedAdditive = "GPU特效排除-多材质additive";

	private const int kBakeDataVersion = 7;

	private static Dictionary<string, zbSpineGpuAnimBakedType> s_activeBaked;

	private static HashSet<string> s_derivedPaths;

	private static Dictionary<string, zbSpineGpuAnimBakedType> s_cachedBaked;

	private static string s_cachedKey;

	private static zbSpineGpuAnimRenderer.MegaBatch s_activeMega;

	private static zbSpineGpuAnimRenderer.MegaBatch s_cachedMega;

	private static string s_cachedMegaKey;

	private static Dictionary<string, zbSpineGpuAnimBakedType> s_activeEffectBaked;

	private static Dictionary<string, zbSpineGpuAnimBakedType> s_cachedEffectBaked;

	private static string s_cachedEffectBakedKey;

	private static zbSpineGpuAnimRenderer.MegaBatch s_activeEffectMega;

	private static zbSpineGpuAnimRenderer.MegaBatch s_cachedEffectMega;

	private static string s_cachedEffectMegaKey;

	private static readonly List<zbMonster> s_pendingReconvert = new List<zbMonster>();

	private static readonly List<float> s_pendingReconvertAt = new List<float>();

	private static int s_reconvertTickFrame = -1;

	public static zbSpineGpuAnimRenderer.MegaBatch ActiveMegaBatch => s_activeMega;

	public static zbSpineGpuAnimRenderer.MegaBatch ActiveEffectMegaBatch => s_activeEffectMega;

	public static void Prepare(List<string> spinePaths)
	{
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Invalid comparison between Unknown and I4
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		s_activeBaked = null;
		s_activeMega = null;
		if (!kEnabled || spinePaths == null || spinePaths.Count == 0)
		{
			return;
		}
		List<string> list = new List<string>(new HashSet<string>(spinePaths));
		list.RemoveAll(string.IsNullOrEmpty);
		list.Sort();
		List<int> list2 = CollectExtraSkillIds();
		string text = "-";
		for (int i = 0; i < list2.Count; i++)
		{
			if (IsBodyAnimSkill(list2[i]))
			{
				text = ((text == "-") ? list2[i].ToString() : (text + "," + list2[i]));
			}
		}
		string text2 = 7 + ":" + text + ":" + string.Join("|", list.ToArray());
		if (s_cachedBaked != null && s_cachedKey == text2 && s_cachedBaked.Count > 0)
		{
			bool flag = true;
			foreach (KeyValuePair<string, zbSpineGpuAnimBakedType> item in s_cachedBaked)
			{
				if (item.Value == null || (Object)(object)item.Value.animTexture == (Object)null || (int)item.Value.animTexture.format != 17)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				s_activeBaked = s_cachedBaked;
				BuildOrReuseMega(text2);
				zbPerfLog.Log($"[zbSpineGpuAnim] 复用烘焙组：{s_activeBaked.Count} 怪型");
				return;
			}
			zbPerfLog.Log("[zbSpineGpuAnim] 缓存烘焙组纹理格式过期（烘焙器已升级），丢弃重烘");
			DestroyCache();
		}
		HashSet<string> hashSet = CollectSkillExcludedPaths(list, list2);
		double num = 0.0;
		long num2 = 0L;
		Dictionary<string, zbSpineGpuAnimBakedType> dictionary = new Dictionary<string, zbSpineGpuAnimBakedType>();
		StringBuilder stringBuilder = new StringBuilder();
		Dictionary<string, int> dictionary2 = CountBornInstances(list);
		int num3 = 0;
		int num4 = 0;
		foreach (string item2 in list)
		{
			dictionary2.TryGetValue(item2, out var value);
			string text3 = ((s_derivedPaths != null && s_derivedPaths.Contains(item2)) ? "（衍生）" : "");
			if (hashSet.Contains(item2))
			{
				stringBuilder.Append("\n  排除（技能面）：" + item2 + text3);
				num4 += value;
				continue;
			}
			SkeletonDataAsset merged = zbCombatAtlasMerger.Instance.GetMerged(item2);
			if ((Object)(object)merged == (Object)null)
			{
				stringBuilder.Append("\n  排除（未命中合并组）：" + item2 + text3);
				num4 += value;
				continue;
			}
			zbSpineGpuAnimBakedType zbSpineGpuAnimBakedType2 = zbSpineGpuAnimBaker.Bake(item2, merged, 30f, 2, out var excludeReason);
			if (zbSpineGpuAnimBakedType2 == null)
			{
				stringBuilder.Append("\n  排除（烘焙自检失败-" + excludeReason + "）：" + item2);
				num4 += value;
				continue;
			}
			dictionary[item2] = zbSpineGpuAnimBakedType2;
			num3 += value;
			num += zbSpineGpuAnimBakedType2.bakeMs;
			num2 += zbSpineGpuAnimBakedType2.TextureBytes;
			List<Vector2> list3 = new List<Vector2>(2);
			if ((Object)(object)zbSpineGpuAnimBakedType2.unionMesh != (Object)null)
			{
				List<Vector2> list4 = new List<Vector2>();
				zbSpineGpuAnimBakedType2.unionMesh.GetUVs(0, list4);
				if (list4.Count > 0)
				{
					list3.Add(list4[0]);
					list3.Add(list4[list4.Count - 1]);
				}
			}
			string text4 = ((list3.Count == 2) ? string.Format(" UV0首尾=({0:0.####},{1:0.####})/({2:0.####},{3:0.####})", new object[4]
			{
				list3[0].x,
				list3[0].y,
				list3[1].x,
				list3[1].y
			}) : "");
			stringBuilder.Append(string.Format("\n  烘焙：{0}{1} 并集顶点={2} 附件={3} ", new object[4] { item2, text3, zbSpineGpuAnimBakedType2.unionVertexCount, zbSpineGpuAnimBakedType2.unionAttachmentCount }) + string.Format("clip={0} 帧={1} 纹理={2}x{3} ", new object[4]
			{
				zbSpineGpuAnimBakedType2.clips.Count,
				zbSpineGpuAnimBakedType2.totalFrames,
				((Texture)zbSpineGpuAnimBakedType2.animTexture).width,
				((Texture)zbSpineGpuAnimBakedType2.animTexture).height
			}) + string.Format("({0}KB) {1}{2:0.#}ms{3}", new object[4]
			{
				zbSpineGpuAnimBakedType2.TextureBytes / 1024,
				zbSpineGpuAnimBakedType2.hasAdditiveSubmesh ? $"[additive子网格 {zbSpineGpuAnimBakedType2.additiveTriangleCount}tri] " : "",
				zbSpineGpuAnimBakedType2.bakeMs,
				text4
			}));
		}
		if (s_cachedBaked != null && s_cachedBaked != dictionary)
		{
			DestroyCache();
		}
		if (dictionary.Count > 0)
		{
			s_activeBaked = dictionary;
			s_cachedBaked = dictionary;
			s_cachedKey = text2;
			BuildOrReuseMega(text2);
			zbPerfLog.Set("GPU动画烘焙怪型数", dictionary.Count);
			int num5 = num3 + num4;
			int num6 = ((num5 > 0) ? (100 * num4 / num5) : 0);
			zbPerfLog.Log($"[zbSpineGpuAnim] 构建烘焙组：{dictionary.Count}/{list.Count} 怪型，" + $"合计 {num:0.#}ms / {num2 / 1024}KB" + string.Format("，实例覆盖 {0}/{1}（排除 {2} 只 / {3}%）{4}", new object[5] { num3, num5, num4, num6, stringBuilder }));
		}
		else
		{
			zbPerfLog.Log($"[zbSpineGpuAnim] 无可烘焙怪型（{list.Count} 怪型全部排除，实例覆盖 0/{num3 + num4}）：{stringBuilder}");
		}
	}

	private static void BuildOrReuseMega(string groupKey)
	{
		if (s_activeBaked == null || s_activeBaked.Count == 0)
		{
			return;
		}
		if (s_cachedMega != null && s_cachedMegaKey == groupKey)
		{
			s_activeMega = s_cachedMega;
			return;
		}
		if (s_cachedMega != null)
		{
			s_cachedMega.Release();
			s_cachedMega = null;
			s_cachedMegaKey = null;
		}
		s_cachedMega = zbSpineGpuAnimRenderer.TryBuildMegaBatch(s_activeBaked, zbCombatAtlasMerger.Instance.GetActiveMergedMaterial(), zbCombatAtlasMerger.Instance.GetActiveMergedAdditiveMaterial());
		s_cachedMegaKey = groupKey;
		s_activeMega = s_cachedMega;
		if (s_cachedMega != null)
		{
			zbPerfLog.Log($"[zbSpineGpuAnim] mega 批次已组装：{s_cachedMega.typeByPath.Count} 怪型 " + $"顶点={s_cachedMega.megaMesh.vertexCount} " + $"animArray={((Texture)s_cachedMega.animArray).width}x{((Texture)s_cachedMega.animArray).height}x{s_cachedMega.animArray.depth} " + $"normalTri={s_cachedMega.normalTriangleCount} additiveTri={s_cachedMega.additiveTriangleCount}");
		}
		else
		{
			zbPerfLog.LogWarning("[zbSpineGpuAnim] mega 批次组装失败，本场怪物全部走原 spine 渲染");
		}
	}

	public static void PrepareEffect(List<string> spinePaths)
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Invalid comparison between Unknown and I4
		s_activeEffectBaked = null;
		s_activeEffectMega = null;
		if (!kEnabled || spinePaths == null || spinePaths.Count == 0)
		{
			return;
		}
		List<string> list = new List<string>(new HashSet<string>(spinePaths));
		list.RemoveAll(string.IsNullOrEmpty);
		list.Sort();
		if (list.Count == 0)
		{
			return;
		}
		string text = 7 + ":effect:" + string.Join("|", list.ToArray());
		if (s_cachedEffectBaked != null && s_cachedEffectBakedKey == text && s_cachedEffectBaked.Count > 0)
		{
			bool flag = true;
			foreach (KeyValuePair<string, zbSpineGpuAnimBakedType> item in s_cachedEffectBaked)
			{
				if (item.Value == null || (Object)(object)item.Value.animTexture == (Object)null || (int)item.Value.animTexture.format != 17)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				s_activeEffectBaked = s_cachedEffectBaked;
				BuildOrReuseEffectMega(text);
				zbPerfLog.Log($"[zbSpineGpuAnim] 复用特效烘焙组：{s_activeEffectBaked.Count} 型");
				return;
			}
			zbPerfLog.Log("[zbSpineGpuAnim] 特效缓存烘焙组纹理格式过期，丢弃重烘");
			DestroyEffectCache();
		}
		double num = 0.0;
		long num2 = 0L;
		Dictionary<string, zbSpineGpuAnimBakedType> dictionary = new Dictionary<string, zbSpineGpuAnimBakedType>();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item2 in list)
		{
			SkeletonDataAsset effectMerged = zbCombatAtlasMerger.Instance.GetEffectMerged(item2);
			if ((Object)(object)effectMerged == (Object)null)
			{
				stringBuilder.Append("\n  排除（未命中特效合并组）：" + item2);
				continue;
			}
			zbSpineGpuAnimBakedType zbSpineGpuAnimBakedType2 = zbSpineGpuAnimBaker.Bake(item2, effectMerged, 30f, 2, out var excludeReason);
			if (zbSpineGpuAnimBakedType2 == null)
			{
				stringBuilder.Append("\n  排除（烘焙自检失败-" + excludeReason + "）：" + item2);
				continue;
			}
			dictionary[item2] = zbSpineGpuAnimBakedType2;
			num += zbSpineGpuAnimBakedType2.bakeMs;
			num2 += zbSpineGpuAnimBakedType2.TextureBytes;
			stringBuilder.Append($"\n  烘焙：{item2} 并集顶点={zbSpineGpuAnimBakedType2.unionVertexCount} 附件={zbSpineGpuAnimBakedType2.unionAttachmentCount} " + string.Format("clip={0} 帧={1} 纹理={2}x{3} ", new object[4]
			{
				zbSpineGpuAnimBakedType2.clips.Count,
				zbSpineGpuAnimBakedType2.totalFrames,
				((Texture)zbSpineGpuAnimBakedType2.animTexture).width,
				((Texture)zbSpineGpuAnimBakedType2.animTexture).height
			}) + string.Format("({0}KB) {1}{2:0.#}ms", zbSpineGpuAnimBakedType2.TextureBytes / 1024, zbSpineGpuAnimBakedType2.hasAdditiveSubmesh ? $"[additive子网格 {zbSpineGpuAnimBakedType2.additiveTriangleCount}tri] " : "", zbSpineGpuAnimBakedType2.bakeMs));
		}
		if (s_cachedEffectBaked != null && s_cachedEffectBaked != dictionary)
		{
			DestroyEffectCache();
		}
		if (dictionary.Count > 0)
		{
			s_activeEffectBaked = dictionary;
			s_cachedEffectBaked = dictionary;
			s_cachedEffectBakedKey = text;
			BuildOrReuseEffectMega(text);
			zbPerfLog.Set("GPU特效烘焙型数", dictionary.Count);
			zbPerfLog.Log($"[zbSpineGpuAnim] 构建特效烘焙组：{dictionary.Count}/{list.Count} 型，" + $"合计 {num:0.#}ms / {num2 / 1024}KB{stringBuilder}");
		}
		else
		{
			zbPerfLog.Log($"[zbSpineGpuAnim] 无可烘焙特效型（{list.Count} 型全部排除）：{stringBuilder}");
		}
	}

	private static void BuildOrReuseEffectMega(string groupKey)
	{
		if (s_activeEffectBaked == null || s_activeEffectBaked.Count == 0)
		{
			return;
		}
		if (s_cachedEffectMega != null && s_cachedEffectMegaKey == groupKey)
		{
			s_activeEffectMega = s_cachedEffectMega;
			return;
		}
		if (s_cachedEffectMega != null)
		{
			s_cachedEffectMega.Release();
			s_cachedEffectMega = null;
			s_cachedEffectMegaKey = null;
		}
		s_cachedEffectMega = zbSpineGpuAnimRenderer.TryBuildMegaBatch(s_activeEffectBaked, zbCombatAtlasMerger.Instance.GetEffectMergedMaterial(), zbCombatAtlasMerger.Instance.GetEffectMergedAdditiveMaterial());
		s_cachedEffectMegaKey = groupKey;
		s_activeEffectMega = s_cachedEffectMega;
		if (s_cachedEffectMega != null)
		{
			zbPerfLog.Log($"[zbSpineGpuAnim] 特效 mega 批次已组装：{s_cachedEffectMega.typeByPath.Count} 型 " + $"顶点={s_cachedEffectMega.megaMesh.vertexCount} " + $"animArray={((Texture)s_cachedEffectMega.animArray).width}x{((Texture)s_cachedEffectMega.animArray).height}x{s_cachedEffectMega.animArray.depth} " + $"normalTri={s_cachedEffectMega.normalTriangleCount} additiveTri={s_cachedEffectMega.additiveTriangleCount}");
		}
		else
		{
			zbPerfLog.LogWarning("[zbSpineGpuAnim] 特效 mega 批次组装失败，本场特效全部走原 spine 渲染");
		}
	}

	private static HashSet<string> CollectSkillExcludedPaths(List<string> allPaths, List<int> extraSkillIds)
	{
		HashSet<string> hashSet = new HashSet<string>();
		BattleInfo battleInfo = ((zbCombatSystem.Instance != null) ? zbCombatSystem.Instance.battleInfo : null);
		if (battleInfo == null || battleInfo.MonsterInfo == null || allPaths == null || allPaths.Count == 0)
		{
			return hashSet;
		}
		Dictionary<string, zbMonsterCsvRecord> dictionary = BuildMonsterRecordByBone();
		foreach (string allPath in allPaths)
		{
			if (hashSet.Contains(allPath) || !dictionary.TryGetValue(allPath, out var value) || value.type != 1)
			{
				continue;
			}
			if (IsBodyAnimSkill(value.monsterSkill1) || IsBodyAnimSkill(value.monsterSkill2) || IsBodyAnimSkill(value.monsterSkill3))
			{
				hashSet.Add(allPath);
				continue;
			}
			for (int i = 0; i < extraSkillIds.Count; i++)
			{
				if (IsBodyAnimSkill(extraSkillIds[i]))
				{
					hashSet.Add(allPath);
					break;
				}
			}
		}
		return hashSet;
	}

	private static List<int> CollectExtraSkillIds()
	{
		List<int> list = new List<int>();
		BattleInfo battleInfo = ((zbCombatSystem.Instance != null) ? zbCombatSystem.Instance.battleInfo : null);
		if (battleInfo == null || battleInfo.MonsterInfo == null)
		{
			return list;
		}
		if (battleInfo.MonsterInfo.ExtraInfo != null && battleInfo.MonsterInfo.ExtraInfo.LegendaryEffect > 0)
		{
			zbLegendaryEffectCsvRecord recordByKey = zbDataConfig.Instance.legendaryEffectCsvTable.GetRecordByKey(battleInfo.MonsterInfo.ExtraInfo.LegendaryEffect);
			if (recordByKey != null && recordByKey.mstSkillId > 0)
			{
				list.Add(recordByKey.mstSkillId);
			}
		}
		if (zbCombatSystem.Instance.isPVPBattleType)
		{
			CollectFootholdSkill(zbCombatSystem.Instance.combatData, list);
			CollectFootholdSkill(zbCombatSystem.Instance.combatData2, list);
		}
		return list;
	}

	private static Dictionary<string, zbMonsterCsvRecord> BuildMonsterRecordByBone()
	{
		Dictionary<string, zbMonsterCsvRecord> dictionary = new Dictionary<string, zbMonsterCsvRecord>();
		zbDataConfig instance = zbDataConfig.Instance;
		if (instance == null || instance.monsterCsvTable == null || instance.monsterAvatarCsvTable == null)
		{
			return dictionary;
		}
		zbMonsterCsvTableData records = instance.monsterCsvTable.GetRecords();
		zbMonsterAvatarCsvTableData records2 = instance.monsterAvatarCsvTable.GetRecords();
		if (records == null || records.records == null || records2 == null || records2.records == null)
		{
			return dictionary;
		}
		foreach (KeyValuePair<int, zbMonsterCsvRecord> record in records.records)
		{
			zbMonsterCsvRecord value = record.Value;
			if (value != null && records2.records.TryGetValue(value.avatarId, out var value2) && !string.IsNullOrEmpty(value2.fightBone))
			{
				dictionary[value2.fightBone + "_SkeletonData.asset"] = value;
			}
		}
		return dictionary;
	}

	public static void AppendSummonDerivedPaths(List<string> allPaths, HashSet<int> waveTids)
	{
		s_derivedPaths = null;
		if (!kEnabled || allPaths == null || allPaths.Count == 0)
		{
			return;
		}
		zbDataConfig config = zbDataConfig.Instance;
		if (config == null || config.monsterCsvTable == null || config.monsterAvatarCsvTable == null || config.monsterSkillCsvTable == null)
		{
			return;
		}
		zbMonsterCsvTableData records = config.monsterCsvTable.GetRecords();
		if (records == null || records.records == null)
		{
			return;
		}
		List<int> list = CollectExtraSkillIds();
		Dictionary<int, string> boneByTid = new Dictionary<int, string>();
		Dictionary<int, zbMonsterCsvRecord> recordByTid = new Dictionary<int, zbMonsterCsvRecord>();
		zbMonsterAvatarCsvTableData records2 = config.monsterAvatarCsvTable.GetRecords();
		if (records2 == null || records2.records == null)
		{
			return;
		}
		foreach (KeyValuePair<int, zbMonsterCsvRecord> record in records.records)
		{
			zbMonsterCsvRecord value = record.Value;
			if (value != null)
			{
				recordByTid[value.id] = value;
				if (records2.records.TryGetValue(value.avatarId, out var value2) && !string.IsNullOrEmpty(value2.fightBone))
				{
					boneByTid[value.id] = value2.fightBone + "_SkeletonData.asset";
				}
			}
		}
		HashSet<string> pathSet = new HashSet<string>(allPaths);
		HashSet<int> visited = new HashSet<int>();
		Queue<int> queue = new Queue<int>();
		foreach (KeyValuePair<int, string> item in boneByTid)
		{
			if (pathSet.Contains(item.Value) && visited.Add(item.Key))
			{
				queue.Enqueue(item.Key);
			}
		}
		if (waveTids != null)
		{
			foreach (int waveTid in waveTids)
			{
				if (visited.Add(waveTid))
				{
					queue.Enqueue(waveTid);
				}
			}
		}
		List<string> derived = new List<string>();
		while (queue.Count > 0)
		{
			int key = queue.Dequeue();
			if (recordByTid.TryGetValue(key, out var value3))
			{
				EnqueueSummonTargets(value3.monsterSkill1);
				EnqueueSummonTargets(value3.monsterSkill2);
				EnqueueSummonTargets(value3.monsterSkill3);
				for (int i = 0; i < list.Count; i++)
				{
					EnqueueSummonTargets(list[i]);
				}
			}
		}
		if (derived.Count > 0)
		{
			allPaths.Sort();
			s_derivedPaths = new HashSet<string>(derived);
			zbPerfLog.Log($"[zbSpineGpuAnim] 衍生/召唤怪型并入烘焙口径 {derived.Count} 型：" + string.Join("、", derived.ToArray()));
		}
		void EnqueueSummonTargets(int skillId)
		{
			if (skillId > 0)
			{
				zbMonsterSkillCsvRecord recordByKey = config.monsterSkillCsvTable.GetRecordByKey(skillId);
				if (recordByKey != null && recordByKey.param != null)
				{
					for (int j = 0; j < recordByKey.param.Length; j++)
					{
						int num = (int)recordByKey.param[j];
						if (num >= 1000 && visited.Add(num) && recordByTid.TryGetValue(num, out var value4) && value4.type == 1 && boneByTid.TryGetValue(num, out var value5))
						{
							if (!pathSet.Contains(value5))
							{
								pathSet.Add(value5);
								allPaths.Add(value5);
								derived.Add(value5);
							}
							queue.Enqueue(num);
						}
					}
				}
			}
		}
	}

	private static Dictionary<string, int> CountBornInstances(List<string> sortedPaths)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		BattleInfo battleInfo = ((zbCombatSystem.Instance != null) ? zbCombatSystem.Instance.battleInfo : null);
		if (battleInfo == null || battleInfo.MonsterInfo == null)
		{
			return dictionary;
		}
		foreach (BattleMonsterRoundDetail round in battleInfo.MonsterInfo.RoundList)
		{
			if (round == null)
			{
				continue;
			}
			foreach (BattleBornMonsterDetail born in round.BornList)
			{
				if (born == null)
				{
					continue;
				}
				zbMonsterCsvRecord recordByKey = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(born.MonsterId);
				if (recordByKey == null || recordByKey.type != 1)
				{
					continue;
				}
				zbMonsterAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.monsterAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
				if (recordByKey2 != null && !string.IsNullOrEmpty(recordByKey2.fightBone))
				{
					string text = recordByKey2.fightBone + "_SkeletonData.asset";
					if (sortedPaths.Contains(text))
					{
						dictionary.TryGetValue(text, out var value);
						dictionary[text] = value + born.Num;
					}
				}
			}
		}
		return dictionary;
	}

	private static void CollectFootholdSkill(zbCombatData combatData, List<int> skillIds)
	{
		if (combatData == null || combatData.battleSideTeamInfo == null)
		{
			return;
		}
		zbCombatSystem instance = zbCombatSystem.Instance;
		zbCombatData zbCombatData2 = ((instance.combatData == combatData) ? instance.combatData2 : instance.combatData);
		if (zbCombatData2 == null || zbCombatData2.battleSideTeamInfo == null)
		{
			return;
		}
		int footholdId = zbCombatData2.battleSideTeamInfo.MasterTeamInfo.FootholdInfo.FootholdId;
		zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(footholdId);
		if (recordByKey != null && recordByKey.effectId > 0)
		{
			zbLegendaryEffectCsvRecord recordByKey2 = zbDataConfig.Instance.legendaryEffectCsvTable.GetRecordByKey(recordByKey.effectId);
			if (recordByKey2 != null && recordByKey2.mstSkillId > 0)
			{
				skillIds.Add(recordByKey2.mstSkillId);
			}
		}
	}

	private static bool IsBodyAnimSkill(int skillId)
	{
		if (skillId > 0)
		{
			return kBodyAnimSkillIds.Contains(skillId);
		}
		return false;
	}

	public static bool TryConvertMonster(zbMonster monster)
	{
		if (!kEnabled || (Object)(object)monster == (Object)null || monster.combatData == null || s_activeBaked == null)
		{
			return false;
		}
		zbCombatSystem instance = zbCombatSystem.Instance;
		if (!kPvpEnabled && instance.isPVPBattleType)
		{
			Inc("GPU排除-PVP闸");
			return false;
		}
		if (monster.isEliteBoss)
		{
			Inc("GPU排除-精英boss");
			return false;
		}
		if (instance.bMonsterNeedWaterEffect)
		{
			Inc("GPU排除-水场景");
			return false;
		}
		if (monster.isUsingSprite)
		{
			return false;
		}
		SkeletonRenderer skeletonRenderer = monster.m_skeletonRenderer;
		SkeletonAnimation val = (SkeletonAnimation)(object)((skeletonRenderer is SkeletonAnimation) ? skeletonRenderer : null);
		if (val == null || ((SkeletonRenderer)val).skeleton == null)
		{
			Inc("GPU排除-骨架无效");
			return false;
		}
		if ((Object)(object)((Component)monster.m_skeletonRenderer).GetComponent<SkeletonRenderSeparator>() != (Object)null)
		{
			Inc("GPU排除-separator");
			return false;
		}
		SkeletonDataAsset skeletonDataAsset = ((SkeletonRenderer)val).skeletonDataAsset;
		string key = monster.monsterAvatarRecord.fightBone + "_SkeletonData.asset";
		if (!s_activeBaked.TryGetValue(key, out var value) || (Object)(object)value.animTexture == (Object)null)
		{
			Inc("GPU排除-未烘焙型");
			return false;
		}
		if (!zbCombatAtlasMerger.Instance.IsMergedAsset(skeletonDataAsset))
		{
			Inc("GPU排除-非合并资产");
			return false;
		}
		if (value.hasAdditiveSubmesh && (s_activeMega == null || (Object)(object)s_activeMega.materialAdditive == (Object)null))
		{
			Inc("GPU排除-多材质additive");
			return false;
		}
		if (s_activeMega == null || !s_activeMega.typeByPath.TryGetValue(key, out var value2))
		{
			Inc("GPU排除-未烘焙型");
			return false;
		}
		zbSpineGpuAnimRenderer zbSpineGpuAnimRenderer2 = monster.combatData.EnsureGpuAnimRenderer();
		if (zbSpineGpuAnimRenderer2 == null)
		{
			return false;
		}
		zbSpineAnimation spineAnimation = monster.anim.spineAnimation;
		if (spineAnimation.gpuPlayer == null)
		{
			spineAnimation.gpuPlayer = new zbSpineGpuAnimPlayer();
		}
		spineAnimation.gpuPlayer.Bind(value, monster, zbSpineGpuAnimRenderer2, value2);
		if (monster.anim.spineColorChange != null)
		{
			monster.anim.spineColorChange.gpuTintSink = spineAnimation.gpuPlayer;
		}
		((SkeletonRenderer)val).UpdateMode = (UpdateMode)0;
		((Behaviour)val).enabled = false;
		MeshRenderer component = ((Component)val).GetComponent<MeshRenderer>();
		if ((Object)(object)component != (Object)null)
		{
			((Renderer)component).enabled = false;
		}
		monster.m_spineGpuTakenOver = true;
		Inc("GPU动画转换怪数");
		return true;
	}

	public static void ReleaseMonster(zbMonster monster)
	{
		zbSpineAnimation zbSpineAnimation2 = (((Object)(object)monster != (Object)null && monster.anim != null) ? monster.anim.spineAnimation : null);
		if (zbSpineAnimation2 != null && zbSpineAnimation2.gpuPlayer != null)
		{
			zbSpineAnimation2.gpuPlayer.Unbind();
			zbSpineAnimation2.gpuPlayer.Reset();
		}
		if ((Object)(object)monster != (Object)null && monster.anim != null && monster.anim.spineColorChange != null)
		{
			monster.anim.spineColorChange.gpuTintSink = null;
		}
	}

	public static void RestoreDormantMonster(zbMonster monster)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		SkeletonAnimation val = (((Object)(object)monster != (Object)null && monster.anim != null) ? monster.anim.spineAnimation : null)?.skeletonAnimation;
		if (!((Object)(object)val == (Object)null) && !((Behaviour)val).enabled && (int)((SkeletonRenderer)val).UpdateMode == 0)
		{
			((Behaviour)val).enabled = true;
			((SkeletonRenderer)val).UpdateMode = (UpdateMode)3;
			MeshRenderer component = ((Component)val).GetComponent<MeshRenderer>();
			if ((Object)(object)component != (Object)null && !((Renderer)component).enabled)
			{
				((Renderer)component).enabled = true;
			}
		}
	}

	public static void NotifyRuntimeFallback()
	{
		Inc("GPU动画运行时回退数");
	}

	public static bool TryConvertBullet(zbMonsterBullet bullet)
	{
		if (!kEnabled || (Object)(object)bullet == (Object)null || bullet.combatData == null || s_activeBaked == null || s_activeMega == null)
		{
			return false;
		}
		zbCombatSystem instance = zbCombatSystem.Instance;
		if (!kPvpEnabled && instance.isPVPBattleType)
		{
			Inc("GPU弹幕排除-PVP闸");
			return false;
		}
		if (instance.bMonsterNeedWaterEffect)
		{
			Inc("GPU弹幕排除-水场景");
			return false;
		}
		SkeletonAnimation animationSpine = bullet.animationSpine;
		if (!((Object)(object)animationSpine != (Object)null) || ((SkeletonRenderer)animationSpine).skeleton == null)
		{
			Inc("GPU弹幕排除-骨架无效");
			return false;
		}
		if ((Object)(object)((Component)animationSpine).GetComponent<SkeletonRenderSeparator>() != (Object)null)
		{
			Inc("GPU弹幕排除-separator");
			return false;
		}
		if ((Object)(object)bullet.monster == (Object)null || bullet.monster.monsterAvatarRecord == null)
		{
			Inc("GPU弹幕排除-宿主无效");
			return false;
		}
		string key = bullet.monster.monsterAvatarRecord.fightBone + "_SkeletonData.asset";
		if (!s_activeBaked.TryGetValue(key, out var value) || (Object)(object)value.animTexture == (Object)null)
		{
			Inc("GPU弹幕排除-未烘焙宿主型");
			return false;
		}
		if (!zbCombatAtlasMerger.Instance.IsMergedAsset(((SkeletonRenderer)animationSpine).skeletonDataAsset))
		{
			Inc("GPU弹幕排除-非合并资产");
			return false;
		}
		if (value.hasAdditiveSubmesh && (Object)(object)s_activeMega.materialAdditive == (Object)null)
		{
			Inc("GPU弹幕排除-多材质additive");
			return false;
		}
		if (!s_activeMega.typeByPath.TryGetValue(key, out var value2))
		{
			Inc("GPU弹幕排除-未烘焙宿主型");
			return false;
		}
		zbSpineGpuAnimRenderer zbSpineGpuAnimRenderer2 = bullet.combatData.EnsureGpuAnimRenderer();
		if (zbSpineGpuAnimRenderer2 == null)
		{
			return false;
		}
		if (bullet.gpuPlayer == null)
		{
			bullet.gpuPlayer = new zbSpineGpuAnimPlayer();
		}
		bullet.gpuPlayer.Bind(value, ((Component)animationSpine).transform, zbSpineGpuAnimRenderer2, value2);
		((SkeletonRenderer)animationSpine).UpdateMode = (UpdateMode)0;
		((Behaviour)animationSpine).enabled = false;
		MeshRenderer component = ((Component)animationSpine).GetComponent<MeshRenderer>();
		if ((Object)(object)component != (Object)null)
		{
			((Renderer)component).enabled = false;
		}
		bullet.m_spineGpuTakenOver = true;
		Inc("GPU弹幕转换数");
		return true;
	}

	public static void ReleaseBullet(zbMonsterBullet bullet)
	{
		if ((Object)(object)bullet != (Object)null && bullet.gpuPlayer != null)
		{
			bullet.gpuPlayer.Unbind();
			bullet.gpuPlayer.Reset();
		}
	}

	public static void RestoreDormantBullet(zbMonsterBullet bullet)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)bullet == (Object)null)
		{
			return;
		}
		bullet.m_spineGpuTakenOver = false;
		SkeletonAnimation animationSpine = bullet.animationSpine;
		if (!((Object)(object)animationSpine == (Object)null) && !((Behaviour)animationSpine).enabled && (int)((SkeletonRenderer)animationSpine).UpdateMode == 0)
		{
			((Behaviour)animationSpine).enabled = true;
			((SkeletonRenderer)animationSpine).UpdateMode = (UpdateMode)3;
			MeshRenderer component = ((Component)animationSpine).GetComponent<MeshRenderer>();
			if ((Object)(object)component != (Object)null && !((Renderer)component).enabled)
			{
				((Renderer)component).enabled = true;
			}
		}
	}

	public static bool TryConvertEffect(zbSpineGpuAnimPlayer player, SkeletonAnimation skeletonAnimation, string spinePath, zbCombatData combatData)
	{
		if (!kEnabled || player == null || (Object)(object)skeletonAnimation == (Object)null || combatData == null || s_activeEffectBaked == null || string.IsNullOrEmpty(spinePath))
		{
			return false;
		}
		zbCombatSystem instance = zbCombatSystem.Instance;
		if (!kPvpEnabled && instance != null && instance.isPVPBattleType)
		{
			Inc("GPU特效排除-PVP闸");
			return false;
		}
		if (((SkeletonRenderer)skeletonAnimation).skeleton == null)
		{
			Inc("GPU特效排除-骨架无效");
			return false;
		}
		if ((Object)(object)((Component)skeletonAnimation).GetComponent<SkeletonRenderSeparator>() != (Object)null)
		{
			Inc("GPU特效排除-separator");
			return false;
		}
		if (!s_activeEffectBaked.TryGetValue(spinePath, out var value) || (Object)(object)value.animTexture == (Object)null)
		{
			Inc("GPU特效排除-未烘焙型");
			return false;
		}
		if (!zbCombatAtlasMerger.Instance.IsEffectMergedAsset(((SkeletonRenderer)skeletonAnimation).skeletonDataAsset))
		{
			Inc("GPU特效排除-非合并资产");
			return false;
		}
		if (value.hasAdditiveSubmesh && (s_activeEffectMega == null || (Object)(object)s_activeEffectMega.materialAdditive == (Object)null))
		{
			Inc("GPU特效排除-多材质additive");
			return false;
		}
		if (s_activeEffectMega == null || !s_activeEffectMega.typeByPath.TryGetValue(spinePath, out var value2))
		{
			Inc("GPU特效排除-未烘焙型");
			return false;
		}
		zbSpineGpuAnimRenderer zbSpineGpuAnimRenderer2 = combatData.EnsureGpuAnimRenderer();
		if (zbSpineGpuAnimRenderer2 == null)
		{
			return false;
		}
		player.Bind(value, ((Component)skeletonAnimation).transform, zbSpineGpuAnimRenderer2, value2);
		((SkeletonRenderer)skeletonAnimation).UpdateMode = (UpdateMode)0;
		((Behaviour)skeletonAnimation).enabled = false;
		MeshRenderer component = ((Component)skeletonAnimation).GetComponent<MeshRenderer>();
		if ((Object)(object)component != (Object)null)
		{
			((Renderer)component).enabled = false;
		}
		Inc("GPU特效转换数");
		return true;
	}

	public static void ReleaseEffect(zbSpineGpuAnimPlayer player)
	{
		if (player != null)
		{
			player.Unbind();
			player.Reset();
		}
	}

	public static void RestoreDormantEffect(SkeletonAnimation skeletonAnimation)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)skeletonAnimation == (Object)null))
		{
			if (!((Behaviour)skeletonAnimation).enabled)
			{
				((Behaviour)skeletonAnimation).enabled = true;
			}
			if ((int)((SkeletonRenderer)skeletonAnimation).UpdateMode == 0)
			{
				((SkeletonRenderer)skeletonAnimation).UpdateMode = (UpdateMode)3;
			}
			MeshRenderer component = ((Component)skeletonAnimation).GetComponent<MeshRenderer>();
			if ((Object)(object)component != (Object)null && !((Renderer)component).enabled)
			{
				((Renderer)component).enabled = true;
			}
		}
	}

	public static void RegisterPendingReconvert(zbMonster monster)
	{
		if (kEnabled && !((Object)(object)monster == (Object)null) && !s_pendingReconvert.Contains(monster))
		{
			s_pendingReconvert.Add(monster);
			s_pendingReconvertAt.Add(Time.unscaledTime + 3f);
		}
	}

	public static void TickPendingReconvert()
	{
		if (!kEnabled || s_pendingReconvert.Count == 0 || s_reconvertTickFrame == Time.frameCount)
		{
			return;
		}
		s_reconvertTickFrame = Time.frameCount;
		float unscaledTime = Time.unscaledTime;
		for (int num = s_pendingReconvert.Count - 1; num >= 0; num--)
		{
			zbMonster zbMonster2 = s_pendingReconvert[num];
			if ((Object)(object)zbMonster2 == (Object)null || zbMonster2.m_spineGpuTakenOver)
			{
				s_pendingReconvert.RemoveAt(num);
				s_pendingReconvertAt.RemoveAt(num);
			}
			else if (!(unscaledTime < s_pendingReconvertAt[num]))
			{
				zbSpineAnimation zbSpineAnimation2 = ((zbMonster2.anim != null) ? zbMonster2.anim.spineAnimation : null);
				SkeletonAnimation val = zbSpineAnimation2?.skeletonAnimation;
				if ((Object)(object)val == (Object)null || !((Component)val).gameObject.activeInHierarchy)
				{
					s_pendingReconvert.RemoveAt(num);
					s_pendingReconvertAt.RemoveAt(num);
				}
				else if (!HasActiveTrackAboveZero(val))
				{
					s_pendingReconvert.RemoveAt(num);
					s_pendingReconvertAt.RemoveAt(num);
					if (TryConvertMonster(zbMonster2))
					{
						Inc("GPU动画重转换次数");
						zbSpineGpuAnimPlayer gpuPlayer = zbSpineAnimation2.gpuPlayer;
						gpuPlayer.Reset();
						gpuPlayer.timeScale = val.timeScale;
						gpuPlayer.SetSkeletonAlpha(((SkeletonRenderer)val).skeleton.A);
						TrackEntry current = val.AnimationState.GetCurrent(0);
						if (current != null && current.Animation != null)
						{
							if (current.Animation.Name == "<empty>")
							{
								gpuPlayer.PlaySetupPose();
							}
							else if (gpuPlayer.PlayClip(current.Animation.Name, current.Loop))
							{
								gpuPlayer.SetTime(current.TrackTime);
							}
						}
						gpuPlayer.Advance(0f);
						zbPerfLog.Log("[zbSpineGpuAnim] 回退怪回归 GPU：" + ((zbMonster2.monsterAvatarRecord != null) ? zbMonster2.monsterAvatarRecord.fightBone : "<null>"));
					}
				}
			}
		}
	}

	public static void ReconvertAfterSpriteRestore(zbMonster monster)
	{
		zbSpineAnimation zbSpineAnimation2 = (((Object)(object)monster != (Object)null && monster.anim != null) ? monster.anim.spineAnimation : null);
		if (zbSpineAnimation2 != null && zbSpineAnimation2.gpuPlayer != null && zbSpineAnimation2.gpuPlayer.isFallback)
		{
			RegisterPendingReconvert(monster);
		}
		else
		{
			TryConvertMonster(monster);
		}
	}

	private static bool HasActiveTrackAboveZero(SkeletonAnimation skeletonAnimation)
	{
		ExposedList<TrackEntry> val = ((skeletonAnimation.AnimationState != null) ? skeletonAnimation.AnimationState.Tracks : null);
		if (val == null)
		{
			return false;
		}
		TrackEntry[] items = val.Items;
		for (int i = 1; i < val.Count; i++)
		{
			TrackEntry val2 = items[i];
			if (val2 != null && (val2.Loop || val2.TrackTime < val2.TrackEnd))
			{
				return true;
			}
		}
		return false;
	}

	public static zbSpineGpuAnimBakedType FindBaked(string spinePath)
	{
		if (s_activeBaked == null || spinePath == null || !s_activeBaked.TryGetValue(spinePath, out var value))
		{
			return null;
		}
		return value;
	}

	public static void Clear()
	{
		s_activeBaked = null;
		s_activeMega = null;
		s_derivedPaths = null;
		s_activeEffectBaked = null;
		s_activeEffectMega = null;
		s_pendingReconvert.Clear();
		s_pendingReconvertAt.Clear();
		DestroyCache();
		DestroyEffectCache();
	}

	private static void DestroyCache()
	{
		if (s_cachedBaked != null)
		{
			foreach (KeyValuePair<string, zbSpineGpuAnimBakedType> item in s_cachedBaked)
			{
				if (item.Value != null)
				{
					item.Value.Release();
				}
			}
			s_cachedBaked = null;
			s_cachedKey = null;
		}
		if (s_cachedMega != null)
		{
			s_cachedMega.Release();
			s_cachedMega = null;
			s_cachedMegaKey = null;
		}
	}

	private static void DestroyEffectCache()
	{
		if (s_cachedEffectBaked != null)
		{
			foreach (KeyValuePair<string, zbSpineGpuAnimBakedType> item in s_cachedEffectBaked)
			{
				if (item.Value != null)
				{
					item.Value.Release();
				}
			}
			s_cachedEffectBaked = null;
			s_cachedEffectBakedKey = null;
		}
		if (s_cachedEffectMega != null)
		{
			s_cachedEffectMega.Release();
			s_cachedEffectMega = null;
			s_cachedEffectMegaKey = null;
		}
	}

	private static void Inc(string key)
	{
		zbPerfLog.Inc(key, 1L);
	}
}
