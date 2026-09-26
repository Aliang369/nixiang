using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTruckTransportInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTruckTransportInfoReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlRydWNrVHJhbnNwb3J0SW5mby5wcm90bxIFdHlwZXMaH3R5cGUu" + "VHJ1Y2tSb2JSZWNvcmREZXRhaWwucHJvdG8aH3R5cGUuVHJ1Y2tUcmFuc3Bv" + "cnREZXRhaWwucHJvdG8ieQoSVHJ1Y2tUcmFuc3BvcnRJbmZvEjIKDVRyYW5z" + "cG9ydEluZm8YASABKAsyGy50eXBlcy5UcnVja1RyYW5zcG9ydERldGFpbBIv" + "CgpSZWNvcmRMaXN0GAIgAygLMhsudHlwZXMuVHJ1Y2tSb2JSZWNvcmREZXRh" + "aWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeTruckRobRecordDetailReflection.Descriptor,
			TypeTruckTransportDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TruckTransportInfo), (MessageParser)(object)TruckTransportInfo.Parser, new string[2] { "TransportInfo", "RecordList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
