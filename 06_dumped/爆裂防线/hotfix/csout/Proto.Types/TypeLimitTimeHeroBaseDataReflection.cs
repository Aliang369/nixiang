using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLimitTimeHeroBaseDataReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLimitTimeHeroBaseDataReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkxpbWl0VGltZUhlcm9CYXNlRGF0YS5wcm90bxIFdHlwZXMiiAEK" + "FUxpbWl0VGltZUhlcm9CYXNlRGF0YRIPCgdIZXJvVGlkGAEgASgFEhAKCE9u" + "Y2VDb3N0GAIgASgFEhEKCU9uY2VTY29yZRgDIAEoBRISCgpUb3RhbFNjb3Jl" + "GAQgASgFEhEKCUxlZnRTY29yZRgFIAEoBRISCgpOZXh0RnJlZVRtGAYgASgD" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LimitTimeHeroBaseData), (MessageParser)(object)LimitTimeHeroBaseData.Parser, new string[6] { "HeroTid", "OnceCost", "OnceScore", "TotalScore", "LeftScore", "NextFreeTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
