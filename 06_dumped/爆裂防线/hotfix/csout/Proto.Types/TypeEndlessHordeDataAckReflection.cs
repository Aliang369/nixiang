using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEndlessHordeDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEndlessHordeDataAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkVuZGxlc3NIb3JkZURhdGFBY2sucHJvdG8SBXR5cGVzGht0eXBl" + "LkVuZGxlc3NIb3JkZUluZm8ucHJvdG8iSAoTRW5kbGVzc0hvcmRlRGF0YUFj" + "axIxChBFbmRsZXNzSG9yZGVJbmZvGAEgASgLMhcudHlwZXMuRW5kbGVzc0hv" + "cmRlSW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeEndlessHordeInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EndlessHordeDataAck), (MessageParser)(object)EndlessHordeDataAck.Parser, new string[1] { "EndlessHordeInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
