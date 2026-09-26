using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWarRoomInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWarRoomInfoReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLldhclJvb21JbmZvLnByb3RvEgV0eXBlcxoYdHlwZS5XYXJSb29t" + "RGV0YWlsLnByb3RvIjUKC1dhclJvb21JbmZvEiYKCFJvb21MaXN0GAEgAygL" + "MhQudHlwZXMuV2FyUm9vbURldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeWarRoomDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WarRoomInfo), (MessageParser)(object)WarRoomInfo.Parser, new string[1] { "RoomList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
