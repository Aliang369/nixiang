using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHomelandV2RoomEditReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHomelandV2RoomEditReqReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkhvbWVsYW5kVjJSb29tRWRpdFJlcS5wcm90bxIFdHlwZXMaIXR5" + "cGUuSG9tZWxhbmRSb29tRWRpdERldGFpbC5wcm90byJIChVIb21lbGFuZFYy" + "Um9vbUVkaXRSZXESLwoIUm9vbUxpc3QYASADKAsyHS50eXBlcy5Ib21lbGFu" + "ZFJvb21FZGl0RGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygIS" + "WmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeHomelandRoomEditDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HomelandV2RoomEditReq), (MessageParser)(object)HomelandV2RoomEditReq.Parser, new string[1] { "RoomList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
