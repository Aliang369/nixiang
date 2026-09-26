using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEBarPoolTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEBarPoolTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChdlbnVtLkVCYXJQb29sVHlwZS5wcm90bxIMRUJhclBvb2xUeXBlKl8KDEVC" + "YXJQb29sVHlwZRIOCgpFbXB0eVZhbHVlEAASCQoFRmlyc3QQARIMCghGaXJz" + "dFRlbhACEg0KCUNvbW1vblRlbhADEgsKB1JhcmVUZW4QBBIKCgZDb21tb24Q" + "BUIuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xF" + "bnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EBarPoolType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
