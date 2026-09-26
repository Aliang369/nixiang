using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeTruckRobRecordDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTruckRobRecordDetailReflection()
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLlRydWNrUm9iUmVjb3JkRGV0YWlsLnByb3RvEgV0eXBlcxodZW51" + "bS5FQmF0dGxlUmVzdWx0U2NvcmUucHJvdG8aFWVudW0uRUZpZ2h0VHlwZS5w" + "cm90bxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxoXdHlwZS5Sb2xlQmFzZUlu" + "Zm8ucHJvdG8i7AEKFFRydWNrUm9iUmVjb3JkRGV0YWlsEgwKBFVVSUQYASAB" + "KAkSKQoJRmlnaHRUeXBlGAIgASgOMhYuRUZpZ2h0VHlwZS5FRmlnaHRUeXBl" + "EjsKC1Jlc3VsdFNjb3JlGAMgASgOMiYuRUJhdHRsZVJlc3VsdFNjb3JlLkVC" + "YXR0bGVSZXN1bHRTY29yZRIQCghSZWNvcmRUbRgEIAEoAxIpCgxSb2xlQmFz" + "ZUluZm8YBSABKAsyEy50eXBlcy5Sb2xlQmFzZUluZm8SIQoFQXdhcmQYBiAB" + "KAsyEi50eXBlcy5Bd2FyZERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			EnumEBattleResultScoreReflection.Descriptor,
			EnumEFightTypeReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TruckRobRecordDetail), (MessageParser)(object)TruckRobRecordDetail.Parser, new string[6] { "UUID", "FightType", "ResultScore", "RecordTm", "RoleBaseInfo", "Award" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
