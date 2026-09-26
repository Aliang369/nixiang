using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FacilitySelfInfo : IMessage<FacilitySelfInfo>, IMessage, IEquatable<FacilitySelfInfo>, IDeepCloneable<FacilitySelfInfo>, IBufferMessage
{
	private static readonly MessageParser<FacilitySelfInfo> _parser = new MessageParser<FacilitySelfInfo>((Func<FacilitySelfInfo>)(() => new FacilitySelfInfo()));

	private UnknownFieldSet _unknownFields;

	public const int CreateFieldNumber = 1;

	private FacilityBaseInfo create_;

	public const int JoinFieldNumber = 2;

	private FacilityBaseInfo join_;

	public const int RollingFieldNumber = 3;

	private FacilityRollingDetail rolling_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FacilitySelfInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFacilitySelfInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityBaseInfo Create
	{
		get
		{
			return create_;
		}
		set
		{
			create_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityBaseInfo Join
	{
		get
		{
			return join_;
		}
		set
		{
			join_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityRollingDetail Rolling
	{
		get
		{
			return rolling_;
		}
		set
		{
			rolling_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilitySelfInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilitySelfInfo(FacilitySelfInfo other)
		: this()
	{
		create_ = ((other.create_ != null) ? other.create_.Clone() : null);
		join_ = ((other.join_ != null) ? other.join_.Clone() : null);
		rolling_ = ((other.rolling_ != null) ? other.rolling_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilitySelfInfo Clone()
	{
		return new FacilitySelfInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FacilitySelfInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FacilitySelfInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Create, other.Create))
		{
			return false;
		}
		if (!object.Equals(Join, other.Join))
		{
			return false;
		}
		if (!object.Equals(Rolling, other.Rolling))
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
		if (create_ != null)
		{
			num ^= Create.GetHashCode();
		}
		if (join_ != null)
		{
			num ^= Join.GetHashCode();
		}
		if (rolling_ != null)
		{
			num ^= Rolling.GetHashCode();
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
		if (create_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Create);
		}
		if (join_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Join);
		}
		if (rolling_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Rolling);
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
		if (create_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Create);
		}
		if (join_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Join);
		}
		if (rolling_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Rolling);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FacilitySelfInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.create_ != null)
		{
			if (create_ == null)
			{
				Create = new FacilityBaseInfo();
			}
			Create.MergeFrom(other.Create);
		}
		if (other.join_ != null)
		{
			if (join_ == null)
			{
				Join = new FacilityBaseInfo();
			}
			Join.MergeFrom(other.Join);
		}
		if (other.rolling_ != null)
		{
			if (rolling_ == null)
			{
				Rolling = new FacilityRollingDetail();
			}
			Rolling.MergeFrom(other.Rolling);
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
				if (create_ == null)
				{
					Create = new FacilityBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Create);
				break;
			case 18u:
				if (join_ == null)
				{
					Join = new FacilityBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Join);
				break;
			case 26u:
				if (rolling_ == null)
				{
					Rolling = new FacilityRollingDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Rolling);
				break;
			}
		}
	}
}
