using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeZhanLingCommonInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeZhanLingCommonInfoReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlpoYW5MaW5nQ29tbW9uSW5mby5wcm90bxIFdHlwZXMaFWVudW0u" + "RUJvb2xTdGF0ZS5wcm90byKBAQoSWmhhbkxpbmdDb21tb25JbmZvEg0KBVJl" + "YWNoGAEgASgFEiYKBkFjdGl2ZRgCIAEoDjIWLkVCb29sU3RhdGUuRUJvb2xT" + "dGF0ZRIPCgdHb29kc0lkGAMgASgFEhAKCExvd0F3YXJkGAQgASgFEhEKCUhp" + "Z2hBd2FyZBgFIAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBoolStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ZhanLingCommonInfo), (MessageParser)(object)ZhanLingCommonInfo.Parser, new string[5] { "Reach", "Active", "GoodsId", "LowAward", "HighAward" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
