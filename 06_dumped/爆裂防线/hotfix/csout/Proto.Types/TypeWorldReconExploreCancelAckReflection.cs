using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldReconExploreCancelAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldReconExploreCancelAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLldvcmxkUmVjb25FeHBsb3JlQ2FuY2VsQWNrLnByb3RvEgV0eXBl" + "cxoUdHlwZS5Xb3JsZEluZm8ucHJvdG8iQQoaV29ybGRSZWNvbkV4cGxvcmVD" + "YW5jZWxBY2sSIwoJV29ybGRJbmZvGAEgASgLMhAudHlwZXMuV29ybGRJbmZv" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeWorldInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldReconExploreCancelAck), (MessageParser)(object)WorldReconExploreCancelAck.Parser, new string[1] { "WorldInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
