using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTruckMatchListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTruckMatchListAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlRydWNrTWF0Y2hMaXN0QWNrLnByb3RvEgV0eXBlcxobdHlwZS5U" + "cnVja01hdGNoRGV0YWlsLnByb3RvIj8KEVRydWNrTWF0Y2hMaXN0QWNrEioK" + "CU1hdGNoTGlzdBgBIAMoCzIXLnR5cGVzLlRydWNrTWF0Y2hEZXRhaWxCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeTruckMatchDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TruckMatchListAck), (MessageParser)(object)TruckMatchListAck.Parser, new string[1] { "MatchList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
