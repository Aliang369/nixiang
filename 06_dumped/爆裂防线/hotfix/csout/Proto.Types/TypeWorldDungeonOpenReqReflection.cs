using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeWorldDungeonOpenReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldDungeonOpenReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLldvcmxkRHVuZ2Vvbk9wZW5SZXEucHJvdG8SBXR5cGVzGhtlbnVt" + "LkVEaWZmaWN1bHR5TGV2ZWwucHJvdG8icwoTV29ybGREdW5nZW9uT3BlblJl" + "cRIOCgZHcmlkSWQYASABKAUSOAoMRHVuZ2VvbkxldmVsGAIgASgOMiIuRURp" + "ZmZpY3VsdHlMZXZlbC5FRGlmZmljdWx0eUxldmVsEhIKClNlbGVjdExpc3QY" + "AyADKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJv" + "dG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEDifficultyLevelReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldDungeonOpenReq), (MessageParser)(object)WorldDungeonOpenReq.Parser, new string[3] { "GridId", "DungeonLevel", "SelectList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
