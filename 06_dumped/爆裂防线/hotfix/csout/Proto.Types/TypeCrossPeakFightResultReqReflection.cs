using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossPeakFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossPeakFightResultReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkNyb3NzUGVha0ZpZ2h0UmVzdWx0UmVxLnByb3RvEgV0eXBlcxob" + "dHlwZS5CYXR0bGVSZXN1bHRJbmZvLnByb3RvImgKF0Nyb3NzUGVha0ZpZ2h0" + "UmVzdWx0UmVxEhAKCFBsYXllcklkGAEgASgFEgwKBFJhbmsYAiABKAUSLQoM" + "QmF0dGxlUmVzdWx0GAMgASgLMhcudHlwZXMuQmF0dGxlUmVzdWx0SW5mb0Iu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossPeakFightResultReq), (MessageParser)(object)CrossPeakFightResultReq.Parser, new string[3] { "PlayerId", "Rank", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
