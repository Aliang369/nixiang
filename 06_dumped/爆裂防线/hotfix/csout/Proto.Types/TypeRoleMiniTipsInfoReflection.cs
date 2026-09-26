using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRoleMiniTipsInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRoleMiniTipsInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLlJvbGVNaW5pVGlwc0luZm8ucHJvdG8SBXR5cGVzGhl0eXBlLkxl" + "Z2lvbkZsYWdJbmZvLnByb3RvGhd0eXBlLlJvbGVCYXNlSW5mby5wcm90byJs" + "ChBSb2xlTWluaVRpcHNJbmZvEikKDFJvbGVCYXNlSW5mbxgBIAEoCzITLnR5" + "cGVzLlJvbGVCYXNlSW5mbxItCg5MZWdpb25GbGFnSW5mbxgCIAEoCzIVLnR5" + "cGVzLkxlZ2lvbkZsYWdJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeLegionFlagInfoReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RoleMiniTipsInfo), (MessageParser)(object)RoleMiniTipsInfo.Parser, new string[2] { "RoleBaseInfo", "LegionFlagInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
