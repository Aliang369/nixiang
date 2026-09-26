using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeNoticeListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeNoticeListAckReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLk5vdGljZUxpc3RBY2sucHJvdG8SBXR5cGVzGhJ0eXBlLkdhbWVF" + "bnYucHJvdG8aF3R5cGUuTm90aWNlRGV0YWlsLnByb3RvIlkKDU5vdGljZUxp" + "c3RBY2sSHwoHR2FtZUVudhgBIAEoCzIOLnR5cGVzLkdhbWVFbnYSJwoKTm90" + "aWNlTGlzdBgCIAMoCzITLnR5cGVzLk5vdGljZURldGFpbEIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeGameEnvReflection.Descriptor,
			TypeNoticeDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(NoticeListAck), (MessageParser)(object)NoticeListAck.Parser, new string[2] { "GameEnv", "NoticeList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
