using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRoleTipsSuitInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRoleTipsSuitInfoReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLlJvbGVUaXBzU3VpdEluZm8ucHJvdG8SBXR5cGVzGht0eXBlLlBs" + "YXllclN1aXREZXRhaWwucHJvdG8ibQoQUm9sZVRpcHNTdWl0SW5mbxIpCghT" + "dWl0TGlzdBgBIAMoCzIXLnR5cGVzLlBsYXllclN1aXREZXRhaWwSLgoNU3Vw" + "ZXJTdWl0TGlzdBgCIAMoCzIXLnR5cGVzLlBsYXllclN1aXREZXRhaWxCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypePlayerSuitDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RoleTipsSuitInfo), (MessageParser)(object)RoleTipsSuitInfo.Parser, new string[2] { "SuitList", "SuperSuitList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
