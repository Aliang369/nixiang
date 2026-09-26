using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeWorldEventDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldEventDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLldvcmxkRXZlbnREZXRhaWwucHJvdG8SBXR5cGVzGhplbnVtLkVX" + "b3JsZEV2ZW50VHlwZS5wcm90byJoChBXb3JsZEV2ZW50RGV0YWlsEg4KBkdy" + "aWRJZBgBIAEoBRIzCglFdmVudFR5cGUYAiABKA4yIC5FV29ybGRFdmVudFR5" + "cGUuRVdvcmxkRXZlbnRUeXBlEg8KB0V2ZW50SWQYAyABKAVCLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEWorldEventTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldEventDetail), (MessageParser)(object)WorldEventDetail.Parser, new string[3] { "GridId", "EventType", "EventId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
