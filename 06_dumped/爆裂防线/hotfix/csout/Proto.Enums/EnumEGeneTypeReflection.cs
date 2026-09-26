using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEGeneTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEGeneTypeReflection()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChRlbnVtLkVHZW5lVHlwZS5wcm90bxIJRUdlbmVUeXBlKq4BCglFR2VuZVR5" + "cGUSDgoKRW1wdHlWYWx1ZRAAEgoKBkxldmVsMRABEgoKBkxldmVsMhACEgoK" + "BkxldmVsMxADEgoKBkxldmVsNBAEEgoKBkxldmVsNRAFEgoKBkxldmVsNhAG" + "EgoKBkxldmVsNxAHEgoKBkxldmVsOBAIEgoKBkxldmVsORAJEgsKB0xldmVs" + "MTAQChILCgdMZXZlbDExEAsSCwoHTGV2ZWwxMhAMQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLkVudW1zygISWmJnYW1lXFByb3RvXEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EGeneType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
