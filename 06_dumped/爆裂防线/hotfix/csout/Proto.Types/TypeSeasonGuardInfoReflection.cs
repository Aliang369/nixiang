using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonGuardInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonGuardInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLlNlYXNvbkd1YXJkSW5mby5wcm90bxIFdHlwZXMicwoPU2Vhc29u" + "R3VhcmRJbmZvEg4KBkNpdHlJZBgBIAEoBRISCgpHdWFyZENvdW50GAIgASgF" + "Eg4KBkZhbGxlbhgDIAEoBRITCgtGYWxsZW5FbmRUbRgEIAEoAxIXCg9HYXJy" + "aXNvblJvbGVJZHMYBSADKANCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonGuardInfo), (MessageParser)(object)SeasonGuardInfo.Parser, new string[5] { "CityId", "GuardCount", "Fallen", "FallenEndTm", "GarrisonRoleIds" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
