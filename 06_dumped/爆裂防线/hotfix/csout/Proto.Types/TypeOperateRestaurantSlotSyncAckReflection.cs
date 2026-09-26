using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateRestaurantSlotSyncAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateRestaurantSlotSyncAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLk9wZXJhdGVSZXN0YXVyYW50U2xvdFN5bmNBY2sucHJvdG8SBXR5" + "cGVzGht0eXBlLlJlc3RhdXJhbnREZXRhaWwucHJvdG8iTwocT3BlcmF0ZVJl" + "c3RhdXJhbnRTbG90U3luY0FjaxIvCg5SZXN0YXVyYW50TGlzdBgBIAMoCzIX" + "LnR5cGVzLlJlc3RhdXJhbnREZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8u" + "VHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRestaurantDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateRestaurantSlotSyncAck), (MessageParser)(object)OperateRestaurantSlotSyncAck.Parser, new string[1] { "RestaurantList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
