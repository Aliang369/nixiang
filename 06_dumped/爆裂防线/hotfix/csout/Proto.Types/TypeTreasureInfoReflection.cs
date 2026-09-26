using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTreasureInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTreasureInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLlRyZWFzdXJlSW5mby5wcm90bxIFdHlwZXMaH3R5cGUuQmF0dGxl" + "VGVhbUF0dHJEZXRhaWwucHJvdG8aGXR5cGUuVHJlYXN1cmVEZXRhaWwucHJv" + "dG8iagoMVHJlYXN1cmVJbmZvEisKDFRyZWFzdXJlTGlzdBgBIAMoCzIVLnR5" + "cGVzLlRyZWFzdXJlRGV0YWlsEi0KCEF0dHJMaXN0GAIgAygLMhsudHlwZXMu" + "QmF0dGxlVGVhbUF0dHJEZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeBattleTeamAttrDetailReflection.Descriptor,
			TypeTreasureDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TreasureInfo), (MessageParser)(object)TreasureInfo.Parser, new string[2] { "TreasureList", "AttrList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
