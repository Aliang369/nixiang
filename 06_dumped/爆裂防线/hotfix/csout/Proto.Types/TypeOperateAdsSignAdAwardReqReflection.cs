using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateAdsSignAdAwardReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateAdsSignAdAwardReqReflection()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLk9wZXJhdGVBZHNTaWduQWRBd2FyZFJlcS5wcm90bxIFdHlwZXMi" + "RAoYT3BlcmF0ZUFkc1NpZ25BZEF3YXJkUmVxEgwKBFVVSUQYASABKAkSDQoF" + "QWR2SWQYAiABKAUSCwoDRGF5GAMgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateAdsSignAdAwardReq), (MessageParser)(object)OperateAdsSignAdAwardReq.Parser, new string[3] { "UUID", "AdvId", "Day" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
