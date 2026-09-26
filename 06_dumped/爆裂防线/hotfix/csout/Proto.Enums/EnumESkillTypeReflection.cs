using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumESkillTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumESkillTypeReflection()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChVlbnVtLkVTa2lsbFR5cGUucHJvdG8SCkVTa2lsbFR5cGUqzQEKCkVTa2ls" + "bFR5cGUSDgoKRW1wdHlWYWx1ZRAAEgkKBVFpYW5nEAESBwoDUGFvEAISCwoH" + "SmlndWFuZxADEgsKB0JpbmdkYW4QBBIOCgpSYW5zaGFvZGFuEAUSCwoHSG9u" + "Z3poYRAGEgoKBkRpYW5jaRAHEgsKB1d1cmVuamkQCBIJCgVNb3R1bxAJEgsK" + "B0JpbmdiYW8QChILCgdTaGV4aWFuEAsSCwoHRGlhbnFpdRAMEgwKCExvbmdq" + "dWFuEA0SCwoHRmVuZ3JlbhAOQi5aCS4vO3Byb3RvY6oCC1Byb3RvLkVudW1z" + "ygISWmJnYW1lXFByb3RvXEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ESkillType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
