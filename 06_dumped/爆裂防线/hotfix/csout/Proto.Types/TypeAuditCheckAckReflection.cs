using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeAuditCheckAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAuditCheckAckReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLkF1ZGl0Q2hlY2tBY2sucHJvdG8SBXR5cGVzGhVlbnVtLkVCb29s" + "U3RhdGUucHJvdG8iNgoNQXVkaXRDaGVja0FjaxIlCgVTdGF0ZRgBIAEoDjIW" + "LkVCb29sU3RhdGUuRUJvb2xTdGF0ZUIuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBoolStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AuditCheckAck), (MessageParser)(object)AuditCheckAck.Parser, new string[1] { "State" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
