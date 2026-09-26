using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumELegionWarStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumELegionWarStateReflection()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChplbnVtLkVMZWdpb25XYXJTdGF0ZS5wcm90bxIPRUxlZ2lvbldhclN0YXRl" + "Kn8KD0VMZWdpb25XYXJTdGF0ZRIOCgpFbXB0eVZhbHVlEAASCwoHUHJldmll" + "dxABEgoKBlNpZ25VcBACEgsKB1ByZXBhcmUQAxIMCghGaWdodGluZxAEEgwK" + "CFNldHRsaW5nEAUSDAoIRmluaXNoZWQQBhIMCghXYWl0TmV4dBAHQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLkVudW1zygISWmJnYW1lXFByb3RvXEVudW1zYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ELegionWarState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
