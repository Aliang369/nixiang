using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateRestaurantDailyAwardDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateRestaurantDailyAwardDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cix0eXBlLk9wZXJhdGVSZXN0YXVyYW50RGFpbHlBd2FyZERldGFpbC5wcm90" + "bxIFdHlwZXMiUQohT3BlcmF0ZVJlc3RhdXJhbnREYWlseUF3YXJkRGV0YWls" + "Eg0KBUNvdW50GAEgASgFEg0KBVRvdGFsGAIgASgFEg4KBk5leHRUbRgDIAEo" + "A0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateRestaurantDailyAwardDetail), (MessageParser)(object)OperateRestaurantDailyAwardDetail.Parser, new string[3] { "Count", "Total", "NextTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
