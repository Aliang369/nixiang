using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateBossSubFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateBossSubFightResultAckReflection()
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLk9wZXJhdGVCb3NzU3ViRmlnaHRSZXN1bHRBY2sucHJvdG8SBXR5" + "cGVzGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGht0eXBlLkJhdHRsZVJlc3Vs" + "dEluZm8ucHJvdG8aHHR5cGUuQm9zc1N1YmplY3REZXRhaWwucHJvdG8aFXR5" + "cGUuRGF0YUNoYW5nZS5wcm90byLdAQocT3BlcmF0ZUJvc3NTdWJGaWdodFJl" + "c3VsdEFjaxItCgxCYXR0bGVTZXR0bGUYASABKAsyFy50eXBlcy5CYXR0bGVS" + "ZXN1bHRJbmZvEjEKD0Jvc3NTdWJqZWN0TGlzdBgCIAMoCzIYLnR5cGVzLkJv" + "c3NTdWJqZWN0RGV0YWlsEiUKCUF3YXJkTGlzdBgDIAMoCzISLnR5cGVzLkF3" + "YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYBCABKAsyES50eXBlcy5EYXRhQ2hh" + "bmdlEg0KBVNjb3JlGAUgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeBossSubjectDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateBossSubFightResultAck), (MessageParser)(object)OperateBossSubFightResultAck.Parser, new string[5] { "BattleSettle", "BossSubjectList", "AwardList", "DataChange", "Score" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
