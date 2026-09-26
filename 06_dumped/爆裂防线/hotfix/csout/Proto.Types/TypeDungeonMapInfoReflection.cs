using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeDungeonMapInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDungeonMapInfoReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkR1bmdlb25NYXBJbmZvLnByb3RvEgV0eXBlcxofdHlwZS5EdW5n" + "ZW9uTWFwQ2VsbERldGFpbC5wcm90byJbCg5EdW5nZW9uTWFwSW5mbxIMCgRN" + "YXhYGAEgASgFEgwKBE1heFkYAiABKAUSLQoIQ2VsbExpc3QYAyADKAsyGy50" + "eXBlcy5EdW5nZW9uTWFwQ2VsbERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90" + "by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeDungeonMapCellDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DungeonMapInfo), (MessageParser)(object)DungeonMapInfo.Parser, new string[3] { "MaxX", "MaxY", "CellList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
