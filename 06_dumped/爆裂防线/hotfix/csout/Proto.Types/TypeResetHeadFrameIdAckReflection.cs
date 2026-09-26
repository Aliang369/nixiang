using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeResetHeadFrameIdAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeResetHeadFrameIdAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlJlc2V0SGVhZEZyYW1lSWRBY2sucHJvdG8SBXR5cGVzGhd0eXBl" + "LlJvbGVCYXNlSW5mby5wcm90byJAChNSZXNldEhlYWRGcmFtZUlkQWNrEikK" + "DFJvbGVCYXNlSW5mbxgBIAEoCzITLnR5cGVzLlJvbGVCYXNlSW5mb0IuWgku" + "Lztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRoleBaseInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ResetHeadFrameIdAck), (MessageParser)(object)ResetHeadFrameIdAck.Parser, new string[1] { "RoleBaseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
