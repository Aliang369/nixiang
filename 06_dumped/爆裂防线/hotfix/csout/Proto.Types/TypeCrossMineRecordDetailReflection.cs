using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeCrossMineRecordDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossMineRecordDetailReflection()
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkNyb3NzTWluZVJlY29yZERldGFpbC5wcm90bxIFdHlwZXMaHWVu" + "dW0uRUJhdHRsZVJlc3VsdFNjb3JlLnByb3RvGhVlbnVtLkVGaWdodFR5cGUu" + "cHJvdG8aF3R5cGUuUm9sZUJhc2VJbmZvLnByb3RvItsBChVDcm9zc01pbmVS" + "ZWNvcmREZXRhaWwSDAoEVVVJRBgBIAEoCRIpCglGaWdodFR5cGUYAiABKA4y" + "Fi5FRmlnaHRUeXBlLkVGaWdodFR5cGUSOwoLUmVzdWx0U2NvcmUYAyABKA4y" + "Ji5FQmF0dGxlUmVzdWx0U2NvcmUuRUJhdHRsZVJlc3VsdFNjb3JlEhAKCFJl" + "Y29yZFRtGAQgASgDEikKDFJvbGVCYXNlSW5mbxgFIAEoCzITLnR5cGVzLlJv" + "bGVCYXNlSW5mbxIPCgdQb2ludElkGAYgASgFQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEBattleResultScoreReflection.Descriptor,
			EnumEFightTypeReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossMineRecordDetail), (MessageParser)(object)CrossMineRecordDetail.Parser, new string[6] { "UUID", "FightType", "ResultScore", "RecordTm", "RoleBaseInfo", "PointId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
