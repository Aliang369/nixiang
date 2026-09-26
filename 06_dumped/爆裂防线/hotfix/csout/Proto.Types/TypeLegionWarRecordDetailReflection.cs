using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionWarRecordDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarRecordDetailReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkxlZ2lvbldhclJlY29yZERldGFpbC5wcm90bxIFdHlwZXMaHWVu" + "dW0uRUJhdHRsZVJlc3VsdFNjb3JlLnByb3RvGhVlbnVtLkVGaWdodFR5cGUu" + "cHJvdG8aF3R5cGUuUm9sZUJhc2VJbmZvLnByb3RvIsoBChVMZWdpb25XYXJS" + "ZWNvcmREZXRhaWwSDAoEVVVJRBgBIAEoCRIpCglGaWdodFR5cGUYAiABKA4y" + "Fi5FRmlnaHRUeXBlLkVGaWdodFR5cGUSOwoLUmVzdWx0U2NvcmUYAyABKA4y" + "Ji5FQmF0dGxlUmVzdWx0U2NvcmUuRUJhdHRsZVJlc3VsdFNjb3JlEhAKCFJl" + "Y29yZFRtGAQgASgDEikKDFJvbGVCYXNlSW5mbxgFIAEoCzITLnR5cGVzLlJv" + "bGVCYXNlSW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEBattleResultScoreReflection.Descriptor,
			EnumEFightTypeReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarRecordDetail), (MessageParser)(object)LegionWarRecordDetail.Parser, new string[5] { "UUID", "FightType", "ResultScore", "RecordTm", "RoleBaseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
