using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEndlessMineMapRowDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEndlessMineMapRowDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkVuZGxlc3NNaW5lTWFwUm93RGV0YWlsLnByb3RvEgV0eXBlcxoj" + "dHlwZS5FbmRsZXNzTWluZU1hcENlbGxEZXRhaWwucHJvdG8iWQoXRW5kbGVz" + "c01pbmVNYXBSb3dEZXRhaWwSCwoDUm93GAEgASgFEjEKCENlbGxMaXN0GAIg" + "AygLMh8udHlwZXMuRW5kbGVzc01pbmVNYXBDZWxsRGV0YWlsQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeEndlessMineMapCellDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EndlessMineMapRowDetail), (MessageParser)(object)EndlessMineMapRowDetail.Parser, new string[2] { "Row", "CellList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
