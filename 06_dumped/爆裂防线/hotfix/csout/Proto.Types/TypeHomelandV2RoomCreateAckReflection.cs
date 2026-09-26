using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHomelandV2RoomCreateAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHomelandV2RoomCreateAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkhvbWVsYW5kVjJSb29tQ3JlYXRlQWNrLnByb3RvEgV0eXBlcxoV" + "dHlwZS5EYXRhQ2hhbmdlLnByb3RvGhZ0eXBlLkRlZmVuc2VJbmZvLnByb3Rv" + "Ghd0eXBlLkhvbWVsYW5kSW5mby5wcm90bxoWdHlwZS5XYXJSb29tSW5mby5w" + "cm90byK9AQoXSG9tZWxhbmRWMlJvb21DcmVhdGVBY2sSKQoMSG9tZWxhbmRJ" + "bmZvGAEgASgLMhMudHlwZXMuSG9tZWxhbmRJbmZvEicKC1dhclJvb21JbmZv" + "GAIgASgLMhIudHlwZXMuV2FyUm9vbUluZm8SJwoLRGVmZW5zZUluZm8YAyAB" + "KAsyEi50eXBlcy5EZWZlbnNlSW5mbxIlCgpEYXRhQ2hhbmdlGAQgASgLMhEu" + "dHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oC" + "ElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeDataChangeReflection.Descriptor,
			TypeDefenseInfoReflection.Descriptor,
			TypeHomelandInfoReflection.Descriptor,
			TypeWarRoomInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HomelandV2RoomCreateAck), (MessageParser)(object)HomelandV2RoomCreateAck.Parser, new string[4] { "HomelandInfo", "WarRoomInfo", "DefenseInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
