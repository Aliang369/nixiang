using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateTargetNextGrandDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateTargetNextGrandDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLk9wZXJhdGVUYXJnZXROZXh0R3JhbmREZXRhaWwucHJvdG8SBXR5" + "cGVzGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvInQKHE9wZXJhdGVUYXJnZXRO" + "ZXh0R3JhbmREZXRhaWwSFAoMQ3VycmVudEluZGV4GAEgASgFEhIKCkdyYW5k" + "SW5kZXgYAiABKAUSKgoOR3JhbmRBd2FyZExpc3QYAyADKAsyEi50eXBlcy5B" + "d2FyZERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeAwardDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateTargetNextGrandDetail), (MessageParser)(object)OperateTargetNextGrandDetail.Parser, new string[3] { "CurrentIndex", "GrandIndex", "GrandAwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
