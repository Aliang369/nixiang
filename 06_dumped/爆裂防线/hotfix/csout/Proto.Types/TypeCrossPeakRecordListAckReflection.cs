using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossPeakRecordListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossPeakRecordListAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkNyb3NzUGVha1JlY29yZExpc3RBY2sucHJvdG8SBXR5cGVzGiB0" + "eXBlLkNyb3NzUGVha1JlY29yZERldGFpbC5wcm90byJKChZDcm9zc1BlYWtS" + "ZWNvcmRMaXN0QWNrEjAKClJlY29yZExpc3QYASADKAsyHC50eXBlcy5Dcm9z" + "c1BlYWtSZWNvcmREZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeCrossPeakRecordDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossPeakRecordListAck), (MessageParser)(object)CrossPeakRecordListAck.Parser, new string[1] { "RecordList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
