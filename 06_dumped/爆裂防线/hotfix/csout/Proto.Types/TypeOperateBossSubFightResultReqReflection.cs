using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateBossSubFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateBossSubFightResultReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLk9wZXJhdGVCb3NzU3ViRmlnaHRSZXN1bHRSZXEucHJvdG8SBXR5" + "cGVzGht0eXBlLkJhdHRsZVJlc3VsdEluZm8ucHJvdG8ibgocT3BlcmF0ZUJv" + "c3NTdWJGaWdodFJlc3VsdFJlcRIMCgRVVUlEGAEgASgJEhEKCUJvc3NMZXZl" + "bBgCIAEoBRItCgxCYXR0bGVSZXN1bHQYAyABKAsyFy50eXBlcy5CYXR0bGVS" + "ZXN1bHRJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1l" + "XFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateBossSubFightResultReq), (MessageParser)(object)OperateBossSubFightResultReq.Parser, new string[3] { "UUID", "BossLevel", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
