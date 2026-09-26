using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldBossFightRebornReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldBossFightRebornReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLldvcmxkQm9zc0ZpZ2h0UmVib3JuUmVxLnByb3RvEgV0eXBlcyI7" + "ChdXb3JsZEJvc3NGaWdodFJlYm9yblJlcRIMCgRVVUlEGAEgASgFEhIKCkJh" + "dHRsZVVVSUQYAiABKAlCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldBossFightRebornReq), (MessageParser)(object)WorldBossFightRebornReq.Parser, new string[2] { "UUID", "BattleUUID" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
