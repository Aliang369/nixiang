using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeWorldReconDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldReconDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLldvcmxkUmVjb25EZXRhaWwucHJvdG8SBXR5cGVzGhtlbnVtLkVX" + "b3JsZFJlY29uU3RhdGUucHJvdG8iiAEKEFdvcmxkUmVjb25EZXRhaWwSDwoH" + "UmVjb25JZBgBIAEoBRIOCgZHcmlkSWQYAiABKAUSMQoFU3RhdGUYAyABKA4y" + "Ii5FV29ybGRSZWNvblN0YXRlLkVXb3JsZFJlY29uU3RhdGUSDwoHU3RhdGVD" + "ZBgEIAEoBRIPCgdTdGF0ZVRtGAUgASgDQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEWorldReconStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldReconDetail), (MessageParser)(object)WorldReconDetail.Parser, new string[5] { "ReconId", "GridId", "State", "StateCd", "StateTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
