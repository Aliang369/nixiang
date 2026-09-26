using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMechaDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMechaDetailReflection()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLk1lY2hhRGV0YWlsLnByb3RvEgV0eXBlcxogdHlwZS5CYXR0bGVN" + "ZWNoYUF0dHJEZXRhaWwucHJvdG8aG3R5cGUuTWVjaGFTa2lsbERldGFpbC5w" + "cm90byKnAQoLTWVjaGFEZXRhaWwSDwoHTWVjaGFJZBgBIAEoBRINCgVMZXZl" + "bBgCIAEoBRIMCgRTdGFyGAMgASgFEioKCVNraWxsTGlzdBgEIAMoCzIXLnR5" + "cGVzLk1lY2hhU2tpbGxEZXRhaWwSLgoIQXR0ckxpc3QYBSADKAsyHC50eXBl" + "cy5CYXR0bGVNZWNoYUF0dHJEZXRhaWwSDgoGQXR0YWNrGAYgASgFQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeBattleMechaAttrDetailReflection.Descriptor,
			TypeMechaSkillDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MechaDetail), (MessageParser)(object)MechaDetail.Parser, new string[6] { "MechaId", "Level", "Star", "SkillList", "AttrList", "Attack" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
