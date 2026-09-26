using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionOfficialSetReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionOfficialSetReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkxlZ2lvbk9mZmljaWFsU2V0UmVxLnByb3RvEgV0eXBlcxoaZW51" + "bS5FTGVnaW9uT2ZmaWNpYWwucHJvdG8iXAoUTGVnaW9uT2ZmaWNpYWxTZXRS" + "ZXESEAoIUGxheWVySWQYASABKAUSMgoIT2ZmaWNpYWwYAiABKA4yIC5FTGVn" + "aW9uT2ZmaWNpYWwuRUxlZ2lvbk9mZmljaWFsQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumELegionOfficialReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionOfficialSetReq), (MessageParser)(object)LegionOfficialSetReq.Parser, new string[2] { "PlayerId", "Official" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
