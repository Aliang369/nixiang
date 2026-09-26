using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeCrossLadderRecordDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderRecordDetailReflection()
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkNyb3NzTGFkZGVyUmVjb3JkRGV0YWlsLnByb3RvEgV0eXBlcxod" + "ZW51bS5FQmF0dGxlUmVzdWx0U2NvcmUucHJvdG8aFWVudW0uRUZpZ2h0VHlw" + "ZS5wcm90bxoXdHlwZS5Sb2xlQmFzZUluZm8ucHJvdG8i8wEKF0Nyb3NzTGFk" + "ZGVyUmVjb3JkRGV0YWlsEgwKBFVVSUQYASABKAkSKQoJRmlnaHRUeXBlGAIg" + "ASgOMhYuRUZpZ2h0VHlwZS5FRmlnaHRUeXBlEjsKC1Jlc3VsdFNjb3JlGAMg" + "ASgOMiYuRUJhdHRsZVJlc3VsdFNjb3JlLkVCYXR0bGVSZXN1bHRTY29yZRIT" + "CgtDaGFuZ2VTY29yZRgEIAEoBRIQCghSZWNvcmRUbRgFIAEoAxIpCgxSb2xl" + "QmFzZUluZm8YBiABKAsyEy50eXBlcy5Sb2xlQmFzZUluZm8SEAoITGFkZGVy" + "SWQYByABKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEBattleResultScoreReflection.Descriptor,
			EnumEFightTypeReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderRecordDetail), (MessageParser)(object)CrossLadderRecordDetail.Parser, new string[7] { "UUID", "FightType", "ResultScore", "ChangeScore", "RecordTm", "RoleBaseInfo", "LadderId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
