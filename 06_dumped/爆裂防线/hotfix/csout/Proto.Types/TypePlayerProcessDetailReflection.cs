using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePlayerProcessDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePlayerProcessDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlBsYXllclByb2Nlc3NEZXRhaWwucHJvdG8SBXR5cGVzGhd0eXBl" + "LlJvbGVCYXNlSW5mby5wcm90byJnChNQbGF5ZXJQcm9jZXNzRGV0YWlsEikK" + "DFJvbGVCYXNlSW5mbxgBIAEoCzITLnR5cGVzLlJvbGVCYXNlSW5mbxIRCglN" + "YWluTGV2ZWwYAiABKAUSEgoKRWxpdGVMZXZlbBgDIAEoBUIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRoleBaseInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PlayerProcessDetail), (MessageParser)(object)PlayerProcessDetail.Parser, new string[3] { "RoleBaseInfo", "MainLevel", "EliteLevel" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
