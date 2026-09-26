using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateWallSubjectFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateWallSubjectFightResultReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cit0eXBlLk9wZXJhdGVXYWxsU3ViamVjdEZpZ2h0UmVzdWx0UmVxLnByb3Rv" + "EgV0eXBlcxobdHlwZS5CYXR0bGVSZXN1bHRJbmZvLnByb3RvInAKIE9wZXJh" + "dGVXYWxsU3ViamVjdEZpZ2h0UmVzdWx0UmVxEgwKBFVVSUQYASABKAkSDwoH" + "TGV2ZWxJZBgCIAEoBRItCgxCYXR0bGVSZXN1bHQYAyABKAsyFy50eXBlcy5C" + "YXR0bGVSZXN1bHRJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygIS" + "WmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateWallSubjectFightResultReq), (MessageParser)(object)OperateWallSubjectFightResultReq.Parser, new string[3] { "UUID", "LevelId", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
