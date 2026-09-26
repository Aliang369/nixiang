using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeAuxPlayerSuggestReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAuxPlayerSuggestReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkF1eFBsYXllclN1Z2dlc3RSZXEucHJvdG8SBXR5cGVzGhplbnVt" + "LkVCYXR0bGVGdW5jVHlwZS5wcm90byJcChNBdXhQbGF5ZXJTdWdnZXN0UmVx" + "EjQKCkJhdHRsZVR5cGUYASABKA4yIC5FQmF0dGxlRnVuY1R5cGUuRUJhdHRs" + "ZUZ1bmNUeXBlEg8KB0xldmVsSWQYAiABKAVCLloJLi87cHJvdG9jqgILUHJv" + "dG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBattleFuncTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AuxPlayerSuggestReq), (MessageParser)(object)AuxPlayerSuggestReq.Parser, new string[2] { "BattleType", "LevelId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
