using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEChatActionTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEChatActionTypeReflection()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChplbnVtLkVDaGF0QWN0aW9uVHlwZS5wcm90bxIPRUNoYXRBY3Rpb25UeXBl" + "KosBCg9FQ2hhdEFjdGlvblR5cGUSDgoKRW1wdHlWYWx1ZRAAEggKBFRleHQQ" + "ARIQCgxMZWdpb25JbnZpdGUQAhIMCghCYXJTaGFyZRADEg0KCUhlcm9TaGFy" + "ZRAEEg4KCkVxdWlwU2hhcmUQBRIQCgxCYXR0bGVSZXN1bHQQBhINCglXb3Js" + "ZExhaXIQB0IuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQ" + "cm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EChatActionType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
