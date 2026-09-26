using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEBoxTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEBoxTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChNlbnVtLkVCb3hUeXBlLnByb3RvEghFQm94VHlwZSpiCghFQm94VHlwZRIO" + "CgpFbXB0eVZhbHVlEAASCwoHUmFuZEJveBABEg0KCVNlbGVjdEJveBACEg4K" + "CkNvbXBvc2VCb3gQAxIMCghBd2FyZEJveBAEEgwKCEF1dG9PcGVuEAVCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EBoxType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
