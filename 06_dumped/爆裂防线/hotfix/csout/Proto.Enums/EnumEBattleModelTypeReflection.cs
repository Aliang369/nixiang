using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEBattleModelTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEBattleModelTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChtlbnVtLkVCYXR0bGVNb2RlbFR5cGUucHJvdG8SEEVCYXR0bGVNb2RlbFR5" + "cGUqbQoQRUJhdHRsZU1vZGVsVHlwZRIOCgpFbXB0eVZhbHVlEAASCwoHS2ls" + "bEFsbBABEgoKBkhvbGRPbhACEg0KCU5vVmljdG9yeRADEgoKBlNpbmdsZRAE" + "EgwKCEtpbGxCb3NzEAUSBwoDUFZQEAZCLloJLi87cHJvdG9jqgILUHJvdG8u" + "RW51bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EBattleModelType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
