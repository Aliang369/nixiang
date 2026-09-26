using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeDungeonMapCellDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDungeonMapCellDetailReflection()
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkR1bmdlb25NYXBDZWxsRGV0YWlsLnByb3RvEgV0eXBlcxohZW51" + "bS5FRHVuZ2Vvbk1hcENlbGxGdW5UeXBlLnByb3RvGh9lbnVtLkVEdW5nZW9u" + "TWFwQ2VsbFN0YXRlLnByb3RvGh90eXBlLkR1bmdlb25NYXBHcmlkRGV0YWls" + "LnByb3RvIowCChREdW5nZW9uTWFwQ2VsbERldGFpbBIOCgZDZWxsSWQYASAB" + "KAkSEAoIQ2VsbFR5cGUYAiABKAUSEQoJQ2VsbFN0eWxlGAMgASgFEj8KB0Z1" + "blR5cGUYBCABKA4yLi5FRHVuZ2Vvbk1hcENlbGxGdW5UeXBlLkVEdW5nZW9u" + "TWFwQ2VsbEZ1blR5cGUSCQoBWBgFIAEoBRIJCgFZGAYgASgFEjkKBVN0YXRl" + "GAcgASgOMiouRUR1bmdlb25NYXBDZWxsU3RhdGUuRUR1bmdlb25NYXBDZWxs" + "U3RhdGUSLQoIR3JpZExpc3QYCCADKAsyGy50eXBlcy5EdW5nZW9uTWFwR3Jp" + "ZERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEDungeonMapCellFunTypeReflection.Descriptor,
			EnumEDungeonMapCellStateReflection.Descriptor,
			TypeDungeonMapGridDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DungeonMapCellDetail), (MessageParser)(object)DungeonMapCellDetail.Parser, new string[8] { "CellId", "CellType", "CellStyle", "FunType", "X", "Y", "State", "GridList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
