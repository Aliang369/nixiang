using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeDungeonMapGridDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDungeonMapGridDetailReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkR1bmdlb25NYXBHcmlkRGV0YWlsLnByb3RvEgV0eXBlcxoXZW51" + "bS5FQWN0aXZlU3RhdGUucHJvdG8aH2VudW0uRUR1bmdlb25FdmVudFRyaWdn" + "ZXIucHJvdG8aHGVudW0uRUR1bmdlb25FdmVudFR5cGUucHJvdG8i2AEKFER1" + "bmdlb25NYXBHcmlkRGV0YWlsEg4KBkdyaWRJZBgBIAEoBRI3CglFdmVudFR5" + "cGUYAiABKA4yJC5FRHVuZ2VvbkV2ZW50VHlwZS5FRHVuZ2VvbkV2ZW50VHlw" + "ZRIPCgdFdmVudElkGAMgASgFEjsKB1RyaWdnZXIYBCABKA4yKi5FRHVuZ2Vv" + "bkV2ZW50VHJpZ2dlci5FRHVuZ2VvbkV2ZW50VHJpZ2dlchIpCgVTdGF0ZRgF" + "IAEoDjIaLkVBY3RpdmVTdGF0ZS5FQWN0aXZlU3RhdGVCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEActiveStateReflection.Descriptor,
			EnumEDungeonEventTriggerReflection.Descriptor,
			EnumEDungeonEventTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DungeonMapGridDetail), (MessageParser)(object)DungeonMapGridDetail.Parser, new string[5] { "GridId", "EventType", "EventId", "Trigger", "State" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
