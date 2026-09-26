using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateSubjectRouletteInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateSubjectRouletteInfoReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLk9wZXJhdGVTdWJqZWN0Um91bGV0dGVJbmZvLnByb3RvEgV0eXBl" + "cxomdHlwZS5MaW1pdFRpbWVSb3VsZXR0ZVJhbmREZXRhaWwucHJvdG8aHnR5" + "cGUuT3BlcmF0ZVRhcmdldERldGFpbC5wcm90byLKAQoaT3BlcmF0ZVN1Ympl" + "Y3RSb3VsZXR0ZUluZm8SEQoJT25jZUx1Y2t5GAEgASgFEhAKCE1heEx1Y2t5" + "GAIgASgFEg0KBUx1Y2t5GAMgASgFEhIKClRvdGFsQ291bnQYBCABKAUSNAoI" + "U2xvdExpc3QYBSADKAsyIi50eXBlcy5MaW1pdFRpbWVSb3VsZXR0ZVJhbmRE" + "ZXRhaWwSLgoKVGFyZ2V0TGlzdBgGIAMoCzIaLnR5cGVzLk9wZXJhdGVUYXJn" + "ZXREZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeLimitTimeRouletteRandDetailReflection.Descriptor,
			TypeOperateTargetDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateSubjectRouletteInfo), (MessageParser)(object)OperateSubjectRouletteInfo.Parser, new string[6] { "OnceLucky", "MaxLucky", "Lucky", "TotalCount", "SlotList", "TargetList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
