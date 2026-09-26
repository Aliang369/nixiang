using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonMarchSpeedUpAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonMarchSpeedUpAckReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlNlYXNvbk1hcmNoU3BlZWRVcEFjay5wcm90bxIFdHlwZXMaG3R5" + "cGUuU2Vhc29uTW92ZURldGFpbC5wcm90byJhChVTZWFzb25NYXJjaFNwZWVk" + "VXBBY2sSJQoETW92ZRgBIAEoCzIXLnR5cGVzLlNlYXNvbk1vdmVEZXRhaWwS" + "DAoEQ29zdBgCIAEoBRITCgtEaWFtb25kTGVmdBgDIAEoBUIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSeasonMoveDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonMarchSpeedUpAck), (MessageParser)(object)SeasonMarchSpeedUpAck.Parser, new string[3] { "Move", "Cost", "DiamondLeft" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
