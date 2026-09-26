using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMineWarOccupyPointAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMineWarOccupyPointAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLk1pbmVXYXJPY2N1cHlQb2ludEFjay5wcm90bxIFdHlwZXMaFnR5" + "cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90bxoa" + "dHlwZS5NaW5lV2FyQmFzZUluZm8ucHJvdG8aGXR5cGUuTWluZVdhck1hcElu" + "Zm8ucHJvdG8ixQEKFU1pbmVXYXJPY2N1cHlQb2ludEFjaxIvCg9NaW5lV2Fy" + "QmFzZUluZm8YASABKAsyFi50eXBlcy5NaW5lV2FyQmFzZUluZm8SLQoOTWlu" + "ZVdhck1hcEluZm8YAiABKAsyFS50eXBlcy5NaW5lV2FyTWFwSW5mbxIlCglB" + "d2FyZExpc3QYAyADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hh" + "bmdlGAQgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeMineWarBaseInfoReflection.Descriptor,
			TypeMineWarMapInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MineWarOccupyPointAck), (MessageParser)(object)MineWarOccupyPointAck.Parser, new string[4] { "MineWarBaseInfo", "MineWarMapInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
