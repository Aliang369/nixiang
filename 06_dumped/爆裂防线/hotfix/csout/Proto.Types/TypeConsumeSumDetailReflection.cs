using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeConsumeSumDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeConsumeSumDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkNvbnN1bWVTdW1EZXRhaWwucHJvdG8SBXR5cGVzGh50eXBlLkNv" + "bnN1bWVUYXJnZXREZXRhaWwucHJvdG8aGnR5cGUuT3BlcmF0ZUJhc2VJbmZv" + "LnByb3RvInsKEENvbnN1bWVTdW1EZXRhaWwSKAoIQmFzZUluZm8YASABKAsy" + "Fi50eXBlcy5PcGVyYXRlQmFzZUluZm8SLgoKVGFyZ2V0TGlzdBgCIAMoCzIa" + "LnR5cGVzLkNvbnN1bWVUYXJnZXREZXRhaWwSDQoFQ291bnQYAyABKAVCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeConsumeTargetDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ConsumeSumDetail), (MessageParser)(object)ConsumeSumDetail.Parser, new string[3] { "BaseInfo", "TargetList", "Count" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
