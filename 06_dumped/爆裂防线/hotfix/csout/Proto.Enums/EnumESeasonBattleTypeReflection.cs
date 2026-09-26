using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumESeasonBattleTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumESeasonBattleTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChxlbnVtLkVTZWFzb25CYXR0bGVUeXBlLnByb3RvEhFFU2Vhc29uQmF0dGxl" + "VHlwZSpXChFFU2Vhc29uQmF0dGxlVHlwZRIOCgpFbXB0eVZhbHVlEAASCwoH" + "Q2l0eVB2UBABEgsKB0NpdHlQdkUQAhILCgdBaXJkcm9wEAMSCwoHSW1wZWFj" + "aBAEQi5aCS4vO3Byb3RvY6oCC1Byb3RvLkVudW1zygISWmJnYW1lXFByb3Rv" + "XEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ESeasonBattleType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
