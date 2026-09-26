using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorkerStyleInfo : IMessage<WorkerStyleInfo>, IMessage, IEquatable<WorkerStyleInfo>, IDeepCloneable<WorkerStyleInfo>, IBufferMessage
{
	private static readonly MessageParser<WorkerStyleInfo> _parser = new MessageParser<WorkerStyleInfo>((Func<WorkerStyleInfo>)(() => new WorkerStyleInfo()));

	private UnknownFieldSet _unknownFields;

	public const int GenderFieldNumber = 1;

	private EGender gender_;

	public const int BodyColorFieldNumber = 2;

	private static readonly FieldCodec<int> _repeated_bodyColor_codec = FieldCodec.ForInt32(18u);

	private readonly RepeatedField<int> bodyColor_ = new RepeatedField<int>();

	public const int HairColorFieldNumber = 3;

	private static readonly FieldCodec<int> _repeated_hairColor_codec = FieldCodec.ForInt32(26u);

	private readonly RepeatedField<int> hairColor_ = new RepeatedField<int>();

	public const int EyeColorFieldNumber = 4;

	private static readonly FieldCodec<int> _repeated_eyeColor_codec = FieldCodec.ForInt32(34u);

	private readonly RepeatedField<int> eyeColor_ = new RepeatedField<int>();

	public const int JobFieldNumber = 5;

	private int job_;

	public const int DescFieldNumber = 6;

	private int desc_;

	public const int NameFieldNumber = 7;

	private string name_ = "";

	public const int FaceFieldNumber = 8;

	private static readonly FieldCodec<int> _repeated_face_codec = FieldCodec.ForInt32(66u);

	private readonly RepeatedField<int> face_ = new RepeatedField<int>();

	public const int SuitFieldNumber = 9;

	private static readonly FieldCodec<int> _repeated_suit_codec = FieldCodec.ForInt32(74u);

	private readonly RepeatedField<int> suit_ = new RepeatedField<int>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorkerStyleInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorkerStyleInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EGender Gender
	{
		get
		{
			return gender_;
		}
		set
		{
			gender_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> BodyColor => bodyColor_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> HairColor => hairColor_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> EyeColor => eyeColor_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Job
	{
		get
		{
			return job_;
		}
		set
		{
			job_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Desc
	{
		get
		{
			return desc_;
		}
		set
		{
			desc_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Name
	{
		get
		{
			return name_;
		}
		set
		{
			name_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> Face => face_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> Suit => suit_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorkerStyleInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorkerStyleInfo(WorkerStyleInfo other)
		: this()
	{
		gender_ = other.gender_;
		bodyColor_ = other.bodyColor_.Clone();
		hairColor_ = other.hairColor_.Clone();
		eyeColor_ = other.eyeColor_.Clone();
		job_ = other.job_;
		desc_ = other.desc_;
		name_ = other.name_;
		face_ = other.face_.Clone();
		suit_ = other.suit_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorkerStyleInfo Clone()
	{
		return new WorkerStyleInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorkerStyleInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorkerStyleInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Gender != other.Gender)
		{
			return false;
		}
		if (!bodyColor_.Equals(other.bodyColor_))
		{
			return false;
		}
		if (!hairColor_.Equals(other.hairColor_))
		{
			return false;
		}
		if (!eyeColor_.Equals(other.eyeColor_))
		{
			return false;
		}
		if (Job != other.Job)
		{
			return false;
		}
		if (Desc != other.Desc)
		{
			return false;
		}
		if (Name != other.Name)
		{
			return false;
		}
		if (!face_.Equals(other.face_))
		{
			return false;
		}
		if (!suit_.Equals(other.suit_))
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
		if (Gender != EGender.EmptyValue)
		{
			num ^= Gender.GetHashCode();
		}
		num ^= ((object)bodyColor_).GetHashCode();
		num ^= ((object)hairColor_).GetHashCode();
		num ^= ((object)eyeColor_).GetHashCode();
		if (Job != 0)
		{
			num ^= Job.GetHashCode();
		}
		if (Desc != 0)
		{
			num ^= Desc.GetHashCode();
		}
		if (Name.Length != 0)
		{
			num ^= Name.GetHashCode();
		}
		num ^= ((object)face_).GetHashCode();
		num ^= ((object)suit_).GetHashCode();
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
		if (Gender != EGender.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)Gender);
		}
		bodyColor_.WriteTo(ref output, _repeated_bodyColor_codec);
		hairColor_.WriteTo(ref output, _repeated_hairColor_codec);
		eyeColor_.WriteTo(ref output, _repeated_eyeColor_codec);
		if (Job != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(Job);
		}
		if (Desc != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(Desc);
		}
		if (Name.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)58);
			((WriteContext)(ref output)).WriteString(Name);
		}
		face_.WriteTo(ref output, _repeated_face_codec);
		suit_.WriteTo(ref output, _repeated_suit_codec);
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
		if (Gender != EGender.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Gender);
		}
		num += bodyColor_.CalculateSize(_repeated_bodyColor_codec);
		num += hairColor_.CalculateSize(_repeated_hairColor_codec);
		num += eyeColor_.CalculateSize(_repeated_eyeColor_codec);
		if (Job != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Job);
		}
		if (Desc != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Desc);
		}
		if (Name.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Name);
		}
		num += face_.CalculateSize(_repeated_face_codec);
		num += suit_.CalculateSize(_repeated_suit_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorkerStyleInfo other)
	{
		if (other != null)
		{
			if (other.Gender != EGender.EmptyValue)
			{
				Gender = other.Gender;
			}
			bodyColor_.Add((IEnumerable<int>)other.bodyColor_);
			hairColor_.Add((IEnumerable<int>)other.hairColor_);
			eyeColor_.Add((IEnumerable<int>)other.eyeColor_);
			if (other.Job != 0)
			{
				Job = other.Job;
			}
			if (other.Desc != 0)
			{
				Desc = other.Desc;
			}
			if (other.Name.Length != 0)
			{
				Name = other.Name;
			}
			face_.Add((IEnumerable<int>)other.face_);
			suit_.Add((IEnumerable<int>)other.suit_);
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
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
			case 8u:
				Gender = (EGender)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
			case 18u:
				bodyColor_.AddEntriesFrom(ref input, _repeated_bodyColor_codec);
				break;
			case 24u:
			case 26u:
				hairColor_.AddEntriesFrom(ref input, _repeated_hairColor_codec);
				break;
			case 32u:
			case 34u:
				eyeColor_.AddEntriesFrom(ref input, _repeated_eyeColor_codec);
				break;
			case 40u:
				Job = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				Desc = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 58u:
				Name = ((ParseContext)(ref input)).ReadString();
				break;
			case 64u:
			case 66u:
				face_.AddEntriesFrom(ref input, _repeated_face_codec);
				break;
			case 72u:
			case 74u:
				suit_.AddEntriesFrom(ref input, _repeated_suit_codec);
				break;
			}
		}
	}
}
