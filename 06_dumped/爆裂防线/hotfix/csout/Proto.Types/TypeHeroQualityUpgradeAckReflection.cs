using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHeroQualityUpgradeAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroQualityUpgradeAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkhlcm9RdWFsaXR5VXBncmFkZUFjay5wcm90bxIFdHlwZXMaFXR5" + "cGUuRGF0YUNoYW5nZS5wcm90byKWAQoVSGVyb1F1YWxpdHlVcGdyYWRlQWNr" + "EiUKCkRhdGFDaGFuZ2UYASABKAsyES50eXBlcy5EYXRhQ2hhbmdlEhIKCk9s" + "ZFF1YWxpdHkYAiABKAUSFgoOT2xkUXVhbGl0eVN0YXIYAyABKAUSEgoKTmV3" + "UXVhbGl0eRgEIAEoBRIWCg5OZXdRdWFsaXR5U3RhchgFIAEoBUIuWgkuLztw" + "cm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeDataChangeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroQualityUpgradeAck), (MessageParser)(object)HeroQualityUpgradeAck.Parser, new string[5] { "DataChange", "OldQuality", "OldQualityStar", "NewQuality", "NewQualityStar" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
