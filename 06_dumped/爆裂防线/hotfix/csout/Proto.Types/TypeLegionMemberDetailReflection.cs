using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionMemberDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionMemberDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkxlZ2lvbk1lbWJlckRldGFpbC5wcm90bxIFdHlwZXMaF3R5cGUu" + "Um9sZUJhc2VJbmZvLnByb3RvGhh0eXBlLlJvbGVMb2dpbkluZm8ucHJvdG8i" + "jAEKEkxlZ2lvbk1lbWJlckRldGFpbBIpCgxSb2xlQmFzZUluZm8YASABKAsy" + "Ey50eXBlcy5Sb2xlQmFzZUluZm8SKwoNUm9sZUxvZ2luSW5mbxgCIAEoCzIU" + "LnR5cGVzLlJvbGVMb2dpbkluZm8SDgoGQWN0aXZlGAMgASgFEg4KBkpvaW5U" + "bRgEIAEoA0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeRoleBaseInfoReflection.Descriptor,
			TypeRoleLoginInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionMemberDetail), (MessageParser)(object)LegionMemberDetail.Parser, new string[4] { "RoleBaseInfo", "RoleLoginInfo", "Active", "JoinTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
