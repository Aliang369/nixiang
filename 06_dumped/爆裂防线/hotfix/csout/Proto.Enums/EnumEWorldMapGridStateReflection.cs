using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEWorldMapGridStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEWorldMapGridStateReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch1lbnVtLkVXb3JsZE1hcEdyaWRTdGF0ZS5wcm90bxISRVdvcmxkTWFwR3Jp" + "ZFN0YXRlKk4KEkVXb3JsZE1hcEdyaWRTdGF0ZRINCglJbnZpc2libGUQABIL" + "CgdWaXNpYmxlEAESDgoKRXhwbG9yYWJsZRACEgwKCEV4cGxvcmVkEANCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EWorldMapGridState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
