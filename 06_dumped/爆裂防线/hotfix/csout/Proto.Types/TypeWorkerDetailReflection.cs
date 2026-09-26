using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorkerDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorkerDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLldvcmtlckRldGFpbC5wcm90bxIFdHlwZXMaGnR5cGUuV29ya2Vy" + "U3R5bGVJbmZvLnByb3RvIlsKDFdvcmtlckRldGFpbBIQCghXb3JrZXJJZBgB" + "IAEoBRIpCglTdHlsZUluZm8YAiABKAsyFi50eXBlcy5Xb3JrZXJTdHlsZUlu" + "Zm8SDgoGUm9vbUlkGAMgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeWorkerStyleInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorkerDetail), (MessageParser)(object)WorkerDetail.Parser, new string[3] { "WorkerId", "StyleInfo", "RoomId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
