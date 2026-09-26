using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEWorldBuildingTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEWorldBuildingTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch1lbnVtLkVXb3JsZEJ1aWxkaW5nVHlwZS5wcm90bxISRVdvcmxkQnVpbGRp" + "bmdUeXBlKlMKEkVXb3JsZEJ1aWxkaW5nVHlwZRIOCgpFbXB0eVZhbHVlEAAS" + "CAoESG9tZRABEgoKBkNvbW1vbhACEgwKCFJlc291cmNlEAMSCQoFRXZlbnQQ" + "BEIuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xF" + "bnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EWorldBuildingType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
