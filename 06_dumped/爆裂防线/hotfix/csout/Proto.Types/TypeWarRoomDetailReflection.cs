using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeWarRoomDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWarRoomDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLldhclJvb21EZXRhaWwucHJvdG8SBXR5cGVzGhVlbnVtLkVTa2ls" + "bFR5cGUucHJvdG8aHHR5cGUuV2FyUm9vbUF0dHJEZXRhaWwucHJvdG8ihwEK" + "DVdhclJvb21EZXRhaWwSKQoJU2tpbGxUeXBlGAEgASgOMhYuRVNraWxsVHlw" + "ZS5FU2tpbGxUeXBlEg4KBlJvb21JZBgCIAEoBRIPCgdIZXJvVGlkGAMgASgF" + "EioKCEF0dHJMaXN0GAQgAygLMhgudHlwZXMuV2FyUm9vbUF0dHJEZXRhaWxC" + "LloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlw" + "ZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumESkillTypeReflection.Descriptor,
			TypeWarRoomAttrDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WarRoomDetail), (MessageParser)(object)WarRoomDetail.Parser, new string[4] { "SkillType", "RoomId", "HeroTid", "AttrList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
