using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEndlessMinePropDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEndlessMinePropDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkVuZGxlc3NNaW5lUHJvcERldGFpbC5wcm90bxIFdHlwZXMiXAoV" + "RW5kbGVzc01pbmVQcm9wRGV0YWlsEg4KBlByb3BJZBgBIAEoBRINCgVDb3Vu" + "dBgCIAEoBRISCgpEYWlseUNvdW50GAMgASgFEhAKCEJ1eUNvdW50GAQgASgF" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EndlessMinePropDetail), (MessageParser)(object)EndlessMinePropDetail.Parser, new string[4] { "PropId", "Count", "DailyCount", "BuyCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
