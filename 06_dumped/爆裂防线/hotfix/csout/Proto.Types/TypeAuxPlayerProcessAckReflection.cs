using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAuxPlayerProcessAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAuxPlayerProcessAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkF1eFBsYXllclByb2Nlc3NBY2sucHJvdG8SBXR5cGVzGh50eXBl" + "LlBsYXllclByb2Nlc3NEZXRhaWwucHJvdG8iRQoTQXV4UGxheWVyUHJvY2Vz" + "c0FjaxIuCgpQbGF5ZXJMaXN0GAEgAygLMhoudHlwZXMuUGxheWVyUHJvY2Vz" + "c0RldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypePlayerProcessDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AuxPlayerProcessAck), (MessageParser)(object)AuxPlayerProcessAck.Parser, new string[1] { "PlayerList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
