using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonCityWarResultNtfReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonCityWarResultNtfReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLlNlYXNvbkNpdHlXYXJSZXN1bHROdGYucHJvdG8SBXR5cGVzGh10" + "eXBlLlNlYXNvbkJhdHRsZVJlcG9ydC5wcm90byJDChZTZWFzb25DaXR5V2Fy" + "UmVzdWx0TnRmEikKBlJlcG9ydBgBIAEoCzIZLnR5cGVzLlNlYXNvbkJhdHRs" + "ZVJlcG9ydEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSeasonBattleReportReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonCityWarResultNtf), (MessageParser)(object)SeasonCityWarResultNtf.Parser, new string[1] { "Report" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
