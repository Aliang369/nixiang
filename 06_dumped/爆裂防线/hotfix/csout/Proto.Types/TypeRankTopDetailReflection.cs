using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeRankTopDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRankTopDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLlJhbmtUb3BEZXRhaWwucHJvdG8SBXR5cGVzGhRlbnVtLkVSYW5r" + "VHlwZS5wcm90bxoVdHlwZS5SYW5rRGV0YWlsLnByb3RvIlwKDVJhbmtUb3BE" + "ZXRhaWwSJgoIUmFua1R5cGUYASABKA4yFC5FUmFua1R5cGUuRVJhbmtUeXBl" + "EiMKCFJhbmtJbmZvGAIgASgLMhEudHlwZXMuUmFua0RldGFpbEIuWgkuLztw" + "cm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumERankTypeReflection.Descriptor,
			TypeRankDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RankTopDetail), (MessageParser)(object)RankTopDetail.Parser, new string[2] { "RankType", "RankInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
