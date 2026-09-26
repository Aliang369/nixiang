using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeMineWarRecordDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMineWarRecordDetailReflection()
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLk1pbmVXYXJSZWNvcmREZXRhaWwucHJvdG8SBXR5cGVzGh1lbnVt" + "LkVCYXR0bGVSZXN1bHRTY29yZS5wcm90bxoVZW51bS5FRmlnaHRUeXBlLnBy" + "b3RvGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGhd0eXBlLlJvbGVCYXNlSW5m" + "by5wcm90byKCAgoTTWluZVdhclJlY29yZERldGFpbBIMCgRVVUlEGAEgASgJ" + "EikKCUZpZ2h0VHlwZRgCIAEoDjIWLkVGaWdodFR5cGUuRUZpZ2h0VHlwZRI7" + "CgtSZXN1bHRTY29yZRgDIAEoDjImLkVCYXR0bGVSZXN1bHRTY29yZS5FQmF0" + "dGxlUmVzdWx0U2NvcmUSEAoIUmVjb3JkVG0YBCABKAMSKQoMUm9sZUJhc2VJ" + "bmZvGAUgASgLMhMudHlwZXMuUm9sZUJhc2VJbmZvEg8KB1BvaW50SWQYBiAB" + "KAUSJwoLU2V0dGxlQXdhcmQYByABKAsyEi50eXBlcy5Bd2FyZERldGFpbEIu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			EnumEBattleResultScoreReflection.Descriptor,
			EnumEFightTypeReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MineWarRecordDetail), (MessageParser)(object)MineWarRecordDetail.Parser, new string[7] { "UUID", "FightType", "ResultScore", "RecordTm", "RoleBaseInfo", "PointId", "SettleAward" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
