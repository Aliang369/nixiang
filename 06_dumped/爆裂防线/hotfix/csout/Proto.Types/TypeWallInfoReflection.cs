using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWallInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWallInfoReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChN0eXBlLldhbGxJbmZvLnByb3RvEgV0eXBlcxoVdHlwZS5XYWxsRGV0YWls" + "LnByb3RvIocBCghXYWxsSW5mbxINCgVMZXZlbBgBIAEoBRIjCghXYWxsTGlz" + "dBgCIAMoCzIRLnR5cGVzLldhbGxEZXRhaWwSDgoGV2FsbElkGAMgASgFEg8K" + "B1VzZUxpc3QYBCADKAUSDwoHVG90YWxIcBgFIAEoBRIVCg1Ub3RhbEltbXVu" + "aXR5GAYgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1l" + "XFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeWallDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WallInfo), (MessageParser)(object)WallInfo.Parser, new string[6] { "Level", "WallList", "WallId", "UseList", "TotalHp", "TotalImmunity" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
