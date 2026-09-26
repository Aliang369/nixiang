using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeCrossPeakRecordDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossPeakRecordDetailReflection()
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkNyb3NzUGVha1JlY29yZERldGFpbC5wcm90bxIFdHlwZXMaHWVu" + "dW0uRUJhdHRsZVJlc3VsdFNjb3JlLnByb3RvGhVlbnVtLkVGaWdodFR5cGUu" + "cHJvdG8aF3R5cGUuUm9sZUJhc2VJbmZvLnByb3RvIt4BChVDcm9zc1BlYWtS" + "ZWNvcmREZXRhaWwSDAoEVVVJRBgBIAEoCRIpCglGaWdodFR5cGUYAiABKA4y" + "Fi5FRmlnaHRUeXBlLkVGaWdodFR5cGUSOwoLUmVzdWx0U2NvcmUYAyABKA4y" + "Ji5FQmF0dGxlUmVzdWx0U2NvcmUuRUJhdHRsZVJlc3VsdFNjb3JlEhIKCkNo" + "YW5nZVJhbmsYBCABKAUSEAoIUmVjb3JkVG0YBSABKAMSKQoMUm9sZUJhc2VJ" + "bmZvGAYgASgLMhMudHlwZXMuUm9sZUJhc2VJbmZvQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEBattleResultScoreReflection.Descriptor,
			EnumEFightTypeReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossPeakRecordDetail), (MessageParser)(object)CrossPeakRecordDetail.Parser, new string[6] { "UUID", "FightType", "ResultScore", "ChangeRank", "RecordTm", "RoleBaseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
