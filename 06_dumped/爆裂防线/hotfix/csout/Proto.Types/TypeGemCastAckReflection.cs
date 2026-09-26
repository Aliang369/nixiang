using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeGemCastAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeGemCastAckReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChV0eXBlLkdlbUNhc3RBY2sucHJvdG8SBXR5cGVzGhV0eXBlLkRhdGFDaGFu" + "Z2UucHJvdG8iUwoKR2VtQ2FzdEFjaxIlCgpEYXRhQ2hhbmdlGAEgASgLMhEu" + "dHlwZXMuRGF0YUNoYW5nZRIOCgZTcmNUaWQYAiABKAUSDgoGRHN0VGlkGAMg" + "ASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeDataChangeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(GemCastAck), (MessageParser)(object)GemCastAck.Parser, new string[3] { "DataChange", "SrcTid", "DstTid" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
