using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossLadderRecordListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderRecordListAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkNyb3NzTGFkZGVyUmVjb3JkTGlzdEFjay5wcm90bxIFdHlwZXMa" + "InR5cGUuQ3Jvc3NMYWRkZXJSZWNvcmREZXRhaWwucHJvdG8iTgoYQ3Jvc3NM" + "YWRkZXJSZWNvcmRMaXN0QWNrEjIKClJlY29yZExpc3QYASADKAsyHi50eXBl" + "cy5Dcm9zc0xhZGRlclJlY29yZERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90" + "by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeCrossLadderRecordDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderRecordListAck), (MessageParser)(object)CrossLadderRecordListAck.Parser, new string[1] { "RecordList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
