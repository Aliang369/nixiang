using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityEventCommonAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityEventCommonAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkZhY2lsaXR5RXZlbnRDb21tb25BY2sucHJvdG8SBXR5cGVzGhZ0" + "eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8a" + "GnR5cGUuRmFjaWxpdHlNYXBJbmZvLnByb3RvGh10eXBlLkZhY2lsaXR5Um9v" + "bURldGFpbC5wcm90byLGAQoWRmFjaWxpdHlFdmVudENvbW1vbkFjaxIvCg9G" + "YWNpbGl0eU1hcEluZm8YASABKAsyFi50eXBlcy5GYWNpbGl0eU1hcEluZm8S" + "LQoKUm9vbURldGFpbBgCIAEoCzIZLnR5cGVzLkZhY2lsaXR5Um9vbURldGFp" + "bBIlCglBd2FyZExpc3QYAyADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgpE" + "YXRhQ2hhbmdlGAQgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeFacilityMapInfoReflection.Descriptor,
			TypeFacilityRoomDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityEventCommonAck), (MessageParser)(object)FacilityEventCommonAck.Parser, new string[4] { "FacilityMapInfo", "RoomDetail", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
