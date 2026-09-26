using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonMarchArriveNtfReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonMarchArriveNtfReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLlNlYXNvbk1hcmNoQXJyaXZlTnRmLnByb3RvEgV0eXBlcyJ2ChRT" + "ZWFzb25NYXJjaEFycml2ZU50ZhIOCgZUZWFtSWQYASABKAUSDAoEUG9zWBgC" + "IAEoAhIMCgRQb3NZGAMgASgCEg4KBkFyZWFJZBgEIAEoBRIRCglPY2N1cHlD" + "aGcYBSABKAUSDwoHR2FzTGVmdBgGIAEoBUIuWgkuLztwcm90b2OqAgtQcm90" + "by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonMarchArriveNtf), (MessageParser)(object)SeasonMarchArriveNtf.Parser, new string[6] { "TeamId", "PosX", "PosY", "AreaId", "OccupyChg", "GasLeft" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
