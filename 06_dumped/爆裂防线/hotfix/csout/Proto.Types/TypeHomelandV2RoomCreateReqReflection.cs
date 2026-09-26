using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHomelandV2RoomCreateReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHomelandV2RoomCreateReqReflection()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkhvbWVsYW5kVjJSb29tQ3JlYXRlUmVxLnByb3RvEgV0eXBlcyI/" + "ChdIb21lbGFuZFYyUm9vbUNyZWF0ZVJlcRIOCgZUeXBlSWQYASABKAUSCQoB" + "WBgCIAEoBRIJCgFZGAMgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HomelandV2RoomCreateReq), (MessageParser)(object)HomelandV2RoomCreateReq.Parser, new string[3] { "TypeId", "X", "Y" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
