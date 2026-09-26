using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeDungeonTriggerEventInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDungeonTriggerEventInfoReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkR1bmdlb25UcmlnZ2VyRXZlbnRJbmZvLnByb3RvEgV0eXBlcxof" + "ZW51bS5FRHVuZ2VvbkV2ZW50VHJpZ2dlci5wcm90bxocZW51bS5FRHVuZ2Vv" + "bkV2ZW50VHlwZS5wcm90bxogdHlwZS5EdW5nZW9uRXZlbnRQb3B1cEluZm8u" + "cHJvdG8iiAIKF0R1bmdlb25UcmlnZ2VyRXZlbnRJbmZvEg4KBkNlbGxJZBgB" + "IAEoCRIOCgZHcmlkSWQYAiABKAUSNwoJRXZlbnRUeXBlGAMgASgOMiQuRUR1" + "bmdlb25FdmVudFR5cGUuRUR1bmdlb25FdmVudFR5cGUSDwoHRXZlbnRJZBgE" + "IAEoBRI7CgdUcmlnZ2VyGAUgASgOMiouRUR1bmdlb25FdmVudFRyaWdnZXIu" + "RUR1bmdlb25FdmVudFRyaWdnZXISFAoMRXZlbnRNb2RMaXN0GAYgAygFEjAK" + "CkV2ZW50UG9wdXAYByABKAsyHC50eXBlcy5EdW5nZW9uRXZlbnRQb3B1cElu" + "Zm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEDungeonEventTriggerReflection.Descriptor,
			EnumEDungeonEventTypeReflection.Descriptor,
			TypeDungeonEventPopupInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DungeonTriggerEventInfo), (MessageParser)(object)DungeonTriggerEventInfo.Parser, new string[7] { "CellId", "GridId", "EventType", "EventId", "Trigger", "EventModList", "EventPopup" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
