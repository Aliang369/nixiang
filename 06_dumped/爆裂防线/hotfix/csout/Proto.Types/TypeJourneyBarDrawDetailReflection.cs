using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeJourneyBarDrawDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneyBarDrawDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkpvdXJuZXlCYXJEcmF3RGV0YWlsLnByb3RvEgV0eXBlcxokdHlw" + "ZS5Kb3VybmV5VGFyZ2V0UmVjb3JkRGV0YWlsLnByb3RvInEKFEpvdXJuZXlC" + "YXJEcmF3RGV0YWlsEg8KB1JhcmVIaXQYASABKAUSEgoKVG90YWxDb3VudBgC" + "IAEoBRI0CgpUYXJnZXRMaXN0GAMgAygLMiAudHlwZXMuSm91cm5leVRhcmdl" + "dFJlY29yZERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeJourneyTargetRecordDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneyBarDrawDetail), (MessageParser)(object)JourneyBarDrawDetail.Parser, new string[3] { "RareHit", "TotalCount", "TargetList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
