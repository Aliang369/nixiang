using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionWarZhanLingInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarZhanLingInfoReflection()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkxlZ2lvbldhclpoYW5MaW5nSW5mby5wcm90bxIFdHlwZXMaFWVu" + "dW0uRUJvb2xTdGF0ZS5wcm90byLGAQoVTGVnaW9uV2FyWmhhbkxpbmdJbmZv" + "Eg4KBlNlYXNvbhgBIAEoBRINCgVTY29yZRgCIAEoBRINCgVMZXZlbBgDIAEo" + "BRIsCgxNZWRpdW1BY3RpdmUYBCABKA4yFi5FQm9vbFN0YXRlLkVCb29sU3Rh" + "dGUSKgoKSGlnaEFjdGl2ZRgFIAEoDjIWLkVCb29sU3RhdGUuRUJvb2xTdGF0" + "ZRIQCghMb3dBd2FyZBgGIAEoBRITCgtNZWRpdW1Bd2FyZBgHIAEoBUIuWgku" + "Lztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBoolStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarZhanLingInfo), (MessageParser)(object)LegionWarZhanLingInfo.Parser, new string[7] { "Season", "Score", "Level", "MediumActive", "HighActive", "LowAward", "MediumAward" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
