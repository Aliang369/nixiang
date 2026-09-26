using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PlayerProcessDetail : IMessage<PlayerProcessDetail>, IMessage, IEquatable<PlayerProcessDetail>, IDeepCloneable<PlayerProcessDetail>, IBufferMessage
{
	private static readonly MessageParser<PlayerProcessDetail> _parser = new MessageParser<PlayerProcessDetail>((Func<PlayerProcessDetail>)(() => new PlayerProcessDetail()));

	private UnknownFieldSet _unknownFields;

	public const int RoleBaseInfoFieldNumber = 1;

	private RoleBaseInfo roleBaseInfo_;

	public const int MainLevelFieldNumber = 2;

	private int mainLevel_;

	public const int EliteLevelFieldNumber = 3;

	private int eliteLevel_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<PlayerProcessDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypePlayerProcessDetailReflection.Descriptor.MessageTypes[0];

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
	public int MainLevel
	{
		get
		{
			return mainLevel_;
		}
		set
		{
			mainLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int EliteLevel
	{
		get
		{
			return eliteLevel_;
		}
		set
		{
			eliteLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PlayerProcessDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PlayerProcessDetail(PlayerProcessDetail other)
		: this()
	{
		roleBaseInfo_ = ((other.roleBaseInfo_ != null) ? other.roleBaseInfo_.Clone() : null);
		mainLevel_ = other.mainLevel_;
		eliteLevel_ = other.eliteLevel_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PlayerProcessDetail Clone()
	{
		return new PlayerProcessDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as PlayerProcessDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(PlayerProcessDetail other)
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
		if (MainLevel != other.MainLevel)
		{
			return false;
		}
		if (EliteLevel != other.EliteLevel)
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
		if (MainLevel != 0)
		{
			num ^= MainLevel.GetHashCode();
		}
		if (EliteLevel != 0)
		{
			num ^= EliteLevel.GetHashCode();
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
		if (MainLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(MainLevel);
		}
		if (EliteLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(EliteLevel);
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
		if (MainLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MainLevel);
		}
		if (EliteLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(EliteLevel);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(PlayerProcessDetail other)
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
		if (other.MainLevel != 0)
		{
			MainLevel = other.MainLevel;
		}
		if (other.EliteLevel != 0)
		{
			EliteLevel = other.EliteLevel;
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
			case 16u:
				MainLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				EliteLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
