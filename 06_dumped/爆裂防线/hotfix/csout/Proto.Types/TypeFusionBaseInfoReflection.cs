using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFusionBaseInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFusionBaseInfoReflection()
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkZ1c2lvbkJhc2VJbmZvLnByb3RvEgV0eXBlcxoedHlwZS5PcGVy" + "YXRlVGFyZ2V0RGV0YWlsLnByb3RvIvcBCg5GdXNpb25CYXNlSW5mbxIUCgxI" + "aXN0b3J5U2NvcmUYASABKAUSEAoIQWN0U2NvcmUYAiABKAUSEQoJQmVzdFNj" + "b3JlGAMgASgFEg0KBVNjb3JlGAQgASgFEg0KBUNsZWFuGAUgASgFEhAKCENv" + "bW1pdFRtGAYgASgDEi4KClRhcmdldExpc3QYByADKAsyGi50eXBlcy5PcGVy" + "YXRlVGFyZ2V0RGV0YWlsEhYKDkRhaWx5QmVzdFNjb3JlGAggASgFEjIKDkRh" + "aWx5QXdhcmRMaXN0GAkgAygLMhoudHlwZXMuT3BlcmF0ZVRhcmdldERldGFp" + "bEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeOperateTargetDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FusionBaseInfo), (MessageParser)(object)FusionBaseInfo.Parser, new string[9] { "HistoryScore", "ActScore", "BestScore", "Score", "Clean", "CommitTm", "TargetList", "DailyBestScore", "DailyAwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
