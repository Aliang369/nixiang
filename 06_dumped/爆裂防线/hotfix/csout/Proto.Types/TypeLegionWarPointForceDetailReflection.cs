using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionWarPointForceDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarPointForceDetailReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkxlZ2lvbldhclBvaW50Rm9yY2VEZXRhaWwucHJvdG8SBXR5cGVz" + "GiFlbnVtLkVMZWdpb25XYXJDcm93ZGVkU3RhdGUucHJvdG8aGmVudW0uRUxl" + "Z2lvbldhckZvcmNlLnByb3RvIuMBChlMZWdpb25XYXJQb2ludEZvcmNlRGV0" + "YWlsEjEKB0ZvcmNlSWQYASABKA4yIC5FTGVnaW9uV2FyRm9yY2UuRUxlZ2lv" + "bldhckZvcmNlEhMKC1BsYXllckNvdW50GAIgASgFEkQKDENyb3dkZWRTdGF0" + "ZRgDIAEoDjIuLkVMZWdpb25XYXJDcm93ZGVkU3RhdGUuRUxlZ2lvbldhckNy" + "b3dkZWRTdGF0ZRIRCglPY2N1cHlQY3QYBCABKAISEgoKT2NjdXB5UmF0ZRgF" + "IAEoAhIRCglTY29yZVJhdGUYBiABKAJCLloJLi87cHJvdG9jqgILUHJvdG8u" + "VHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumELegionWarCrowdedStateReflection.Descriptor,
			EnumELegionWarForceReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarPointForceDetail), (MessageParser)(object)LegionWarPointForceDetail.Parser, new string[6] { "ForceId", "PlayerCount", "CrowdedState", "OccupyPct", "OccupyRate", "ScoreRate" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
