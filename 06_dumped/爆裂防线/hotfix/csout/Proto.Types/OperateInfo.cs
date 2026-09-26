using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateInfo : IMessage<OperateInfo>, IMessage, IEquatable<OperateInfo>, IDeepCloneable<OperateInfo>, IBufferMessage
{
	private static readonly MessageParser<OperateInfo> _parser = new MessageParser<OperateInfo>((Func<OperateInfo>)(() => new OperateInfo()));

	private UnknownFieldSet _unknownFields;

	public const int RechargeDailyListFieldNumber = 1;

	private static readonly FieldCodec<RechargeDailyDetail> _repeated_rechargeDailyList_codec = FieldCodec.ForMessage<RechargeDailyDetail>(10u, RechargeDailyDetail.Parser);

	private readonly RepeatedField<RechargeDailyDetail> rechargeDailyList_ = new RepeatedField<RechargeDailyDetail>();

	public const int RechargeDailySumListFieldNumber = 2;

	private static readonly FieldCodec<RechargeSumDetail> _repeated_rechargeDailySumList_codec = FieldCodec.ForMessage<RechargeSumDetail>(18u, RechargeSumDetail.Parser);

	private readonly RepeatedField<RechargeSumDetail> rechargeDailySumList_ = new RepeatedField<RechargeSumDetail>();

	public const int RechargeSumListFieldNumber = 3;

	private static readonly FieldCodec<RechargeSumDetail> _repeated_rechargeSumList_codec = FieldCodec.ForMessage<RechargeSumDetail>(26u, RechargeSumDetail.Parser);

	private readonly RepeatedField<RechargeSumDetail> rechargeSumList_ = new RepeatedField<RechargeSumDetail>();

	public const int RechargeGiftListFieldNumber = 4;

	private static readonly FieldCodec<RechargeGiftDetail> _repeated_rechargeGiftList_codec = FieldCodec.ForMessage<RechargeGiftDetail>(34u, RechargeGiftDetail.Parser);

	private readonly RepeatedField<RechargeGiftDetail> rechargeGiftList_ = new RepeatedField<RechargeGiftDetail>();

	public const int ConsumeDailySumListFieldNumber = 5;

	private static readonly FieldCodec<ConsumeSumDetail> _repeated_consumeDailySumList_codec = FieldCodec.ForMessage<ConsumeSumDetail>(42u, ConsumeSumDetail.Parser);

	private readonly RepeatedField<ConsumeSumDetail> consumeDailySumList_ = new RepeatedField<ConsumeSumDetail>();

	public const int ConsumeSumListFieldNumber = 6;

	private static readonly FieldCodec<ConsumeSumDetail> _repeated_consumeSumList_codec = FieldCodec.ForMessage<ConsumeSumDetail>(50u, ConsumeSumDetail.Parser);

	private readonly RepeatedField<ConsumeSumDetail> consumeSumList_ = new RepeatedField<ConsumeSumDetail>();

	public const int LimitTimeHeroListFieldNumber = 7;

	private static readonly FieldCodec<LimitTimeHeroDetail> _repeated_limitTimeHeroList_codec = FieldCodec.ForMessage<LimitTimeHeroDetail>(58u, LimitTimeHeroDetail.Parser);

	private readonly RepeatedField<LimitTimeHeroDetail> limitTimeHeroList_ = new RepeatedField<LimitTimeHeroDetail>();

	public const int LimitTimeShopListFieldNumber = 8;

	private static readonly FieldCodec<LimitTimeShopDetail> _repeated_limitTimeShopList_codec = FieldCodec.ForMessage<LimitTimeShopDetail>(66u, LimitTimeShopDetail.Parser);

	private readonly RepeatedField<LimitTimeShopDetail> limitTimeShopList_ = new RepeatedField<LimitTimeShopDetail>();

	public const int LimitVipHeroListFieldNumber = 9;

	private static readonly FieldCodec<LimitVipHeroDetail> _repeated_limitVipHeroList_codec = FieldCodec.ForMessage<LimitVipHeroDetail>(74u, LimitVipHeroDetail.Parser);

	private readonly RepeatedField<LimitVipHeroDetail> limitVipHeroList_ = new RepeatedField<LimitVipHeroDetail>();

	public const int LoginSumListFieldNumber = 10;

	private static readonly FieldCodec<LoginSumDetail> _repeated_loginSumList_codec = FieldCodec.ForMessage<LoginSumDetail>(82u, LoginSumDetail.Parser);

	private readonly RepeatedField<LoginSumDetail> loginSumList_ = new RepeatedField<LoginSumDetail>();

	public const int DailyActiveListFieldNumber = 11;

	private static readonly FieldCodec<DailyActiveDetail> _repeated_dailyActiveList_codec = FieldCodec.ForMessage<DailyActiveDetail>(90u, DailyActiveDetail.Parser);

	private readonly RepeatedField<DailyActiveDetail> dailyActiveList_ = new RepeatedField<DailyActiveDetail>();

	public const int BarDrawListFieldNumber = 12;

	private static readonly FieldCodec<BarDrawDetail> _repeated_barDrawList_codec = FieldCodec.ForMessage<BarDrawDetail>(98u, BarDrawDetail.Parser);

	private readonly RepeatedField<BarDrawDetail> barDrawList_ = new RepeatedField<BarDrawDetail>();

	public const int PropExchangeListFieldNumber = 13;

	private static readonly FieldCodec<PropExchangeDetail> _repeated_propExchangeList_codec = FieldCodec.ForMessage<PropExchangeDetail>(106u, PropExchangeDetail.Parser);

	private readonly RepeatedField<PropExchangeDetail> propExchangeList_ = new RepeatedField<PropExchangeDetail>();

	public const int ScratchCardListFieldNumber = 14;

	private static readonly FieldCodec<ScratchCardDetail> _repeated_scratchCardList_codec = FieldCodec.ForMessage<ScratchCardDetail>(114u, ScratchCardDetail.Parser);

	private readonly RepeatedField<ScratchCardDetail> scratchCardList_ = new RepeatedField<ScratchCardDetail>();

	public const int JiZiListFieldNumber = 15;

	private static readonly FieldCodec<JiZiDetail> _repeated_jiZiList_codec = FieldCodec.ForMessage<JiZiDetail>(122u, JiZiDetail.Parser);

	private readonly RepeatedField<JiZiDetail> jiZiList_ = new RepeatedField<JiZiDetail>();

	public const int CipherBoxListFieldNumber = 16;

	private static readonly FieldCodec<CipherBoxDetail> _repeated_cipherBoxList_codec = FieldCodec.ForMessage<CipherBoxDetail>(130u, CipherBoxDetail.Parser);

	private readonly RepeatedField<CipherBoxDetail> cipherBoxList_ = new RepeatedField<CipherBoxDetail>();

	public const int BarrageCmdListFieldNumber = 17;

	private static readonly FieldCodec<BarrageCmdDetail> _repeated_barrageCmdList_codec = FieldCodec.ForMessage<BarrageCmdDetail>(138u, BarrageCmdDetail.Parser);

	private readonly RepeatedField<BarrageCmdDetail> barrageCmdList_ = new RepeatedField<BarrageCmdDetail>();

	public const int LimitTimeCatcherListFieldNumber = 18;

	private static readonly FieldCodec<LimitTimeCatcherDetail> _repeated_limitTimeCatcherList_codec = FieldCodec.ForMessage<LimitTimeCatcherDetail>(146u, LimitTimeCatcherDetail.Parser);

	private readonly RepeatedField<LimitTimeCatcherDetail> limitTimeCatcherList_ = new RepeatedField<LimitTimeCatcherDetail>();

	public const int HolidayTaskListFieldNumber = 19;

	private static readonly FieldCodec<HolidayTaskDetail> _repeated_holidayTaskList_codec = FieldCodec.ForMessage<HolidayTaskDetail>(154u, HolidayTaskDetail.Parser);

	private readonly RepeatedField<HolidayTaskDetail> holidayTaskList_ = new RepeatedField<HolidayTaskDetail>();

	public const int PrayLanternListFieldNumber = 20;

	private static readonly FieldCodec<PrayLanternDetail> _repeated_prayLanternList_codec = FieldCodec.ForMessage<PrayLanternDetail>(162u, PrayLanternDetail.Parser);

	private readonly RepeatedField<PrayLanternDetail> prayLanternList_ = new RepeatedField<PrayLanternDetail>();

	public const int ResourceExchangeListFieldNumber = 21;

	private static readonly FieldCodec<ResourceExchangeDetail> _repeated_resourceExchangeList_codec = FieldCodec.ForMessage<ResourceExchangeDetail>(170u, ResourceExchangeDetail.Parser);

	private readonly RepeatedField<ResourceExchangeDetail> resourceExchangeList_ = new RepeatedField<ResourceExchangeDetail>();

	public const int FusionInfoFieldNumber = 22;

	private FusionInfo fusionInfo_;

	public const int InfiniteGiftListFieldNumber = 23;

	private static readonly FieldCodec<InfiniteGiftDetail> _repeated_infiniteGiftList_codec = FieldCodec.ForMessage<InfiniteGiftDetail>(186u, InfiniteGiftDetail.Parser);

	private readonly RepeatedField<InfiniteGiftDetail> infiniteGiftList_ = new RepeatedField<InfiniteGiftDetail>();

	public const int LimitTimeRouletteListFieldNumber = 24;

	private static readonly FieldCodec<LimitTimeRouletteDetail> _repeated_limitTimeRouletteList_codec = FieldCodec.ForMessage<LimitTimeRouletteDetail>(194u, LimitTimeRouletteDetail.Parser);

	private readonly RepeatedField<LimitTimeRouletteDetail> limitTimeRouletteList_ = new RepeatedField<LimitTimeRouletteDetail>();

	public const int RestaurantListFieldNumber = 25;

	private static readonly FieldCodec<RestaurantDetail> _repeated_restaurantList_codec = FieldCodec.ForMessage<RestaurantDetail>(202u, RestaurantDetail.Parser);

	private readonly RepeatedField<RestaurantDetail> restaurantList_ = new RepeatedField<RestaurantDetail>();

	public const int ThemeTaskListFieldNumber = 26;

	private static readonly FieldCodec<ThemeTaskDetail> _repeated_themeTaskList_codec = FieldCodec.ForMessage<ThemeTaskDetail>(210u, ThemeTaskDetail.Parser);

	private readonly RepeatedField<ThemeTaskDetail> themeTaskList_ = new RepeatedField<ThemeTaskDetail>();

	public const int EndlessMineInfoFieldNumber = 27;

	private EndlessMineDetail endlessMineInfo_;

	public const int SnowVeilListFieldNumber = 28;

	private static readonly FieldCodec<SnowVeilDetail> _repeated_snowVeilList_codec = FieldCodec.ForMessage<SnowVeilDetail>(226u, SnowVeilDetail.Parser);

	private readonly RepeatedField<SnowVeilDetail> snowVeilList_ = new RepeatedField<SnowVeilDetail>();

	public const int LimitTimeTreasureListFieldNumber = 29;

	private static readonly FieldCodec<LimitTimeRouletteDetail> _repeated_limitTimeTreasureList_codec = FieldCodec.ForMessage<LimitTimeRouletteDetail>(234u, LimitTimeRouletteDetail.Parser);

	private readonly RepeatedField<LimitTimeRouletteDetail> limitTimeTreasureList_ = new RepeatedField<LimitTimeRouletteDetail>();

	public const int LimitTimeVendorListFieldNumber = 30;

	private static readonly FieldCodec<LimitTimeRouletteDetail> _repeated_limitTimeVendorList_codec = FieldCodec.ForMessage<LimitTimeRouletteDetail>(242u, LimitTimeRouletteDetail.Parser);

	private readonly RepeatedField<LimitTimeRouletteDetail> limitTimeVendorList_ = new RepeatedField<LimitTimeRouletteDetail>();

	public const int ZhanLingKillListFieldNumber = 31;

	private static readonly FieldCodec<ZhanLingKillDetail> _repeated_zhanLingKillList_codec = FieldCodec.ForMessage<ZhanLingKillDetail>(250u, ZhanLingKillDetail.Parser);

	private readonly RepeatedField<ZhanLingKillDetail> zhanLingKillList_ = new RepeatedField<ZhanLingKillDetail>();

	public const int BossSubjectListFieldNumber = 32;

	private static readonly FieldCodec<BossSubjectDetail> _repeated_bossSubjectList_codec = FieldCodec.ForMessage<BossSubjectDetail>(258u, BossSubjectDetail.Parser);

	private readonly RepeatedField<BossSubjectDetail> bossSubjectList_ = new RepeatedField<BossSubjectDetail>();

	public const int AdsDirectListFieldNumber = 33;

	private static readonly FieldCodec<AdsDirectDetail> _repeated_adsDirectList_codec = FieldCodec.ForMessage<AdsDirectDetail>(266u, AdsDirectDetail.Parser);

	private readonly RepeatedField<AdsDirectDetail> adsDirectList_ = new RepeatedField<AdsDirectDetail>();

	public const int AdsGiftListFieldNumber = 34;

	private static readonly FieldCodec<AdsGiftDetail> _repeated_adsGiftList_codec = FieldCodec.ForMessage<AdsGiftDetail>(274u, AdsGiftDetail.Parser);

	private readonly RepeatedField<AdsGiftDetail> adsGiftList_ = new RepeatedField<AdsGiftDetail>();

	public const int AdsRouletteListFieldNumber = 35;

	private static readonly FieldCodec<AdsRouletteDetail> _repeated_adsRouletteList_codec = FieldCodec.ForMessage<AdsRouletteDetail>(282u, AdsRouletteDetail.Parser);

	private readonly RepeatedField<AdsRouletteDetail> adsRouletteList_ = new RepeatedField<AdsRouletteDetail>();

	public const int AdsSignListFieldNumber = 36;

	private static readonly FieldCodec<AdsSignDetail> _repeated_adsSignList_codec = FieldCodec.ForMessage<AdsSignDetail>(290u, AdsSignDetail.Parser);

	private readonly RepeatedField<AdsSignDetail> adsSignList_ = new RepeatedField<AdsSignDetail>();

	public const int AdsZhanLingListFieldNumber = 37;

	private static readonly FieldCodec<AdsZhanLingDetail> _repeated_adsZhanLingList_codec = FieldCodec.ForMessage<AdsZhanLingDetail>(298u, AdsZhanLingDetail.Parser);

	private readonly RepeatedField<AdsZhanLingDetail> adsZhanLingList_ = new RepeatedField<AdsZhanLingDetail>();

	public const int WallSubjectListFieldNumber = 38;

	private static readonly FieldCodec<WallSubjectDetail> _repeated_wallSubjectList_codec = FieldCodec.ForMessage<WallSubjectDetail>(306u, WallSubjectDetail.Parser);

	private readonly RepeatedField<WallSubjectDetail> wallSubjectList_ = new RepeatedField<WallSubjectDetail>();

	public const int MechaSubjectListFieldNumber = 39;

	private static readonly FieldCodec<MechaSubjectDetail> _repeated_mechaSubjectList_codec = FieldCodec.ForMessage<MechaSubjectDetail>(314u, MechaSubjectDetail.Parser);

	private readonly RepeatedField<MechaSubjectDetail> mechaSubjectList_ = new RepeatedField<MechaSubjectDetail>();

	public const int TacticRouletteListFieldNumber = 40;

	private static readonly FieldCodec<TacticRouletteDetail> _repeated_tacticRouletteList_codec = FieldCodec.ForMessage<TacticRouletteDetail>(322u, TacticRouletteDetail.Parser);

	private readonly RepeatedField<TacticRouletteDetail> tacticRouletteList_ = new RepeatedField<TacticRouletteDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<RechargeDailyDetail> RechargeDailyList => rechargeDailyList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<RechargeSumDetail> RechargeDailySumList => rechargeDailySumList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<RechargeSumDetail> RechargeSumList => rechargeSumList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<RechargeGiftDetail> RechargeGiftList => rechargeGiftList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ConsumeSumDetail> ConsumeDailySumList => consumeDailySumList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ConsumeSumDetail> ConsumeSumList => consumeSumList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LimitTimeHeroDetail> LimitTimeHeroList => limitTimeHeroList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LimitTimeShopDetail> LimitTimeShopList => limitTimeShopList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LimitVipHeroDetail> LimitVipHeroList => limitVipHeroList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LoginSumDetail> LoginSumList => loginSumList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<DailyActiveDetail> DailyActiveList => dailyActiveList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BarDrawDetail> BarDrawList => barDrawList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<PropExchangeDetail> PropExchangeList => propExchangeList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ScratchCardDetail> ScratchCardList => scratchCardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<JiZiDetail> JiZiList => jiZiList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<CipherBoxDetail> CipherBoxList => cipherBoxList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BarrageCmdDetail> BarrageCmdList => barrageCmdList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LimitTimeCatcherDetail> LimitTimeCatcherList => limitTimeCatcherList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<HolidayTaskDetail> HolidayTaskList => holidayTaskList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<PrayLanternDetail> PrayLanternList => prayLanternList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ResourceExchangeDetail> ResourceExchangeList => resourceExchangeList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FusionInfo FusionInfo
	{
		get
		{
			return fusionInfo_;
		}
		set
		{
			fusionInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<InfiniteGiftDetail> InfiniteGiftList => infiniteGiftList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LimitTimeRouletteDetail> LimitTimeRouletteList => limitTimeRouletteList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<RestaurantDetail> RestaurantList => restaurantList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ThemeTaskDetail> ThemeTaskList => themeTaskList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessMineDetail EndlessMineInfo
	{
		get
		{
			return endlessMineInfo_;
		}
		set
		{
			endlessMineInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SnowVeilDetail> SnowVeilList => snowVeilList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LimitTimeRouletteDetail> LimitTimeTreasureList => limitTimeTreasureList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LimitTimeRouletteDetail> LimitTimeVendorList => limitTimeVendorList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ZhanLingKillDetail> ZhanLingKillList => zhanLingKillList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BossSubjectDetail> BossSubjectList => bossSubjectList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AdsDirectDetail> AdsDirectList => adsDirectList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AdsGiftDetail> AdsGiftList => adsGiftList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AdsRouletteDetail> AdsRouletteList => adsRouletteList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AdsSignDetail> AdsSignList => adsSignList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AdsZhanLingDetail> AdsZhanLingList => adsZhanLingList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<WallSubjectDetail> WallSubjectList => wallSubjectList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<MechaSubjectDetail> MechaSubjectList => mechaSubjectList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<TacticRouletteDetail> TacticRouletteList => tacticRouletteList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateInfo(OperateInfo other)
		: this()
	{
		rechargeDailyList_ = other.rechargeDailyList_.Clone();
		rechargeDailySumList_ = other.rechargeDailySumList_.Clone();
		rechargeSumList_ = other.rechargeSumList_.Clone();
		rechargeGiftList_ = other.rechargeGiftList_.Clone();
		consumeDailySumList_ = other.consumeDailySumList_.Clone();
		consumeSumList_ = other.consumeSumList_.Clone();
		limitTimeHeroList_ = other.limitTimeHeroList_.Clone();
		limitTimeShopList_ = other.limitTimeShopList_.Clone();
		limitVipHeroList_ = other.limitVipHeroList_.Clone();
		loginSumList_ = other.loginSumList_.Clone();
		dailyActiveList_ = other.dailyActiveList_.Clone();
		barDrawList_ = other.barDrawList_.Clone();
		propExchangeList_ = other.propExchangeList_.Clone();
		scratchCardList_ = other.scratchCardList_.Clone();
		jiZiList_ = other.jiZiList_.Clone();
		cipherBoxList_ = other.cipherBoxList_.Clone();
		barrageCmdList_ = other.barrageCmdList_.Clone();
		limitTimeCatcherList_ = other.limitTimeCatcherList_.Clone();
		holidayTaskList_ = other.holidayTaskList_.Clone();
		prayLanternList_ = other.prayLanternList_.Clone();
		resourceExchangeList_ = other.resourceExchangeList_.Clone();
		fusionInfo_ = ((other.fusionInfo_ != null) ? other.fusionInfo_.Clone() : null);
		infiniteGiftList_ = other.infiniteGiftList_.Clone();
		limitTimeRouletteList_ = other.limitTimeRouletteList_.Clone();
		restaurantList_ = other.restaurantList_.Clone();
		themeTaskList_ = other.themeTaskList_.Clone();
		endlessMineInfo_ = ((other.endlessMineInfo_ != null) ? other.endlessMineInfo_.Clone() : null);
		snowVeilList_ = other.snowVeilList_.Clone();
		limitTimeTreasureList_ = other.limitTimeTreasureList_.Clone();
		limitTimeVendorList_ = other.limitTimeVendorList_.Clone();
		zhanLingKillList_ = other.zhanLingKillList_.Clone();
		bossSubjectList_ = other.bossSubjectList_.Clone();
		adsDirectList_ = other.adsDirectList_.Clone();
		adsGiftList_ = other.adsGiftList_.Clone();
		adsRouletteList_ = other.adsRouletteList_.Clone();
		adsSignList_ = other.adsSignList_.Clone();
		adsZhanLingList_ = other.adsZhanLingList_.Clone();
		wallSubjectList_ = other.wallSubjectList_.Clone();
		mechaSubjectList_ = other.mechaSubjectList_.Clone();
		tacticRouletteList_ = other.tacticRouletteList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateInfo Clone()
	{
		return new OperateInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!rechargeDailyList_.Equals(other.rechargeDailyList_))
		{
			return false;
		}
		if (!rechargeDailySumList_.Equals(other.rechargeDailySumList_))
		{
			return false;
		}
		if (!rechargeSumList_.Equals(other.rechargeSumList_))
		{
			return false;
		}
		if (!rechargeGiftList_.Equals(other.rechargeGiftList_))
		{
			return false;
		}
		if (!consumeDailySumList_.Equals(other.consumeDailySumList_))
		{
			return false;
		}
		if (!consumeSumList_.Equals(other.consumeSumList_))
		{
			return false;
		}
		if (!limitTimeHeroList_.Equals(other.limitTimeHeroList_))
		{
			return false;
		}
		if (!limitTimeShopList_.Equals(other.limitTimeShopList_))
		{
			return false;
		}
		if (!limitVipHeroList_.Equals(other.limitVipHeroList_))
		{
			return false;
		}
		if (!loginSumList_.Equals(other.loginSumList_))
		{
			return false;
		}
		if (!dailyActiveList_.Equals(other.dailyActiveList_))
		{
			return false;
		}
		if (!barDrawList_.Equals(other.barDrawList_))
		{
			return false;
		}
		if (!propExchangeList_.Equals(other.propExchangeList_))
		{
			return false;
		}
		if (!scratchCardList_.Equals(other.scratchCardList_))
		{
			return false;
		}
		if (!jiZiList_.Equals(other.jiZiList_))
		{
			return false;
		}
		if (!cipherBoxList_.Equals(other.cipherBoxList_))
		{
			return false;
		}
		if (!barrageCmdList_.Equals(other.barrageCmdList_))
		{
			return false;
		}
		if (!limitTimeCatcherList_.Equals(other.limitTimeCatcherList_))
		{
			return false;
		}
		if (!holidayTaskList_.Equals(other.holidayTaskList_))
		{
			return false;
		}
		if (!prayLanternList_.Equals(other.prayLanternList_))
		{
			return false;
		}
		if (!resourceExchangeList_.Equals(other.resourceExchangeList_))
		{
			return false;
		}
		if (!object.Equals(FusionInfo, other.FusionInfo))
		{
			return false;
		}
		if (!infiniteGiftList_.Equals(other.infiniteGiftList_))
		{
			return false;
		}
		if (!limitTimeRouletteList_.Equals(other.limitTimeRouletteList_))
		{
			return false;
		}
		if (!restaurantList_.Equals(other.restaurantList_))
		{
			return false;
		}
		if (!themeTaskList_.Equals(other.themeTaskList_))
		{
			return false;
		}
		if (!object.Equals(EndlessMineInfo, other.EndlessMineInfo))
		{
			return false;
		}
		if (!snowVeilList_.Equals(other.snowVeilList_))
		{
			return false;
		}
		if (!limitTimeTreasureList_.Equals(other.limitTimeTreasureList_))
		{
			return false;
		}
		if (!limitTimeVendorList_.Equals(other.limitTimeVendorList_))
		{
			return false;
		}
		if (!zhanLingKillList_.Equals(other.zhanLingKillList_))
		{
			return false;
		}
		if (!bossSubjectList_.Equals(other.bossSubjectList_))
		{
			return false;
		}
		if (!adsDirectList_.Equals(other.adsDirectList_))
		{
			return false;
		}
		if (!adsGiftList_.Equals(other.adsGiftList_))
		{
			return false;
		}
		if (!adsRouletteList_.Equals(other.adsRouletteList_))
		{
			return false;
		}
		if (!adsSignList_.Equals(other.adsSignList_))
		{
			return false;
		}
		if (!adsZhanLingList_.Equals(other.adsZhanLingList_))
		{
			return false;
		}
		if (!wallSubjectList_.Equals(other.wallSubjectList_))
		{
			return false;
		}
		if (!mechaSubjectList_.Equals(other.mechaSubjectList_))
		{
			return false;
		}
		if (!tacticRouletteList_.Equals(other.tacticRouletteList_))
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
		num ^= ((object)rechargeDailyList_).GetHashCode();
		num ^= ((object)rechargeDailySumList_).GetHashCode();
		num ^= ((object)rechargeSumList_).GetHashCode();
		num ^= ((object)rechargeGiftList_).GetHashCode();
		num ^= ((object)consumeDailySumList_).GetHashCode();
		num ^= ((object)consumeSumList_).GetHashCode();
		num ^= ((object)limitTimeHeroList_).GetHashCode();
		num ^= ((object)limitTimeShopList_).GetHashCode();
		num ^= ((object)limitVipHeroList_).GetHashCode();
		num ^= ((object)loginSumList_).GetHashCode();
		num ^= ((object)dailyActiveList_).GetHashCode();
		num ^= ((object)barDrawList_).GetHashCode();
		num ^= ((object)propExchangeList_).GetHashCode();
		num ^= ((object)scratchCardList_).GetHashCode();
		num ^= ((object)jiZiList_).GetHashCode();
		num ^= ((object)cipherBoxList_).GetHashCode();
		num ^= ((object)barrageCmdList_).GetHashCode();
		num ^= ((object)limitTimeCatcherList_).GetHashCode();
		num ^= ((object)holidayTaskList_).GetHashCode();
		num ^= ((object)prayLanternList_).GetHashCode();
		num ^= ((object)resourceExchangeList_).GetHashCode();
		if (fusionInfo_ != null)
		{
			num ^= FusionInfo.GetHashCode();
		}
		num ^= ((object)infiniteGiftList_).GetHashCode();
		num ^= ((object)limitTimeRouletteList_).GetHashCode();
		num ^= ((object)restaurantList_).GetHashCode();
		num ^= ((object)themeTaskList_).GetHashCode();
		if (endlessMineInfo_ != null)
		{
			num ^= EndlessMineInfo.GetHashCode();
		}
		num ^= ((object)snowVeilList_).GetHashCode();
		num ^= ((object)limitTimeTreasureList_).GetHashCode();
		num ^= ((object)limitTimeVendorList_).GetHashCode();
		num ^= ((object)zhanLingKillList_).GetHashCode();
		num ^= ((object)bossSubjectList_).GetHashCode();
		num ^= ((object)adsDirectList_).GetHashCode();
		num ^= ((object)adsGiftList_).GetHashCode();
		num ^= ((object)adsRouletteList_).GetHashCode();
		num ^= ((object)adsSignList_).GetHashCode();
		num ^= ((object)adsZhanLingList_).GetHashCode();
		num ^= ((object)wallSubjectList_).GetHashCode();
		num ^= ((object)mechaSubjectList_).GetHashCode();
		num ^= ((object)tacticRouletteList_).GetHashCode();
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
		rechargeDailyList_.WriteTo(ref output, _repeated_rechargeDailyList_codec);
		rechargeDailySumList_.WriteTo(ref output, _repeated_rechargeDailySumList_codec);
		rechargeSumList_.WriteTo(ref output, _repeated_rechargeSumList_codec);
		rechargeGiftList_.WriteTo(ref output, _repeated_rechargeGiftList_codec);
		consumeDailySumList_.WriteTo(ref output, _repeated_consumeDailySumList_codec);
		consumeSumList_.WriteTo(ref output, _repeated_consumeSumList_codec);
		limitTimeHeroList_.WriteTo(ref output, _repeated_limitTimeHeroList_codec);
		limitTimeShopList_.WriteTo(ref output, _repeated_limitTimeShopList_codec);
		limitVipHeroList_.WriteTo(ref output, _repeated_limitVipHeroList_codec);
		loginSumList_.WriteTo(ref output, _repeated_loginSumList_codec);
		dailyActiveList_.WriteTo(ref output, _repeated_dailyActiveList_codec);
		barDrawList_.WriteTo(ref output, _repeated_barDrawList_codec);
		propExchangeList_.WriteTo(ref output, _repeated_propExchangeList_codec);
		scratchCardList_.WriteTo(ref output, _repeated_scratchCardList_codec);
		jiZiList_.WriteTo(ref output, _repeated_jiZiList_codec);
		cipherBoxList_.WriteTo(ref output, _repeated_cipherBoxList_codec);
		barrageCmdList_.WriteTo(ref output, _repeated_barrageCmdList_codec);
		limitTimeCatcherList_.WriteTo(ref output, _repeated_limitTimeCatcherList_codec);
		holidayTaskList_.WriteTo(ref output, _repeated_holidayTaskList_codec);
		prayLanternList_.WriteTo(ref output, _repeated_prayLanternList_codec);
		resourceExchangeList_.WriteTo(ref output, _repeated_resourceExchangeList_codec);
		if (fusionInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)178, (byte)1);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FusionInfo);
		}
		infiniteGiftList_.WriteTo(ref output, _repeated_infiniteGiftList_codec);
		limitTimeRouletteList_.WriteTo(ref output, _repeated_limitTimeRouletteList_codec);
		restaurantList_.WriteTo(ref output, _repeated_restaurantList_codec);
		themeTaskList_.WriteTo(ref output, _repeated_themeTaskList_codec);
		if (endlessMineInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)218, (byte)1);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)EndlessMineInfo);
		}
		snowVeilList_.WriteTo(ref output, _repeated_snowVeilList_codec);
		limitTimeTreasureList_.WriteTo(ref output, _repeated_limitTimeTreasureList_codec);
		limitTimeVendorList_.WriteTo(ref output, _repeated_limitTimeVendorList_codec);
		zhanLingKillList_.WriteTo(ref output, _repeated_zhanLingKillList_codec);
		bossSubjectList_.WriteTo(ref output, _repeated_bossSubjectList_codec);
		adsDirectList_.WriteTo(ref output, _repeated_adsDirectList_codec);
		adsGiftList_.WriteTo(ref output, _repeated_adsGiftList_codec);
		adsRouletteList_.WriteTo(ref output, _repeated_adsRouletteList_codec);
		adsSignList_.WriteTo(ref output, _repeated_adsSignList_codec);
		adsZhanLingList_.WriteTo(ref output, _repeated_adsZhanLingList_codec);
		wallSubjectList_.WriteTo(ref output, _repeated_wallSubjectList_codec);
		mechaSubjectList_.WriteTo(ref output, _repeated_mechaSubjectList_codec);
		tacticRouletteList_.WriteTo(ref output, _repeated_tacticRouletteList_codec);
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
		num += rechargeDailyList_.CalculateSize(_repeated_rechargeDailyList_codec);
		num += rechargeDailySumList_.CalculateSize(_repeated_rechargeDailySumList_codec);
		num += rechargeSumList_.CalculateSize(_repeated_rechargeSumList_codec);
		num += rechargeGiftList_.CalculateSize(_repeated_rechargeGiftList_codec);
		num += consumeDailySumList_.CalculateSize(_repeated_consumeDailySumList_codec);
		num += consumeSumList_.CalculateSize(_repeated_consumeSumList_codec);
		num += limitTimeHeroList_.CalculateSize(_repeated_limitTimeHeroList_codec);
		num += limitTimeShopList_.CalculateSize(_repeated_limitTimeShopList_codec);
		num += limitVipHeroList_.CalculateSize(_repeated_limitVipHeroList_codec);
		num += loginSumList_.CalculateSize(_repeated_loginSumList_codec);
		num += dailyActiveList_.CalculateSize(_repeated_dailyActiveList_codec);
		num += barDrawList_.CalculateSize(_repeated_barDrawList_codec);
		num += propExchangeList_.CalculateSize(_repeated_propExchangeList_codec);
		num += scratchCardList_.CalculateSize(_repeated_scratchCardList_codec);
		num += jiZiList_.CalculateSize(_repeated_jiZiList_codec);
		num += cipherBoxList_.CalculateSize(_repeated_cipherBoxList_codec);
		num += barrageCmdList_.CalculateSize(_repeated_barrageCmdList_codec);
		num += limitTimeCatcherList_.CalculateSize(_repeated_limitTimeCatcherList_codec);
		num += holidayTaskList_.CalculateSize(_repeated_holidayTaskList_codec);
		num += prayLanternList_.CalculateSize(_repeated_prayLanternList_codec);
		num += resourceExchangeList_.CalculateSize(_repeated_resourceExchangeList_codec);
		if (fusionInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FusionInfo);
		}
		num += infiniteGiftList_.CalculateSize(_repeated_infiniteGiftList_codec);
		num += limitTimeRouletteList_.CalculateSize(_repeated_limitTimeRouletteList_codec);
		num += restaurantList_.CalculateSize(_repeated_restaurantList_codec);
		num += themeTaskList_.CalculateSize(_repeated_themeTaskList_codec);
		if (endlessMineInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)EndlessMineInfo);
		}
		num += snowVeilList_.CalculateSize(_repeated_snowVeilList_codec);
		num += limitTimeTreasureList_.CalculateSize(_repeated_limitTimeTreasureList_codec);
		num += limitTimeVendorList_.CalculateSize(_repeated_limitTimeVendorList_codec);
		num += zhanLingKillList_.CalculateSize(_repeated_zhanLingKillList_codec);
		num += bossSubjectList_.CalculateSize(_repeated_bossSubjectList_codec);
		num += adsDirectList_.CalculateSize(_repeated_adsDirectList_codec);
		num += adsGiftList_.CalculateSize(_repeated_adsGiftList_codec);
		num += adsRouletteList_.CalculateSize(_repeated_adsRouletteList_codec);
		num += adsSignList_.CalculateSize(_repeated_adsSignList_codec);
		num += adsZhanLingList_.CalculateSize(_repeated_adsZhanLingList_codec);
		num += wallSubjectList_.CalculateSize(_repeated_wallSubjectList_codec);
		num += mechaSubjectList_.CalculateSize(_repeated_mechaSubjectList_codec);
		num += tacticRouletteList_.CalculateSize(_repeated_tacticRouletteList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateInfo other)
	{
		if (other == null)
		{
			return;
		}
		rechargeDailyList_.Add((IEnumerable<RechargeDailyDetail>)other.rechargeDailyList_);
		rechargeDailySumList_.Add((IEnumerable<RechargeSumDetail>)other.rechargeDailySumList_);
		rechargeSumList_.Add((IEnumerable<RechargeSumDetail>)other.rechargeSumList_);
		rechargeGiftList_.Add((IEnumerable<RechargeGiftDetail>)other.rechargeGiftList_);
		consumeDailySumList_.Add((IEnumerable<ConsumeSumDetail>)other.consumeDailySumList_);
		consumeSumList_.Add((IEnumerable<ConsumeSumDetail>)other.consumeSumList_);
		limitTimeHeroList_.Add((IEnumerable<LimitTimeHeroDetail>)other.limitTimeHeroList_);
		limitTimeShopList_.Add((IEnumerable<LimitTimeShopDetail>)other.limitTimeShopList_);
		limitVipHeroList_.Add((IEnumerable<LimitVipHeroDetail>)other.limitVipHeroList_);
		loginSumList_.Add((IEnumerable<LoginSumDetail>)other.loginSumList_);
		dailyActiveList_.Add((IEnumerable<DailyActiveDetail>)other.dailyActiveList_);
		barDrawList_.Add((IEnumerable<BarDrawDetail>)other.barDrawList_);
		propExchangeList_.Add((IEnumerable<PropExchangeDetail>)other.propExchangeList_);
		scratchCardList_.Add((IEnumerable<ScratchCardDetail>)other.scratchCardList_);
		jiZiList_.Add((IEnumerable<JiZiDetail>)other.jiZiList_);
		cipherBoxList_.Add((IEnumerable<CipherBoxDetail>)other.cipherBoxList_);
		barrageCmdList_.Add((IEnumerable<BarrageCmdDetail>)other.barrageCmdList_);
		limitTimeCatcherList_.Add((IEnumerable<LimitTimeCatcherDetail>)other.limitTimeCatcherList_);
		holidayTaskList_.Add((IEnumerable<HolidayTaskDetail>)other.holidayTaskList_);
		prayLanternList_.Add((IEnumerable<PrayLanternDetail>)other.prayLanternList_);
		resourceExchangeList_.Add((IEnumerable<ResourceExchangeDetail>)other.resourceExchangeList_);
		if (other.fusionInfo_ != null)
		{
			if (fusionInfo_ == null)
			{
				FusionInfo = new FusionInfo();
			}
			FusionInfo.MergeFrom(other.FusionInfo);
		}
		infiniteGiftList_.Add((IEnumerable<InfiniteGiftDetail>)other.infiniteGiftList_);
		limitTimeRouletteList_.Add((IEnumerable<LimitTimeRouletteDetail>)other.limitTimeRouletteList_);
		restaurantList_.Add((IEnumerable<RestaurantDetail>)other.restaurantList_);
		themeTaskList_.Add((IEnumerable<ThemeTaskDetail>)other.themeTaskList_);
		if (other.endlessMineInfo_ != null)
		{
			if (endlessMineInfo_ == null)
			{
				EndlessMineInfo = new EndlessMineDetail();
			}
			EndlessMineInfo.MergeFrom(other.EndlessMineInfo);
		}
		snowVeilList_.Add((IEnumerable<SnowVeilDetail>)other.snowVeilList_);
		limitTimeTreasureList_.Add((IEnumerable<LimitTimeRouletteDetail>)other.limitTimeTreasureList_);
		limitTimeVendorList_.Add((IEnumerable<LimitTimeRouletteDetail>)other.limitTimeVendorList_);
		zhanLingKillList_.Add((IEnumerable<ZhanLingKillDetail>)other.zhanLingKillList_);
		bossSubjectList_.Add((IEnumerable<BossSubjectDetail>)other.bossSubjectList_);
		adsDirectList_.Add((IEnumerable<AdsDirectDetail>)other.adsDirectList_);
		adsGiftList_.Add((IEnumerable<AdsGiftDetail>)other.adsGiftList_);
		adsRouletteList_.Add((IEnumerable<AdsRouletteDetail>)other.adsRouletteList_);
		adsSignList_.Add((IEnumerable<AdsSignDetail>)other.adsSignList_);
		adsZhanLingList_.Add((IEnumerable<AdsZhanLingDetail>)other.adsZhanLingList_);
		wallSubjectList_.Add((IEnumerable<WallSubjectDetail>)other.wallSubjectList_);
		mechaSubjectList_.Add((IEnumerable<MechaSubjectDetail>)other.mechaSubjectList_);
		tacticRouletteList_.Add((IEnumerable<TacticRouletteDetail>)other.tacticRouletteList_);
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
				rechargeDailyList_.AddEntriesFrom(ref input, _repeated_rechargeDailyList_codec);
				break;
			case 18u:
				rechargeDailySumList_.AddEntriesFrom(ref input, _repeated_rechargeDailySumList_codec);
				break;
			case 26u:
				rechargeSumList_.AddEntriesFrom(ref input, _repeated_rechargeSumList_codec);
				break;
			case 34u:
				rechargeGiftList_.AddEntriesFrom(ref input, _repeated_rechargeGiftList_codec);
				break;
			case 42u:
				consumeDailySumList_.AddEntriesFrom(ref input, _repeated_consumeDailySumList_codec);
				break;
			case 50u:
				consumeSumList_.AddEntriesFrom(ref input, _repeated_consumeSumList_codec);
				break;
			case 58u:
				limitTimeHeroList_.AddEntriesFrom(ref input, _repeated_limitTimeHeroList_codec);
				break;
			case 66u:
				limitTimeShopList_.AddEntriesFrom(ref input, _repeated_limitTimeShopList_codec);
				break;
			case 74u:
				limitVipHeroList_.AddEntriesFrom(ref input, _repeated_limitVipHeroList_codec);
				break;
			case 82u:
				loginSumList_.AddEntriesFrom(ref input, _repeated_loginSumList_codec);
				break;
			case 90u:
				dailyActiveList_.AddEntriesFrom(ref input, _repeated_dailyActiveList_codec);
				break;
			case 98u:
				barDrawList_.AddEntriesFrom(ref input, _repeated_barDrawList_codec);
				break;
			case 106u:
				propExchangeList_.AddEntriesFrom(ref input, _repeated_propExchangeList_codec);
				break;
			case 114u:
				scratchCardList_.AddEntriesFrom(ref input, _repeated_scratchCardList_codec);
				break;
			case 122u:
				jiZiList_.AddEntriesFrom(ref input, _repeated_jiZiList_codec);
				break;
			case 130u:
				cipherBoxList_.AddEntriesFrom(ref input, _repeated_cipherBoxList_codec);
				break;
			case 138u:
				barrageCmdList_.AddEntriesFrom(ref input, _repeated_barrageCmdList_codec);
				break;
			case 146u:
				limitTimeCatcherList_.AddEntriesFrom(ref input, _repeated_limitTimeCatcherList_codec);
				break;
			case 154u:
				holidayTaskList_.AddEntriesFrom(ref input, _repeated_holidayTaskList_codec);
				break;
			case 162u:
				prayLanternList_.AddEntriesFrom(ref input, _repeated_prayLanternList_codec);
				break;
			case 170u:
				resourceExchangeList_.AddEntriesFrom(ref input, _repeated_resourceExchangeList_codec);
				break;
			case 178u:
				if (fusionInfo_ == null)
				{
					FusionInfo = new FusionInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FusionInfo);
				break;
			case 186u:
				infiniteGiftList_.AddEntriesFrom(ref input, _repeated_infiniteGiftList_codec);
				break;
			case 194u:
				limitTimeRouletteList_.AddEntriesFrom(ref input, _repeated_limitTimeRouletteList_codec);
				break;
			case 202u:
				restaurantList_.AddEntriesFrom(ref input, _repeated_restaurantList_codec);
				break;
			case 210u:
				themeTaskList_.AddEntriesFrom(ref input, _repeated_themeTaskList_codec);
				break;
			case 218u:
				if (endlessMineInfo_ == null)
				{
					EndlessMineInfo = new EndlessMineDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)EndlessMineInfo);
				break;
			case 226u:
				snowVeilList_.AddEntriesFrom(ref input, _repeated_snowVeilList_codec);
				break;
			case 234u:
				limitTimeTreasureList_.AddEntriesFrom(ref input, _repeated_limitTimeTreasureList_codec);
				break;
			case 242u:
				limitTimeVendorList_.AddEntriesFrom(ref input, _repeated_limitTimeVendorList_codec);
				break;
			case 250u:
				zhanLingKillList_.AddEntriesFrom(ref input, _repeated_zhanLingKillList_codec);
				break;
			case 258u:
				bossSubjectList_.AddEntriesFrom(ref input, _repeated_bossSubjectList_codec);
				break;
			case 266u:
				adsDirectList_.AddEntriesFrom(ref input, _repeated_adsDirectList_codec);
				break;
			case 274u:
				adsGiftList_.AddEntriesFrom(ref input, _repeated_adsGiftList_codec);
				break;
			case 282u:
				adsRouletteList_.AddEntriesFrom(ref input, _repeated_adsRouletteList_codec);
				break;
			case 290u:
				adsSignList_.AddEntriesFrom(ref input, _repeated_adsSignList_codec);
				break;
			case 298u:
				adsZhanLingList_.AddEntriesFrom(ref input, _repeated_adsZhanLingList_codec);
				break;
			case 306u:
				wallSubjectList_.AddEntriesFrom(ref input, _repeated_wallSubjectList_codec);
				break;
			case 314u:
				mechaSubjectList_.AddEntriesFrom(ref input, _repeated_mechaSubjectList_codec);
				break;
			case 322u:
				tacticRouletteList_.AddEntriesFrom(ref input, _repeated_tacticRouletteList_codec);
				break;
			}
		}
	}
}
