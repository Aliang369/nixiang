using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeIncubateSlotDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeIncubateSlotDetailReflection()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkluY3ViYXRlU2xvdERldGFpbC5wcm90bxIFdHlwZXMiUwoSSW5j" + "dWJhdGVTbG90RGV0YWlsEg0KBUluZGV4GAEgASgFEg4KBlByb3BJZBgCIAEo" + "BRIPCgdTdGFydFRtGAMgASgDEg0KBUVuZFRtGAQgASgDQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(IncubateSlotDetail), (MessageParser)(object)IncubateSlotDetail.Parser, new string[4] { "Index", "PropId", "StartTm", "EndTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
