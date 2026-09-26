using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class RolePvpTipsInfo : IMessage<RolePvpTipsInfo>, IMessage, IEquatable<RolePvpTipsInfo>, IDeepCloneable<RolePvpTipsInfo>, IBufferMessage
{
	private static readonly MessageParser<RolePvpTipsInfo> _parser = new MessageParser<RolePvpTipsInfo>((Func<RolePvpTipsInfo>)(() => new RolePvpTipsInfo()));

	private UnknownFieldSet _unknownFields;

	public const int RoleBaseInfoFieldNumber = 1;

	private RoleBaseInfo roleBaseInfo_;

	public const int PvpModInfoFieldNumber = 2;

	private PvpModInfo pvpModInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RolePvpTipsInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRolePvpTipsInfoReflection.Descriptor.MessageTypes[0];

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
	public PvpModInfo PvpModInfo
	{
		get
		{
			return pvpModInfo_;
		}
		set
		{
			pvpModInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RolePvpTipsInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RolePvpTipsInfo(RolePvpTipsInfo other)
		: this()
	{
		roleBaseInfo_ = ((other.roleBaseInfo_ != null) ? other.roleBaseInfo_.Clone() : null);
		pvpModInfo_ = ((other.pvpModInfo_ != null) ? other.pvpModInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RolePvpTipsInfo Clone()
	{
		return new RolePvpTipsInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RolePvpTipsInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RolePvpTipsInfo other)
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
		if (!object.Equals(PvpModInfo, other.PvpModInfo))
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
		if (pvpModInfo_ != null)
		{
			num ^= PvpModInfo.GetHashCode();
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
		if (pvpModInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)PvpModInfo);
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
		if (pvpModInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)PvpModInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RolePvpTipsInfo other)
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
		if (other.pvpModInfo_ != null)
		{
			if (pvpModInfo_ == null)
			{
				PvpModInfo = new PvpModInfo();
			}
			PvpModInfo.MergeFrom(other.PvpModInfo);
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
				if (pvpModInfo_ == null)
				{
					PvpModInfo = new PvpModInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)PvpModInfo);
				break;
			}
		}
	}
}
