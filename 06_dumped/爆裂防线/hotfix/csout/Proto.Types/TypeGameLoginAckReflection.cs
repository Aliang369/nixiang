using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeGameLoginAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeGameLoginAckReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLkdhbWVMb2dpbkFjay5wcm90bxIFdHlwZXMaEnR5cGUuR2FtZUVu" + "di5wcm90bxoUdHlwZS5Sb2xlU3RhdGUucHJvdG8iVAoMR2FtZUxvZ2luQWNr" + "Eh8KB0dhbWVFbnYYASABKAsyDi50eXBlcy5HYW1lRW52EiMKCVJvbGVTdGF0" + "ZRgCIAEoCzIQLnR5cGVzLlJvbGVTdGF0ZUIuWgkuLztwcm90b2OqAgtQcm90" + "by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeGameEnvReflection.Descriptor,
			TypeRoleStateReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(GameLoginAck), (MessageParser)(object)GameLoginAck.Parser, new string[2] { "GameEnv", "RoleState" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
