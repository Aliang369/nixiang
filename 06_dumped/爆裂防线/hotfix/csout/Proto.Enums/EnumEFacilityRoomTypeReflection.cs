using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEFacilityRoomTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEFacilityRoomTypeReflection()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChxlbnVtLkVGYWNpbGl0eVJvb21UeXBlLnByb3RvEhFFRmFjaWxpdHlSb29t" + "VHlwZSqQAQoRRUZhY2lsaXR5Um9vbVR5cGUSDgoKRW1wdHlWYWx1ZRAAEgkK" + "BVRyYXNoEAESCQoFQ2hlc3QQAhIPCgtMb2NrZWRDaGVzdBADEgsKB01vbnN0" + "ZXIQBBIICgRTaG9wEAUSCwoHUm9sbGluZxAGEhEKDUVudHJhbmNlU3RhaXIQ" + "BxINCglFeGl0U3RhaXIQCEIuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oC" + "ElpiZ2FtZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EFacilityRoomType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
