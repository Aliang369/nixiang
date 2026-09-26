using System;
using System.Reflection;
using System.Text;
using Spine;
using Spine.Unity;
using UnityEngine;

public static class zbSpineGpuAnimInvisibleDiag
{
	public static string DumpMonster(zbMonster monster)
	{
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06da: Unknown result type (might be due to invalid IL or missing references)
		//IL_06de: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_070f: Unknown result type (might be due to invalid IL or missing references)
		//IL_072a: Unknown result type (might be due to invalid IL or missing references)
		//IL_073b: Unknown result type (might be due to invalid IL or missing references)
		//IL_074c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0765: Unknown result type (might be due to invalid IL or missing references)
		//IL_0771: Unknown result type (might be due to invalid IL or missing references)
		//IL_077d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0817: Unknown result type (might be due to invalid IL or missing references)
		//IL_081c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0821: Unknown result type (might be due to invalid IL or missing references)
		//IL_0847: Unknown result type (might be due to invalid IL or missing references)
		//IL_0853: Unknown result type (might be due to invalid IL or missing references)
		//IL_0869: Unknown result type (might be due to invalid IL or missing references)
		//IL_0875: Unknown result type (might be due to invalid IL or missing references)
		//IL_0886: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_060e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0613: Unknown result type (might be due to invalid IL or missing references)
		//IL_0617: Unknown result type (might be due to invalid IL or missing references)
		//IL_062b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0630: Unknown result type (might be due to invalid IL or missing references)
		//IL_0634: Unknown result type (might be due to invalid IL or missing references)
		//IL_0648: Unknown result type (might be due to invalid IL or missing references)
		//IL_064d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0651: Unknown result type (might be due to invalid IL or missing references)
		//IL_0665: Unknown result type (might be due to invalid IL or missing references)
		//IL_066a: Unknown result type (might be due to invalid IL or missing references)
		//IL_066e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)monster == (Object)null)
		{
			return "[zbSpineGpuAnimInvisibleDiag] monster 为 null";
		}
		StringBuilder stringBuilder = new StringBuilder(2048);
		stringBuilder.AppendLine($"======== zbSpineGpuAnimInvisibleDiag（{DateTime.Now:HH:mm:ss.fff}）========");
		SkeletonRenderer skeletonRenderer = monster.m_skeletonRenderer;
		SkeletonAnimation val = (SkeletonAnimation)(object)((skeletonRenderer is SkeletonAnimation) ? skeletonRenderer : null);
		stringBuilder.AppendLine("[基础] GO=" + (((Object)(object)((Component)monster).gameObject != (Object)null) ? ((Object)((Component)monster).gameObject).name : "<销毁>") + " " + $"activeInHierarchy={(Object)(object)((Component)monster).gameObject != (Object)null && ((Component)monster).gameObject.activeInHierarchy} " + $"isGoDestroyed={monster.isGoDestroyed} isUsingSprite(换图)={monster.isUsingSprite} " + $"tier={monster.GetCurrentSpineTier()} cachPositionY={monster.cachPositionY}");
		stringBuilder.AppendLine($"[GPU] m_spineGpuTakenOver={monster.m_spineGpuTakenOver}");
		zbSpineGpuAnimPlayer zbSpineGpuAnimPlayer2 = ((monster.anim != null) ? monster.anim.spineAnimation : null)?.gpuPlayer;
		if (zbSpineGpuAnimPlayer2 != null)
		{
			stringBuilder.AppendLine($"[GPU] isFallback={zbSpineGpuAnimPlayer2.isFallback} hasPlayed={zbSpineGpuAnimPlayer2.hasPlayed} " + string.Format("clip={0} duration={1:0.000} ", zbSpineGpuAnimPlayer2.GetCurrentClipName() ?? "<null>", zbSpineGpuAnimPlayer2.GetCurrentClipDuration()) + $"timeScale={zbSpineGpuAnimPlayer2.timeScale:0.00}");
			stringBuilder.AppendLine($"[GPU] 帧参数 A0={zbSpineGpuAnimPlayer2.frameA0:0.0} A1={zbSpineGpuAnimPlayer2.frameA1:0.0} blendA={zbSpineGpuAnimPlayer2.blendA:0.000} " + $"B0={zbSpineGpuAnimPlayer2.frameB0:0.0} B1={zbSpineGpuAnimPlayer2.frameB1:0.0} blendB={zbSpineGpuAnimPlayer2.blendB:0.000} " + $"cross={zbSpineGpuAnimPlayer2.crossWeight:0.000}");
			Vector4 tintVector = zbSpineGpuAnimPlayer2.tintVector;
			stringBuilder.AppendLine(string.Format("[GPU] tintVector=({0:0.000},{1:0.000},{2:0.000},{3:0.000})", new object[4] { tintVector.x, tintVector.y, tintVector.z, tintVector.w }) + ((tintVector.w < 0.01f) ? "  ★ tint alpha=0 —— GPU 实例会画成全透明！" : ""));
			stringBuilder.AppendLine("[GPU] attachedType=" + ((zbSpineGpuAnimPlayer2.attachedType != null) ? $"已挂(typeIndex={zbSpineGpuAnimPlayer2.attachedType.typeIndex})" : "<null>"));
		}
		else
		{
			stringBuilder.AppendLine("[GPU] gpuPlayer=<null>（未接管或已彻底解绑）");
		}
		if ((Object)(object)val != (Object)null)
		{
			stringBuilder.AppendLine($"[spine] enabled={((Behaviour)val).enabled} " + $"UpdateMode={((SkeletonRenderer)val).UpdateMode} UpdateTiming={val.UpdateTiming} " + $"updateWhenInvisible={((SkeletonRenderer)val).updateWhenInvisible} " + $"singleSubmesh={((SkeletonRenderer)val).singleSubmesh} " + $"activeInHierarchy={((Component)val).gameObject.activeInHierarchy}");
			Skeleton skeleton = ((SkeletonRenderer)val).skeleton;
			if (skeleton != null)
			{
				Color color = SkeletonExtensions.GetColor(skeleton);
				stringBuilder.AppendLine($"[spine] skeleton色=({color.r:0.000},{color.g:0.000}," + $"{color.b:0.000},{color.a:0.000})" + ((color.a < 0.01f) ? "  ★ skeleton alpha=0 —— 原渲染路径画了也全透明！" : ""));
				int num = 0;
				StringBuilder stringBuilder2 = new StringBuilder();
				for (int i = 0; i < skeleton.Slots.Count; i++)
				{
					Slot val2 = skeleton.Slots.Items[i];
					if (val2 != null && val2.A < 0.01f)
					{
						num++;
						if (stringBuilder2.Length > 0)
						{
							stringBuilder2.Append(",");
						}
						if (num <= 6)
						{
							stringBuilder2.Append(val2.Data.Name);
						}
					}
				}
				if (num > 0)
				{
					stringBuilder.AppendLine($"[spine] ★ alpha<0.01 的槽位 {num}/{skeleton.Slots.Count} 个：{stringBuilder2}");
				}
				TrackEntry val3 = ((val.AnimationState != null) ? val.AnimationState.GetCurrent(0) : null);
				if (val3 != null)
				{
					Animation animation = val3.Animation;
					stringBuilder.AppendLine($"[spine] track0={((animation != null) ? animation.Name : null)} TrackTime={val3.TrackTime:0.000} " + $"timeScale={val3.TimeScale:0.00} alpha={val3.Alpha:0.000}");
				}
			}
			else
			{
				stringBuilder.AppendLine("[spine] skeleton=<null>（RefreshSpine 未完成？）");
			}
			MeshRenderer component = ((Component)val).GetComponent<MeshRenderer>();
			MeshFilter component2 = ((Component)val).GetComponent<MeshFilter>();
			stringBuilder.AppendLine($"[spine] MeshRenderer.enabled={(Object)(object)component != (Object)null && ((Renderer)component).enabled} " + "材质=" + (((Object)(object)component != (Object)null && (Object)(object)((Renderer)component).sharedMaterial != (Object)null) ? ((Object)((Renderer)component).sharedMaterial).name : "<null>"));
			Mesh val4 = (((Object)(object)component2 != (Object)null) ? component2.sharedMesh : null);
			if ((Object)(object)val4 != (Object)null)
			{
				string text = $"[spine] mesh顶点={val4.vertexCount} 子网格={val4.subMeshCount} ";
				object[] array = new object[4];
				Bounds bounds = val4.bounds;
				array[0] = ((Bounds)(ref bounds)).size.x;
				bounds = val4.bounds;
				array[1] = ((Bounds)(ref bounds)).size.y;
				bounds = val4.bounds;
				array[2] = ((Bounds)(ref bounds)).center.x;
				bounds = val4.bounds;
				array[3] = ((Bounds)(ref bounds)).center.y;
				stringBuilder.AppendLine(text + string.Format("bounds={0:0.00}x{1:0.00} 中心=({2:0.00},{3:0.00})", array) + ((val4.vertexCount == 0) ? "  ★ mesh 顶点数为 0！" : ""));
			}
			else
			{
				stringBuilder.AppendLine("[spine] mesh=<null>");
			}
		}
		else
		{
			stringBuilder.AppendLine("[spine] m_skeletonRenderer 为 null 或非 SkeletonAnimation");
		}
		if ((Object)(object)skeletonRenderer != (Object)null)
		{
			Transform transform = ((Component)skeletonRenderer).transform;
			Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
			Vector3 lossyScale = transform.lossyScale;
			stringBuilder.AppendLine($"[变换] pos=({transform.position.x:0.00},{transform.position.y:0.00},{transform.position.z:0.00}) " + $"localPos=({transform.localPosition.x:0.00},{transform.localPosition.y:0.00},{transform.localPosition.z:0.00}) " + $"lossyScale=({lossyScale.x:0.000},{lossyScale.y:0.000},{lossyScale.z:0.000})" + ((((Vector3)(ref lossyScale)).sqrMagnitude < 1E-06f) ? "  ★ 缩放为 0 —— 实例矩阵退化！" : ""));
			stringBuilder.AppendLine($"[变换] 矩阵平移=({localToWorldMatrix.m03:0.00},{localToWorldMatrix.m13:0.00},{localToWorldMatrix.m23:0.00})");
		}
		if (monster.anim != null && monster.anim.spineColorChange != null)
		{
			zbSpineColorChange spineColorChange = monster.anim.spineColorChange;
			Color val5 = ReadPrivateField<Color>(spineColorChange, "m_normalColor", Color.white);
			int num2 = ReadPrivateField(spineColorChange, "m_Lerping", 0);
			bool flag = ReadPrivateField(spineColorChange, "useSlot", fallback: false);
			stringBuilder.AppendLine($"[变色] m_normalColor=({val5.r:0.000},{val5.g:0.000}," + $"{val5.b:0.000},{val5.a:0.000})" + ((val5.a < 0.01f) ? "  ★ 基准色 alpha=0 —— 受击 lerp 结束会写回透明色！" : "") + string.Format(" lerping={0} useSlot={1} gpuTintSink={2}", num2, flag, (spineColorChange.gpuTintSink != null) ? "已接" : "<null>"));
		}
		zbCombatData combatData = monster.combatData;
		if (combatData != null)
		{
			Camera spineCullCamera = combatData.spineCullCamera;
			stringBuilder.AppendLine("[分级] 相机=" + (((Object)(object)spineCullCamera != (Object)null) ? ((Object)spineCullCamera).name : "<null>") + " " + $"active={(Object)(object)spineCullCamera != (Object)null && ((Behaviour)spineCullCamera).isActiveAndEnabled} " + $"窗口Y∈[{combatData.spineCullMinY:0.00},{combatData.spineCullMaxY:0.00}] 远怪分界Y={combatData.spineFarBandY:0.00} " + $"怪Y={monster.cachPositionY:0.00}" + ((monster.cachPositionY < combatData.spineCullMinY || monster.cachPositionY > combatData.spineCullMaxY) ? "  ★ 怪在窗口外 —— 会被判 Culled 跳过实例！" : ""));
			if (combatData.spineGpuAnimRenderer != null && combatData.spineGpuAnimRenderer.isValid)
			{
				stringBuilder.Append(combatData.spineGpuAnimRenderer.DumpUnits());
			}
		}
		return stringBuilder.ToString();
	}

	private static T ReadPrivateField<T>(object obj, string fieldName, T fallback)
	{
		try
		{
			FieldInfo field = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			return (field != null) ? ((T)field.GetValue(obj)) : fallback;
		}
		catch
		{
			return fallback;
		}
	}
}
