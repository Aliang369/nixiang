using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeWorldMapGridDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldMapGridDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLldvcmxkTWFwR3JpZERldGFpbC5wcm90bxIFdHlwZXMaHWVudW0u" + "RVdvcmxkTWFwR3JpZFN0YXRlLnByb3RvIoUBChJXb3JsZE1hcEdyaWREZXRh" + "aWwSDgoGR3JpZElkGAEgASgFEgkKAVgYAiABKAUSCQoBWRgDIAEoBRISCgpC" + "dWlsZGluZ0lkGAQgASgFEjUKBVN0YXRlGAUgASgOMiYuRVdvcmxkTWFwR3Jp" + "ZFN0YXRlLkVXb3JsZE1hcEdyaWRTdGF0ZUIuWgkuLztwcm90b2OqAgtQcm90" + "by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEWorldMapGridStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldMapGridDetail), (MessageParser)(object)WorldMapGridDetail.Parser, new string[5] { "GridId", "X", "Y", "BuildingId", "State" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
