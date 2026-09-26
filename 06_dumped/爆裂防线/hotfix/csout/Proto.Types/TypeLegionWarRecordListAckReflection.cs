using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarRecordListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarRecordListAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkxlZ2lvbldhclJlY29yZExpc3RBY2sucHJvdG8SBXR5cGVzGiB0" + "eXBlLkxlZ2lvbldhclJlY29yZERldGFpbC5wcm90byJKChZMZWdpb25XYXJS" + "ZWNvcmRMaXN0QWNrEjAKClJlY29yZExpc3QYASADKAsyHC50eXBlcy5MZWdp" + "b25XYXJSZWNvcmREZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeLegionWarRecordDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarRecordListAck), (MessageParser)(object)LegionWarRecordListAck.Parser, new string[1] { "RecordList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
