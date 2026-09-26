using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRechargeActiveDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRechargeActiveDataAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlJlY2hhcmdlQWN0aXZlRGF0YUFjay5wcm90bxIFdHlwZXMaHXR5" + "cGUuUmVjaGFyZ2VBY3RpdmVJbmZvLnByb3RvIk4KFVJlY2hhcmdlQWN0aXZl" + "RGF0YUFjaxI1ChJSZWNoYXJnZUFjdGl2ZUluZm8YASABKAsyGS50eXBlcy5S" + "ZWNoYXJnZUFjdGl2ZUluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRechargeActiveInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RechargeActiveDataAck), (MessageParser)(object)RechargeActiveDataAck.Parser, new string[1] { "RechargeActiveInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
