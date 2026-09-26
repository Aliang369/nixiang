using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;

public static class zbStyleInfoHelper
{
	public static void RefreshNpcAvatar(SkeletonAnimation animation, zbNpcAvatarCsvRecord record)
	{
		RefreshSpine(animation, (EGender)record.genderId);
		RefreshStyleSkin(animation, record.faceId, record.suitId);
		zbNpcBodyColorCsvRecord recordByKey = zbDataConfig.Instance.npcBodyColorCsvTable.GetRecordByKey(record.bodyId);
		zbNpcHairColorCsvRecord recordByKey2 = zbDataConfig.Instance.npcHairColorCsvTable.GetRecordByKey(record.hairId);
		zbNpcEyeColorCsvRecord recordByKey3 = zbDataConfig.Instance.npcEyeColorCsvTable.GetRecordByKey(record.eyeColorId);
		if (recordByKey != null && recordByKey2 != null && recordByKey3 != null)
		{
			RefreshStyleColor(animation, recordByKey.color, recordByKey2.color, recordByKey3.color);
		}
	}

	public static void RefreshStyleInfo(SkeletonAnimation animation, WorkerStyleInfo styleInfo)
	{
		RefreshSpine(animation, styleInfo.Gender);
		RefreshStyleSkin(animation, (IList<int>)styleInfo.Face, (IList<int>)styleInfo.Suit);
		RefreshStyleColor(animation, (IList<int>)styleInfo.BodyColor, (IList<int>)styleInfo.HairColor, (IList<int>)styleInfo.EyeColor);
	}

	private static void RefreshSpine(SkeletonAnimation animation, EGender gender)
	{
		string spinePath = "animation/citizen/citizen_man_01/citizen_man_01_SkeletonData.asset";
		if (gender == EGender.Female)
		{
			spinePath = "animation/citizen/citizen_woman_01/citizen_woman_01_SkeletonData.asset";
		}
		zbUtil.RefreshSpine(animation, spinePath, "idle", loop: true);
	}

	private static void RefreshStyleSkin(SkeletonAnimation animation, IList<int> face, IList<int> suit)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		Skeleton skeleton = ((SkeletonRenderer)animation).Skeleton;
		SkeletonData data = skeleton.Data;
		Skin val = new Skin("skin-base");
		val.AddSkin(data.FindSkin("body"));
		foreach (int item in face)
		{
			if (item > 0)
			{
				zbNpcFaceCsvRecord recordByKey = zbDataConfig.Instance.npcFaceCsvTable.GetRecordByKey(item);
				if (recordByKey != null && data.FindSkin(recordByKey.icon) != null)
				{
					val.AddSkin(data.FindSkin(recordByKey.icon));
				}
			}
			else if (data.FindSkin("0") != null)
			{
				val.AddSkin(data.FindSkin("0"));
			}
		}
		foreach (int item2 in suit)
		{
			if (item2 > 0)
			{
				zbNpcSuitCsvRecord recordByKey2 = zbDataConfig.Instance.npcSuitCsvTable.GetRecordByKey(item2);
				if (recordByKey2 != null && data.FindSkin(recordByKey2.icon) != null)
				{
					val.AddSkin(data.FindSkin(recordByKey2.icon));
				}
			}
			else if (data.FindSkin("0") != null)
			{
				val.AddSkin(data.FindSkin("0"));
			}
		}
		skeleton.SetSkin(val);
		skeleton.SetSlotsToSetupPose();
	}

	public static void RefreshStyleColor(SkeletonAnimation animation, IList<int> body, IList<int> hair, IList<int> eye)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		Color color = default(Color);
		((Color)(ref color))._002Ector((float)body[0] / 255f, (float)body[1] / 255f, (float)body[2] / 255f);
		SetSlotColor(animation, "body_skincolor", color);
		SetSlotColor(animation, "bodycolor_skincolor", color);
		SetSlotColor(animation, "bodycolor2_skincolor", color);
		SetSlotColor(animation, "arm1_skincolor", color);
		SetSlotColor(animation, "arm11_skincolor", color);
		SetSlotColor(animation, "arm12_skincolor", color);
		SetSlotColor(animation, "arm2_skincolor", color);
		SetSlotColor(animation, "arm21_skincolor", color);
		SetSlotColor(animation, "arm22_skincolor", color);
		SetSlotColor(animation, "head_skincolor", color);
		SetSlotColor(animation, "hand1_skincolor", color);
		SetSlotColor(animation, "hand2_skincolor", color);
		SetSlotColor(animation, "leg1_skincolor", color);
		SetSlotColor(animation, "leg2_skincolor", color);
		Color color2 = default(Color);
		((Color)(ref color2))._002Ector((float)hair[0] / 255f, (float)hair[1] / 255f, (float)hair[2] / 255f);
		SetSlotColor(animation, "hair_skincolor", color2);
		SetSlotColor(animation, "hairback_skincolor", color2);
		SetSlotColor(animation, "haircolor_skincolor", color2);
		SetSlotColor(animation, "eyebrow_skincolor", color2);
		SetSlotColor(animation, "beard_skincolor", color2);
		Color color3 = default(Color);
		((Color)(ref color3))._002Ector((float)eye[0] / 255f, (float)eye[1] / 255f, (float)eye[2] / 255f);
		SetSlotColor(animation, "eyer_skincolor", color3);
		SetSlotColor(animation, "eyel_skincolor", color3);
		SetSlotColor(animation, "eyecolor_skincolor", color3);
		SetSlotColor(animation, "eyecolor2_skincolor", color3);
	}

	private static void SetSlotColor(SkeletonAnimation animation, string slotName, Color color)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Slot val = ((SkeletonRenderer)animation).skeleton.FindSlot(slotName);
		if (val != null)
		{
			SkeletonExtensions.SetColor(val, color);
		}
	}
}
