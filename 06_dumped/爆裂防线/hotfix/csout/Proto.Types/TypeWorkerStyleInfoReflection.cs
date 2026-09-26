using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeWorkerStyleInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorkerStyleInfoReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLldvcmtlclN0eWxlSW5mby5wcm90bxIFdHlwZXMaEmVudW0uRUdl" + "bmRlci5wcm90byKwAQoPV29ya2VyU3R5bGVJbmZvEiAKBkdlbmRlchgBIAEo" + "DjIQLkVHZW5kZXIuRUdlbmRlchIRCglCb2R5Q29sb3IYAiADKAUSEQoJSGFp" + "ckNvbG9yGAMgAygFEhAKCEV5ZUNvbG9yGAQgAygFEgsKA0pvYhgFIAEoBRIM" + "CgREZXNjGAYgASgFEgwKBE5hbWUYByABKAkSDAoERmFjZRgIIAMoBRIMCgRT" + "dWl0GAkgAygFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1l" + "XFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEGenderReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorkerStyleInfo), (MessageParser)(object)WorkerStyleInfo.Parser, new string[9] { "Gender", "BodyColor", "HairColor", "EyeColor", "Job", "Desc", "Name", "Face", "Suit" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
