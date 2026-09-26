using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonMarchStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonMarchStartAckReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlNlYXNvbk1hcmNoU3RhcnRBY2sucHJvdG8SBXR5cGVzGht0eXBl" + "LlNlYXNvbk1vdmVEZXRhaWwucHJvdG8iTQoTU2Vhc29uTWFyY2hTdGFydEFj" + "axIlCgRNb3ZlGAEgASgLMhcudHlwZXMuU2Vhc29uTW92ZURldGFpbBIPCgdH" + "YXNMZWZ0GAIgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJn" + "YW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSeasonMoveDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonMarchStartAck), (MessageParser)(object)SeasonMarchStartAck.Parser, new string[2] { "Move", "GasLeft" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
