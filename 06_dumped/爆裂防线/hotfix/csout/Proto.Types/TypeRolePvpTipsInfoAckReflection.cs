using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRolePvpTipsInfoAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRolePvpTipsInfoAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlJvbGVQdnBUaXBzSW5mb0Fjay5wcm90bxIFdHlwZXMaGnR5cGUu" + "Um9sZVB2cFRpcHNJbmZvLnByb3RvIkEKElJvbGVQdnBUaXBzSW5mb0FjaxIr" + "CgtQdnBUaXBzSW5mbxgBIAEoCzIWLnR5cGVzLlJvbGVQdnBUaXBzSW5mb0Iu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRolePvpTipsInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RolePvpTipsInfoAck), (MessageParser)(object)RolePvpTipsInfoAck.Parser, new string[1] { "PvpTipsInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
