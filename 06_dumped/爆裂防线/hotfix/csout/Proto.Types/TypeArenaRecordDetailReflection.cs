using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeArenaRecordDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeArenaRecordDetailReflection()
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkFyZW5hUmVjb3JkRGV0YWlsLnByb3RvEgV0eXBlcxodZW51bS5F" + "QmF0dGxlUmVzdWx0U2NvcmUucHJvdG8aFWVudW0uRUZpZ2h0VHlwZS5wcm90" + "bxoXdHlwZS5Sb2xlQmFzZUluZm8ucHJvdG8i2gEKEUFyZW5hUmVjb3JkRGV0" + "YWlsEgwKBFVVSUQYASABKAkSKQoJRmlnaHRUeXBlGAIgASgOMhYuRUZpZ2h0" + "VHlwZS5FRmlnaHRUeXBlEjsKC1Jlc3VsdFNjb3JlGAMgASgOMiYuRUJhdHRs" + "ZVJlc3VsdFNjb3JlLkVCYXR0bGVSZXN1bHRTY29yZRISCgpDaGFuZ2VSYW5r" + "GAQgASgFEhAKCFJlY29yZFRtGAUgASgDEikKDFJvbGVCYXNlSW5mbxgGIAEo" + "CzITLnR5cGVzLlJvbGVCYXNlSW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEBattleResultScoreReflection.Descriptor,
			EnumEFightTypeReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ArenaRecordDetail), (MessageParser)(object)ArenaRecordDetail.Parser, new string[6] { "UUID", "FightType", "ResultScore", "ChangeRank", "RecordTm", "RoleBaseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
