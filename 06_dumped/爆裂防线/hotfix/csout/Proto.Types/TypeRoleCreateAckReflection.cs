using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRoleCreateAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRoleCreateAckReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLlJvbGVDcmVhdGVBY2sucHJvdG8SBXR5cGVzGhR0eXBlLlJvbGVT" + "dGF0ZS5wcm90byI0Cg1Sb2xlQ3JlYXRlQWNrEiMKCVJvbGVTdGF0ZRgBIAEo" + "CzIQLnR5cGVzLlJvbGVTdGF0ZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRoleStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RoleCreateAck), (MessageParser)(object)RoleCreateAck.Parser, new string[1] { "RoleState" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
