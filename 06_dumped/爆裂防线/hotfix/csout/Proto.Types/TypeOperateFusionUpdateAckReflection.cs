using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateFusionUpdateAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateFusionUpdateAckReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLk9wZXJhdGVGdXNpb25VcGRhdGVBY2sucHJvdG8SBXR5cGVzGhV0" + "eXBlLkZ1c2lvbkluZm8ucHJvdG8iTQoWT3BlcmF0ZUZ1c2lvblVwZGF0ZUFj" + "axIlCgpGdXNpb25JbmZvGAEgASgLMhEudHlwZXMuRnVzaW9uSW5mbxIMCgRE" + "YXRhGAIgASgJQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1l" + "XFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeFusionInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateFusionUpdateAck), (MessageParser)(object)OperateFusionUpdateAck.Parser, new string[2] { "FusionInfo", "Data" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
