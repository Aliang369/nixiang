using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class RoleMiniTipsInfo : IMessage<RoleMiniTipsInfo>, IMessage, IEquatable<RoleMiniTipsInfo>, IDeepCloneable<RoleMiniTipsInfo>, IBufferMessage
{
	private static readonly MessageParser<RoleMiniTipsInfo> _parser = new MessageParser<RoleMiniTipsInfo>((Func<RoleMiniTipsInfo>)(() => new RoleMiniTipsInfo()));

	private UnknownFieldSet _unknownFields;

	public const int RoleBaseInfoFieldNumber = 1;

	private RoleBaseInfo roleBaseInfo_;

	public const int LegionFlagInfoFieldNumber = 2;

	private LegionFlagInfo legionFlagInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RoleMiniTipsInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRoleMiniTipsInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo RoleBaseInfo
	{
		get
		{
			return roleBaseInfo_;
		}
		set
		{
			roleBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionFlagInfo LegionFlagInfo
	{
		get
		{
			return legionFlagInfo_;
		}
		set
		{
			legionFlagInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleMiniTipsInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleMiniTipsInfo(RoleMiniTipsInfo other)
		: this()
	{
		roleBaseInfo_ = ((other.roleBaseInfo_ != null) ? other.roleBaseInfo_.Clone() : null);
		legionFlagInfo_ = ((other.legionFlagInfo_ != null) ? other.legionFlagInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleMiniTipsInfo Clone()
	{
		return new RoleMiniTipsInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RoleMiniTipsInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RoleMiniTipsInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(RoleBaseInfo, other.RoleBaseInfo))
		{
			return false;
		}
		if (!object.Equals(LegionFlagInfo, other.LegionFlagInfo))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (roleBaseInfo_ != null)
		{
			num ^= RoleBaseInfo.GetHashCode();
		}
		if (legionFlagInfo_ != null)
		{
			num ^= LegionFlagInfo.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= ((object)_unknownFields).GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (roleBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleBaseInfo);
		}
		if (legionFlagInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionFlagInfo);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CalculateSize()
	{
		int num = 0;
		if (roleBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleBaseInfo);
		}
		if (legionFlagInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionFlagInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RoleMiniTipsInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.roleBaseInfo_ != null)
		{
			if (roleBaseInfo_ == null)
			{
				RoleBaseInfo = new RoleBaseInfo();
			}
			RoleBaseInfo.MergeFrom(other.RoleBaseInfo);
		}
		if (other.legionFlagInfo_ != null)
		{
			if (legionFlagInfo_ == null)
			{
				LegionFlagInfo = new LegionFlagInfo();
			}
			LegionFlagInfo.MergeFrom(other.LegionFlagInfo);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalMergeFrom(ref ParseContext input)
	{
		uint num;
		while ((num = ((ParseContext)(ref input)).ReadTag()) != 0)
		{
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 10u:
				if (roleBaseInfo_ == null)
				{
					RoleBaseInfo = new RoleBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleBaseInfo);
				break;
			case 18u:
				if (legionFlagInfo_ == null)
				{
					LegionFlagInfo = new LegionFlagInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionFlagInfo);
				break;
			}
		}
	}
}
