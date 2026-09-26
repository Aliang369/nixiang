using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEndlessHordeFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEndlessHordeFightResultReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLkVuZGxlc3NIb3JkZUZpZ2h0UmVzdWx0UmVxLnByb3RvEgV0eXBl" + "cxobdHlwZS5CYXR0bGVSZXN1bHRJbmZvLnByb3RvInIKGkVuZGxlc3NIb3Jk" + "ZUZpZ2h0UmVzdWx0UmVxEg8KB0xldmVsSWQYASABKAUSLQoMQmF0dGxlUmVz" + "dWx0GAIgASgLMhcudHlwZXMuQmF0dGxlUmVzdWx0SW5mbxIUCgxQYXNzRGlz" + "dGFuY2UYAyABKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdh" + "bWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EndlessHordeFightResultReq), (MessageParser)(object)EndlessHordeFightResultReq.Parser, new string[3] { "LevelId", "BattleResult", "PassDistance" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
