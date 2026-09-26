using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeEndlessMineBaseInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEndlessMineBaseInfoReflection()
	{
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Expected O, but got Unknown
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkVuZGxlc3NNaW5lQmFzZUluZm8ucHJvdG8SBXR5cGVzGhVlbnVt" + "LkVCb29sU3RhdGUucHJvdG8aJHR5cGUuRW5kbGVzc01pbmVFeGNoYW5nZURl" + "dGFpbC5wcm90bxogdHlwZS5FbmRsZXNzTWluZVByb3BEZXRhaWwucHJvdG8a" + "HnR5cGUuT3BlcmF0ZVRhcmdldERldGFpbC5wcm90bxondHlwZS5PcGVyYXRl" + "VGFyZ2V0TmV4dEdyYW5kRGV0YWlsLnByb3RvIs0CChNFbmRsZXNzTWluZUJh" + "c2VJbmZvEi0KDUFjdGl2YXRlQXdhcmQYASABKA4yFi5FQm9vbFN0YXRlLkVC" + "b29sU3RhdGUSDwoHU3RhcnRUbRgCIAEoAxINCgVFbmRUbRgDIAEoAxINCgVE" + "ZXB0aBgEIAEoBRIuCghQcm9wTGlzdBgFIAMoCzIcLnR5cGVzLkVuZGxlc3NN" + "aW5lUHJvcERldGFpbBI4Cg5FeGNoYW5nZURldGFpbBgGIAEoCzIgLnR5cGVz" + "LkVuZGxlc3NNaW5lRXhjaGFuZ2VEZXRhaWwSPgoRRXhjaGFuZ2VOZXh0R3Jh" + "bmQYByABKAsyIy50eXBlcy5PcGVyYXRlVGFyZ2V0TmV4dEdyYW5kRGV0YWls" + "Ei4KClRhcmdldExpc3QYCCADKAsyGi50eXBlcy5PcGVyYXRlVGFyZ2V0RGV0" + "YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeEndlessMineExchangeDetailReflection.Descriptor,
			TypeEndlessMinePropDetailReflection.Descriptor,
			TypeOperateTargetDetailReflection.Descriptor,
			TypeOperateTargetNextGrandDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EndlessMineBaseInfo), (MessageParser)(object)EndlessMineBaseInfo.Parser, new string[8] { "ActivateAward", "StartTm", "EndTm", "Depth", "PropList", "ExchangeDetail", "ExchangeNextGrand", "TargetList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
