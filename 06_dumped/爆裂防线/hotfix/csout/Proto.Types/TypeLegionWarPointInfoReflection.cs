using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarPointInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarPointInfoReflection()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkxlZ2lvbldhclBvaW50SW5mby5wcm90bxIFdHlwZXMaInR5cGUu" + "TGVnaW9uV2FyRmlnaHRpbmdEZXRhaWwucHJvdG8aJXR5cGUuTGVnaW9uV2Fy" + "UG9pbnRQbGF5ZXJEZXRhaWwucHJvdG8aJHR5cGUuTGVnaW9uV2FyUG9pbnRT" + "dGF0ZURldGFpbC5wcm90byK3AQoSTGVnaW9uV2FyUG9pbnRJbmZvEjQKClBv" + "aW50U3RhdGUYASABKAsyIC50eXBlcy5MZWdpb25XYXJQb2ludFN0YXRlRGV0" + "YWlsEjUKClBsYXllckxpc3QYAiADKAsyIS50eXBlcy5MZWdpb25XYXJQb2lu" + "dFBsYXllckRldGFpbBI0CgxGaWdodGluZ0xpc3QYAyADKAsyHi50eXBlcy5M" + "ZWdpb25XYXJGaWdodGluZ0RldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeLegionWarFightingDetailReflection.Descriptor,
			TypeLegionWarPointPlayerDetailReflection.Descriptor,
			TypeLegionWarPointStateDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarPointInfo), (MessageParser)(object)LegionWarPointInfo.Parser, new string[3] { "PointState", "PlayerList", "FightingList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
