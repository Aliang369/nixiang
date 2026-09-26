using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeJourneySignDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneySignDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkpvdXJuZXlTaWduRGV0YWlsLnByb3RvEgV0eXBlcxokdHlwZS5K" + "b3VybmV5VGFyZ2V0UmVjb3JkRGV0YWlsLnByb3RvImUKEUpvdXJuZXlTaWdu" + "RGV0YWlsEgsKA0RheRgBIAEoBRINCgVSZWFjaBgCIAEoBRI0CgpUYXJnZXRM" + "aXN0GAMgAygLMiAudHlwZXMuSm91cm5leVRhcmdldFJlY29yZERldGFpbEIu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeJourneyTargetRecordDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneySignDetail), (MessageParser)(object)JourneySignDetail.Parser, new string[3] { "Day", "Reach", "TargetList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
