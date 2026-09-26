using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class GameData : IMessage<GameData>, IMessage, IEquatable<GameData>, IDeepCloneable<GameData>, IBufferMessage
{
	private static readonly MessageParser<GameData> _parser = new MessageParser<GameData>((Func<GameData>)(() => new GameData()));

	private UnknownFieldSet _unknownFields;

	public const int RoleBaseInfoFieldNumber = 1;

	private RoleBaseInfo roleBaseInfo_;

	public const int RoleCommonInfoFieldNumber = 2;

	private RoleCommonInfo roleCommonInfo_;

	public const int RoleTeamInfoFieldNumber = 3;

	private RoleTeamInfo roleTeamInfo_;

	public const int MoneyListFieldNumber = 4;

	private static readonly FieldCodec<ResourceDetail> _repeated_moneyList_codec = FieldCodec.ForMessage<ResourceDetail>(34u, ResourceDetail.Parser);

	private readonly RepeatedField<ResourceDetail> moneyList_ = new RepeatedField<ResourceDetail>();

	public const int PropListFieldNumber = 5;

	private static readonly FieldCodec<PropDetail> _repeated_propList_codec = FieldCodec.ForMessage<PropDetail>(42u, PropDetail.Parser);

	private readonly RepeatedField<PropDetail> propList_ = new RepeatedField<PropDetail>();

	public const int ItemListFieldNumber = 6;

	private static readonly FieldCodec<ResourceDetail> _repeated_itemList_codec = FieldCodec.ForMessage<ResourceDetail>(50u, ResourceDetail.Parser);

	private readonly RepeatedField<ResourceDetail> itemList_ = new RepeatedField<ResourceDetail>();

	public const int EquipListFieldNumber = 7;

	private static readonly FieldCodec<EquipDetail> _repeated_equipList_codec = FieldCodec.ForMessage<EquipDetail>(58u, EquipDetail.Parser);

	private readonly RepeatedField<EquipDetail> equipList_ = new RepeatedField<EquipDetail>();

	public const int HeroListFieldNumber = 8;

	private static readonly FieldCodec<HeroDetail> _repeated_heroList_codec = FieldCodec.ForMessage<HeroDetail>(66u, HeroDetail.Parser);

	private readonly RepeatedField<HeroDetail> heroList_ = new RepeatedField<HeroDetail>();

	public const int HeroFragListFieldNumber = 9;

	private static readonly FieldCodec<ResourceDetail> _repeated_heroFragList_codec = FieldCodec.ForMessage<ResourceDetail>(74u, ResourceDetail.Parser);

	private readonly RepeatedField<ResourceDetail> heroFragList_ = new RepeatedField<ResourceDetail>();

	public const int GemListFieldNumber = 10;

	private static readonly FieldCodec<ResourceDetail> _repeated_gemList_codec = FieldCodec.ForMessage<ResourceDetail>(82u, ResourceDetail.Parser);

	private readonly RepeatedField<ResourceDetail> gemList_ = new RepeatedField<ResourceDetail>();

	public const int BoxListFieldNumber = 11;

	private static readonly FieldCodec<ResourceDetail> _repeated_boxList_codec = FieldCodec.ForMessage<ResourceDetail>(90u, ResourceDetail.Parser);

	private readonly RepeatedField<ResourceDetail> boxList_ = new RepeatedField<ResourceDetail>();

	public const int LockInfoFieldNumber = 12;

	private LockInfo lockInfo_;

	public const int HomelandInfoFieldNumber = 13;

	private HomelandInfo homelandInfo_;

	public const int WarRoomInfoFieldNumber = 14;

	private WarRoomInfo warRoomInfo_;

	public const int DefenseInfoFieldNumber = 15;

	private DefenseInfo defenseInfo_;

	public const int EquipSlotInfoFieldNumber = 16;

	private EquipSlotInfo equipSlotInfo_;

	public const int MainQuestInfoFieldNumber = 17;

	private MainQuestInfo mainQuestInfo_;

	public const int EliteQuestInfoFieldNumber = 18;

	private EliteQuestInfo eliteQuestInfo_;

	public const int HeroTrialInfoFieldNumber = 19;

	private HeroTrialInfo heroTrialInfo_;

	public const int WorldInfoFieldNumber = 20;

	private WorldInfo worldInfo_;

	public const int PeerageInfoFieldNumber = 21;

	private PeerageInfo peerageInfo_;

	public const int DailyTaskInfoFieldNumber = 22;

	private DailyTaskInfo dailyTaskInfo_;

	public const int FriendsInfoFieldNumber = 23;

	private FriendsInfo friendsInfo_;

	public const int LegionSelfInfoFieldNumber = 24;

	private LegionSelfInfo legionSelfInfo_;

	public const int BarInfoFieldNumber = 25;

	private BarInfo barInfo_;

	public const int ShopListFieldNumber = 26;

	private static readonly FieldCodec<ShopDetail> _repeated_shopList_codec = FieldCodec.ForMessage<ShopDetail>(210u, ShopDetail.Parser);

	private readonly RepeatedField<ShopDetail> shopList_ = new RepeatedField<ShopDetail>();

	public const int AuxAwardInfoFieldNumber = 27;

	private AuxAwardInfo auxAwardInfo_;

	public const int ExchangeListFieldNumber = 28;

	private static readonly FieldCodec<MoneyExchangeDetail> _repeated_exchangeList_codec = FieldCodec.ForMessage<MoneyExchangeDetail>(226u, MoneyExchangeDetail.Parser);

	private readonly RepeatedField<MoneyExchangeDetail> exchangeList_ = new RepeatedField<MoneyExchangeDetail>();

	public const int FreeGiftListFieldNumber = 29;

	private static readonly FieldCodec<FreeGiftDetail> _repeated_freeGiftList_codec = FieldCodec.ForMessage<FreeGiftDetail>(234u, FreeGiftDetail.Parser);

	private readonly RepeatedField<FreeGiftDetail> freeGiftList_ = new RepeatedField<FreeGiftDetail>();

	public const int HeadListFieldNumber = 30;

	private static readonly FieldCodec<int> _repeated_headList_codec = FieldCodec.ForInt32(242u);

	private readonly RepeatedField<int> headList_ = new RepeatedField<int>();

	public const int HeadFrameListFieldNumber = 31;

	private static readonly FieldCodec<int> _repeated_headFrameList_codec = FieldCodec.ForInt32(250u);

	private readonly RepeatedField<int> headFrameList_ = new RepeatedField<int>();

	public const int RechargeShopInfoFieldNumber = 32;

	private RechargeShopInfo rechargeShopInfo_;

	public const int RechargeActiveInfoFieldNumber = 33;

	private RechargeActiveInfo rechargeActiveInfo_;

	public const int VipInfoFieldNumber = 34;

	private VipInfo vipInfo_;

	public const int SubscribeInfoFieldNumber = 35;

	private SubscribeInfo subscribeInfo_;

	public const int VoucherShopInfoFieldNumber = 36;

	private VoucherShopInfo voucherShopInfo_;

	public const int ActivityInfoFieldNumber = 37;

	private ActivityInfo activityInfo_;

	public const int OperateInfoFieldNumber = 38;

	private OperateInfo operateInfo_;

	public const int HandbookInfoFieldNumber = 39;

	private HandbookInfo handbookInfo_;

	public const int SuitInfoFieldNumber = 40;

	private PlayerSuitInfo suitInfo_;

	public const int GunInfoFieldNumber = 41;

	private PlayerGunInfo gunInfo_;

	public const int OpenServerInfoFieldNumber = 42;

	private OpenServerInfo openServerInfo_;

	public const int IdleInfoFieldNumber = 43;

	private IdleInfo idleInfo_;

	public const int WorldBossSelfInfoFieldNumber = 44;

	private WorldBossSelfInfo worldBossSelfInfo_;

	public const int ProgressEventListFieldNumber = 45;

	private static readonly FieldCodec<int> _repeated_progressEventList_codec = FieldCodec.ForInt32(362u);

	private readonly RepeatedField<int> progressEventList_ = new RepeatedField<int>();

	public const int ElementTrialInfoFieldNumber = 46;

	private ElementTrialInfo elementTrialInfo_;

	public const int HeroAwakenInfoFieldNumber = 47;

	private HeroAwakenInfo heroAwakenInfo_;

	public const int MechaInfoFieldNumber = 48;

	private MechaInfo mechaInfo_;

	public const int EndlessHordeInfoFieldNumber = 49;

	private EndlessHordeInfo endlessHordeInfo_;

	public const int JourneyInfoFieldNumber = 50;

	private JourneyInfo journeyInfo_;

	public const int WallInfoFieldNumber = 51;

	private WallInfo wallInfo_;

	public const int LegendaryInfoFieldNumber = 52;

	private LegendaryInfo legendaryInfo_;

	public const int HelpInfoFieldNumber = 53;

	private HelpInfo helpInfo_;

	public const int FootholdInfoFieldNumber = 54;

	private FootholdInfo footholdInfo_;

	public const int ArenaInfoFieldNumber = 55;

	private ArenaInfo arenaInfo_;

	public const int ReminderListFieldNumber = 56;

	private static readonly FieldCodec<int> _repeated_reminderList_codec = FieldCodec.ForInt32(450u);

	private readonly RepeatedField<int> reminderList_ = new RepeatedField<int>();

	public const int CrossLadderInfoFieldNumber = 57;

	private CrossLadderInfo crossLadderInfo_;

	public const int GeneListFieldNumber = 58;

	private static readonly FieldCodec<ResourceDetail> _repeated_geneList_codec = FieldCodec.ForMessage<ResourceDetail>(466u, ResourceDetail.Parser);

	private readonly RepeatedField<ResourceDetail> geneList_ = new RepeatedField<ResourceDetail>();

	public const int TreasureInfoFieldNumber = 59;

	private TreasureInfo treasureInfo_;

	public const int MineWarBaseInfoFieldNumber = 60;

	private MineWarBaseInfo mineWarBaseInfo_;

	public const int ServantInfoFieldNumber = 61;

	private ServantInfo servantInfo_;

	public const int IncubateInfoFieldNumber = 62;

	private IncubateInfo incubateInfo_;

	public const int HeroFollowInfoFieldNumber = 63;

	private HeroFollowInfo heroFollowInfo_;

	public const int CrossPeakSelfInfoFieldNumber = 64;

	private CrossPeakSelfInfo crossPeakSelfInfo_;

	public const int TruckSelfInfoFieldNumber = 65;

	private TruckSelfInfo truckSelfInfo_;

	public const int LegionWarBaseInfoFieldNumber = 66;

	private LegionWarBaseInfo legionWarBaseInfo_;

	public const int AdvertisementInfoFieldNumber = 67;

	private AdvertisementInfo advertisementInfo_;

	public const int FacilitySelfInfoFieldNumber = 68;

	private FacilitySelfInfo facilitySelfInfo_;

	public const int WorldLairInfoFieldNumber = 69;

	private WorldLairInfo worldLairInfo_;

	public const int TacticListFieldNumber = 70;

	private static readonly FieldCodec<TacticDetail> _repeated_tacticList_codec = FieldCodec.ForMessage<TacticDetail>(562u, TacticDetail.Parser);

	private readonly RepeatedField<TacticDetail> tacticList_ = new RepeatedField<TacticDetail>();

	public const int TacticFragListFieldNumber = 71;

	private static readonly FieldCodec<ResourceDetail> _repeated_tacticFragList_codec = FieldCodec.ForMessage<ResourceDetail>(570u, ResourceDetail.Parser);

	private readonly RepeatedField<ResourceDetail> tacticFragList_ = new RepeatedField<ResourceDetail>();

	public const int PvpInfoFieldNumber = 72;

	private PvpInfo pvpInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<GameData> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeGameDataReflection.Descriptor.MessageTypes[0];

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
	public RoleCommonInfo RoleCommonInfo
	{
		get
		{
			return roleCommonInfo_;
		}
		set
		{
			roleCommonInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTeamInfo RoleTeamInfo
	{
		get
		{
			return roleTeamInfo_;
		}
		set
		{
			roleTeamInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ResourceDetail> MoneyList => moneyList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<PropDetail> PropList => propList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ResourceDetail> ItemList => itemList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<EquipDetail> EquipList => equipList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<HeroDetail> HeroList => heroList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ResourceDetail> HeroFragList => heroFragList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ResourceDetail> GemList => gemList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ResourceDetail> BoxList => boxList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LockInfo LockInfo
	{
		get
		{
			return lockInfo_;
		}
		set
		{
			lockInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandInfo HomelandInfo
	{
		get
		{
			return homelandInfo_;
		}
		set
		{
			homelandInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WarRoomInfo WarRoomInfo
	{
		get
		{
			return warRoomInfo_;
		}
		set
		{
			warRoomInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DefenseInfo DefenseInfo
	{
		get
		{
			return defenseInfo_;
		}
		set
		{
			defenseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSlotInfo EquipSlotInfo
	{
		get
		{
			return equipSlotInfo_;
		}
		set
		{
			equipSlotInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MainQuestInfo MainQuestInfo
	{
		get
		{
			return mainQuestInfo_;
		}
		set
		{
			mainQuestInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EliteQuestInfo EliteQuestInfo
	{
		get
		{
			return eliteQuestInfo_;
		}
		set
		{
			eliteQuestInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroTrialInfo HeroTrialInfo
	{
		get
		{
			return heroTrialInfo_;
		}
		set
		{
			heroTrialInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldInfo WorldInfo
	{
		get
		{
			return worldInfo_;
		}
		set
		{
			worldInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PeerageInfo PeerageInfo
	{
		get
		{
			return peerageInfo_;
		}
		set
		{
			peerageInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DailyTaskInfo DailyTaskInfo
	{
		get
		{
			return dailyTaskInfo_;
		}
		set
		{
			dailyTaskInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FriendsInfo FriendsInfo
	{
		get
		{
			return friendsInfo_;
		}
		set
		{
			friendsInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSelfInfo LegionSelfInfo
	{
		get
		{
			return legionSelfInfo_;
		}
		set
		{
			legionSelfInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BarInfo BarInfo
	{
		get
		{
			return barInfo_;
		}
		set
		{
			barInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ShopDetail> ShopList => shopList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AuxAwardInfo AuxAwardInfo
	{
		get
		{
			return auxAwardInfo_;
		}
		set
		{
			auxAwardInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<MoneyExchangeDetail> ExchangeList => exchangeList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<FreeGiftDetail> FreeGiftList => freeGiftList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> HeadList => headList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> HeadFrameList => headFrameList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeShopInfo RechargeShopInfo
	{
		get
		{
			return rechargeShopInfo_;
		}
		set
		{
			rechargeShopInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeActiveInfo RechargeActiveInfo
	{
		get
		{
			return rechargeActiveInfo_;
		}
		set
		{
			rechargeActiveInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VipInfo VipInfo
	{
		get
		{
			return vipInfo_;
		}
		set
		{
			vipInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SubscribeInfo SubscribeInfo
	{
		get
		{
			return subscribeInfo_;
		}
		set
		{
			subscribeInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopInfo VoucherShopInfo
	{
		get
		{
			return voucherShopInfo_;
		}
		set
		{
			voucherShopInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ActivityInfo ActivityInfo
	{
		get
		{
			return activityInfo_;
		}
		set
		{
			activityInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateInfo OperateInfo
	{
		get
		{
			return operateInfo_;
		}
		set
		{
			operateInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HandbookInfo HandbookInfo
	{
		get
		{
			return handbookInfo_;
		}
		set
		{
			handbookInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PlayerSuitInfo SuitInfo
	{
		get
		{
			return suitInfo_;
		}
		set
		{
			suitInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PlayerGunInfo GunInfo
	{
		get
		{
			return gunInfo_;
		}
		set
		{
			gunInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OpenServerInfo OpenServerInfo
	{
		get
		{
			return openServerInfo_;
		}
		set
		{
			openServerInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public IdleInfo IdleInfo
	{
		get
		{
			return idleInfo_;
		}
		set
		{
			idleInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossSelfInfo WorldBossSelfInfo
	{
		get
		{
			return worldBossSelfInfo_;
		}
		set
		{
			worldBossSelfInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> ProgressEventList => progressEventList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ElementTrialInfo ElementTrialInfo
	{
		get
		{
			return elementTrialInfo_;
		}
		set
		{
			elementTrialInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroAwakenInfo HeroAwakenInfo
	{
		get
		{
			return heroAwakenInfo_;
		}
		set
		{
			heroAwakenInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MechaInfo MechaInfo
	{
		get
		{
			return mechaInfo_;
		}
		set
		{
			mechaInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessHordeInfo EndlessHordeInfo
	{
		get
		{
			return endlessHordeInfo_;
		}
		set
		{
			endlessHordeInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyInfo JourneyInfo
	{
		get
		{
			return journeyInfo_;
		}
		set
		{
			journeyInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WallInfo WallInfo
	{
		get
		{
			return wallInfo_;
		}
		set
		{
			wallInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegendaryInfo LegendaryInfo
	{
		get
		{
			return legendaryInfo_;
		}
		set
		{
			legendaryInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HelpInfo HelpInfo
	{
		get
		{
			return helpInfo_;
		}
		set
		{
			helpInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FootholdInfo FootholdInfo
	{
		get
		{
			return footholdInfo_;
		}
		set
		{
			footholdInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ArenaInfo ArenaInfo
	{
		get
		{
			return arenaInfo_;
		}
		set
		{
			arenaInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> ReminderList => reminderList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderInfo CrossLadderInfo
	{
		get
		{
			return crossLadderInfo_;
		}
		set
		{
			crossLadderInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ResourceDetail> GeneList => geneList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TreasureInfo TreasureInfo
	{
		get
		{
			return treasureInfo_;
		}
		set
		{
			treasureInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarBaseInfo MineWarBaseInfo
	{
		get
		{
			return mineWarBaseInfo_;
		}
		set
		{
			mineWarBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServantInfo ServantInfo
	{
		get
		{
			return servantInfo_;
		}
		set
		{
			servantInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public IncubateInfo IncubateInfo
	{
		get
		{
			return incubateInfo_;
		}
		set
		{
			incubateInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroFollowInfo HeroFollowInfo
	{
		get
		{
			return heroFollowInfo_;
		}
		set
		{
			heroFollowInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossPeakSelfInfo CrossPeakSelfInfo
	{
		get
		{
			return crossPeakSelfInfo_;
		}
		set
		{
			crossPeakSelfInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TruckSelfInfo TruckSelfInfo
	{
		get
		{
			return truckSelfInfo_;
		}
		set
		{
			truckSelfInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarBaseInfo LegionWarBaseInfo
	{
		get
		{
			return legionWarBaseInfo_;
		}
		set
		{
			legionWarBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdvertisementInfo AdvertisementInfo
	{
		get
		{
			return advertisementInfo_;
		}
		set
		{
			advertisementInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilitySelfInfo FacilitySelfInfo
	{
		get
		{
			return facilitySelfInfo_;
		}
		set
		{
			facilitySelfInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairInfo WorldLairInfo
	{
		get
		{
			return worldLairInfo_;
		}
		set
		{
			worldLairInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<TacticDetail> TacticList => tacticList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ResourceDetail> TacticFragList => tacticFragList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PvpInfo PvpInfo
	{
		get
		{
			return pvpInfo_;
		}
		set
		{
			pvpInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GameData()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GameData(GameData other)
		: this()
	{
		roleBaseInfo_ = ((other.roleBaseInfo_ != null) ? other.roleBaseInfo_.Clone() : null);
		roleCommonInfo_ = ((other.roleCommonInfo_ != null) ? other.roleCommonInfo_.Clone() : null);
		roleTeamInfo_ = ((other.roleTeamInfo_ != null) ? other.roleTeamInfo_.Clone() : null);
		moneyList_ = other.moneyList_.Clone();
		propList_ = other.propList_.Clone();
		itemList_ = other.itemList_.Clone();
		equipList_ = other.equipList_.Clone();
		heroList_ = other.heroList_.Clone();
		heroFragList_ = other.heroFragList_.Clone();
		gemList_ = other.gemList_.Clone();
		boxList_ = other.boxList_.Clone();
		lockInfo_ = ((other.lockInfo_ != null) ? other.lockInfo_.Clone() : null);
		homelandInfo_ = ((other.homelandInfo_ != null) ? other.homelandInfo_.Clone() : null);
		warRoomInfo_ = ((other.warRoomInfo_ != null) ? other.warRoomInfo_.Clone() : null);
		defenseInfo_ = ((other.defenseInfo_ != null) ? other.defenseInfo_.Clone() : null);
		equipSlotInfo_ = ((other.equipSlotInfo_ != null) ? other.equipSlotInfo_.Clone() : null);
		mainQuestInfo_ = ((other.mainQuestInfo_ != null) ? other.mainQuestInfo_.Clone() : null);
		eliteQuestInfo_ = ((other.eliteQuestInfo_ != null) ? other.eliteQuestInfo_.Clone() : null);
		heroTrialInfo_ = ((other.heroTrialInfo_ != null) ? other.heroTrialInfo_.Clone() : null);
		worldInfo_ = ((other.worldInfo_ != null) ? other.worldInfo_.Clone() : null);
		peerageInfo_ = ((other.peerageInfo_ != null) ? other.peerageInfo_.Clone() : null);
		dailyTaskInfo_ = ((other.dailyTaskInfo_ != null) ? other.dailyTaskInfo_.Clone() : null);
		friendsInfo_ = ((other.friendsInfo_ != null) ? other.friendsInfo_.Clone() : null);
		legionSelfInfo_ = ((other.legionSelfInfo_ != null) ? other.legionSelfInfo_.Clone() : null);
		barInfo_ = ((other.barInfo_ != null) ? other.barInfo_.Clone() : null);
		shopList_ = other.shopList_.Clone();
		auxAwardInfo_ = ((other.auxAwardInfo_ != null) ? other.auxAwardInfo_.Clone() : null);
		exchangeList_ = other.exchangeList_.Clone();
		freeGiftList_ = other.freeGiftList_.Clone();
		headList_ = other.headList_.Clone();
		headFrameList_ = other.headFrameList_.Clone();
		rechargeShopInfo_ = ((other.rechargeShopInfo_ != null) ? other.rechargeShopInfo_.Clone() : null);
		rechargeActiveInfo_ = ((other.rechargeActiveInfo_ != null) ? other.rechargeActiveInfo_.Clone() : null);
		vipInfo_ = ((other.vipInfo_ != null) ? other.vipInfo_.Clone() : null);
		subscribeInfo_ = ((other.subscribeInfo_ != null) ? other.subscribeInfo_.Clone() : null);
		voucherShopInfo_ = ((other.voucherShopInfo_ != null) ? other.voucherShopInfo_.Clone() : null);
		activityInfo_ = ((other.activityInfo_ != null) ? other.activityInfo_.Clone() : null);
		operateInfo_ = ((other.operateInfo_ != null) ? other.operateInfo_.Clone() : null);
		handbookInfo_ = ((other.handbookInfo_ != null) ? other.handbookInfo_.Clone() : null);
		suitInfo_ = ((other.suitInfo_ != null) ? other.suitInfo_.Clone() : null);
		gunInfo_ = ((other.gunInfo_ != null) ? other.gunInfo_.Clone() : null);
		openServerInfo_ = ((other.openServerInfo_ != null) ? other.openServerInfo_.Clone() : null);
		idleInfo_ = ((other.idleInfo_ != null) ? other.idleInfo_.Clone() : null);
		worldBossSelfInfo_ = ((other.worldBossSelfInfo_ != null) ? other.worldBossSelfInfo_.Clone() : null);
		progressEventList_ = other.progressEventList_.Clone();
		elementTrialInfo_ = ((other.elementTrialInfo_ != null) ? other.elementTrialInfo_.Clone() : null);
		heroAwakenInfo_ = ((other.heroAwakenInfo_ != null) ? other.heroAwakenInfo_.Clone() : null);
		mechaInfo_ = ((other.mechaInfo_ != null) ? other.mechaInfo_.Clone() : null);
		endlessHordeInfo_ = ((other.endlessHordeInfo_ != null) ? other.endlessHordeInfo_.Clone() : null);
		journeyInfo_ = ((other.journeyInfo_ != null) ? other.journeyInfo_.Clone() : null);
		wallInfo_ = ((other.wallInfo_ != null) ? other.wallInfo_.Clone() : null);
		legendaryInfo_ = ((other.legendaryInfo_ != null) ? other.legendaryInfo_.Clone() : null);
		helpInfo_ = ((other.helpInfo_ != null) ? other.helpInfo_.Clone() : null);
		footholdInfo_ = ((other.footholdInfo_ != null) ? other.footholdInfo_.Clone() : null);
		arenaInfo_ = ((other.arenaInfo_ != null) ? other.arenaInfo_.Clone() : null);
		reminderList_ = other.reminderList_.Clone();
		crossLadderInfo_ = ((other.crossLadderInfo_ != null) ? other.crossLadderInfo_.Clone() : null);
		geneList_ = other.geneList_.Clone();
		treasureInfo_ = ((other.treasureInfo_ != null) ? other.treasureInfo_.Clone() : null);
		mineWarBaseInfo_ = ((other.mineWarBaseInfo_ != null) ? other.mineWarBaseInfo_.Clone() : null);
		servantInfo_ = ((other.servantInfo_ != null) ? other.servantInfo_.Clone() : null);
		incubateInfo_ = ((other.incubateInfo_ != null) ? other.incubateInfo_.Clone() : null);
		heroFollowInfo_ = ((other.heroFollowInfo_ != null) ? other.heroFollowInfo_.Clone() : null);
		crossPeakSelfInfo_ = ((other.crossPeakSelfInfo_ != null) ? other.crossPeakSelfInfo_.Clone() : null);
		truckSelfInfo_ = ((other.truckSelfInfo_ != null) ? other.truckSelfInfo_.Clone() : null);
		legionWarBaseInfo_ = ((other.legionWarBaseInfo_ != null) ? other.legionWarBaseInfo_.Clone() : null);
		advertisementInfo_ = ((other.advertisementInfo_ != null) ? other.advertisementInfo_.Clone() : null);
		facilitySelfInfo_ = ((other.facilitySelfInfo_ != null) ? other.facilitySelfInfo_.Clone() : null);
		worldLairInfo_ = ((other.worldLairInfo_ != null) ? other.worldLairInfo_.Clone() : null);
		tacticList_ = other.tacticList_.Clone();
		tacticFragList_ = other.tacticFragList_.Clone();
		pvpInfo_ = ((other.pvpInfo_ != null) ? other.pvpInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GameData Clone()
	{
		return new GameData(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as GameData);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(GameData other)
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
		if (!object.Equals(RoleCommonInfo, other.RoleCommonInfo))
		{
			return false;
		}
		if (!object.Equals(RoleTeamInfo, other.RoleTeamInfo))
		{
			return false;
		}
		if (!moneyList_.Equals(other.moneyList_))
		{
			return false;
		}
		if (!propList_.Equals(other.propList_))
		{
			return false;
		}
		if (!itemList_.Equals(other.itemList_))
		{
			return false;
		}
		if (!equipList_.Equals(other.equipList_))
		{
			return false;
		}
		if (!heroList_.Equals(other.heroList_))
		{
			return false;
		}
		if (!heroFragList_.Equals(other.heroFragList_))
		{
			return false;
		}
		if (!gemList_.Equals(other.gemList_))
		{
			return false;
		}
		if (!boxList_.Equals(other.boxList_))
		{
			return false;
		}
		if (!object.Equals(LockInfo, other.LockInfo))
		{
			return false;
		}
		if (!object.Equals(HomelandInfo, other.HomelandInfo))
		{
			return false;
		}
		if (!object.Equals(WarRoomInfo, other.WarRoomInfo))
		{
			return false;
		}
		if (!object.Equals(DefenseInfo, other.DefenseInfo))
		{
			return false;
		}
		if (!object.Equals(EquipSlotInfo, other.EquipSlotInfo))
		{
			return false;
		}
		if (!object.Equals(MainQuestInfo, other.MainQuestInfo))
		{
			return false;
		}
		if (!object.Equals(EliteQuestInfo, other.EliteQuestInfo))
		{
			return false;
		}
		if (!object.Equals(HeroTrialInfo, other.HeroTrialInfo))
		{
			return false;
		}
		if (!object.Equals(WorldInfo, other.WorldInfo))
		{
			return false;
		}
		if (!object.Equals(PeerageInfo, other.PeerageInfo))
		{
			return false;
		}
		if (!object.Equals(DailyTaskInfo, other.DailyTaskInfo))
		{
			return false;
		}
		if (!object.Equals(FriendsInfo, other.FriendsInfo))
		{
			return false;
		}
		if (!object.Equals(LegionSelfInfo, other.LegionSelfInfo))
		{
			return false;
		}
		if (!object.Equals(BarInfo, other.BarInfo))
		{
			return false;
		}
		if (!shopList_.Equals(other.shopList_))
		{
			return false;
		}
		if (!object.Equals(AuxAwardInfo, other.AuxAwardInfo))
		{
			return false;
		}
		if (!exchangeList_.Equals(other.exchangeList_))
		{
			return false;
		}
		if (!freeGiftList_.Equals(other.freeGiftList_))
		{
			return false;
		}
		if (!headList_.Equals(other.headList_))
		{
			return false;
		}
		if (!headFrameList_.Equals(other.headFrameList_))
		{
			return false;
		}
		if (!object.Equals(RechargeShopInfo, other.RechargeShopInfo))
		{
			return false;
		}
		if (!object.Equals(RechargeActiveInfo, other.RechargeActiveInfo))
		{
			return false;
		}
		if (!object.Equals(VipInfo, other.VipInfo))
		{
			return false;
		}
		if (!object.Equals(SubscribeInfo, other.SubscribeInfo))
		{
			return false;
		}
		if (!object.Equals(VoucherShopInfo, other.VoucherShopInfo))
		{
			return false;
		}
		if (!object.Equals(ActivityInfo, other.ActivityInfo))
		{
			return false;
		}
		if (!object.Equals(OperateInfo, other.OperateInfo))
		{
			return false;
		}
		if (!object.Equals(HandbookInfo, other.HandbookInfo))
		{
			return false;
		}
		if (!object.Equals(SuitInfo, other.SuitInfo))
		{
			return false;
		}
		if (!object.Equals(GunInfo, other.GunInfo))
		{
			return false;
		}
		if (!object.Equals(OpenServerInfo, other.OpenServerInfo))
		{
			return false;
		}
		if (!object.Equals(IdleInfo, other.IdleInfo))
		{
			return false;
		}
		if (!object.Equals(WorldBossSelfInfo, other.WorldBossSelfInfo))
		{
			return false;
		}
		if (!progressEventList_.Equals(other.progressEventList_))
		{
			return false;
		}
		if (!object.Equals(ElementTrialInfo, other.ElementTrialInfo))
		{
			return false;
		}
		if (!object.Equals(HeroAwakenInfo, other.HeroAwakenInfo))
		{
			return false;
		}
		if (!object.Equals(MechaInfo, other.MechaInfo))
		{
			return false;
		}
		if (!object.Equals(EndlessHordeInfo, other.EndlessHordeInfo))
		{
			return false;
		}
		if (!object.Equals(JourneyInfo, other.JourneyInfo))
		{
			return false;
		}
		if (!object.Equals(WallInfo, other.WallInfo))
		{
			return false;
		}
		if (!object.Equals(LegendaryInfo, other.LegendaryInfo))
		{
			return false;
		}
		if (!object.Equals(HelpInfo, other.HelpInfo))
		{
			return false;
		}
		if (!object.Equals(FootholdInfo, other.FootholdInfo))
		{
			return false;
		}
		if (!object.Equals(ArenaInfo, other.ArenaInfo))
		{
			return false;
		}
		if (!reminderList_.Equals(other.reminderList_))
		{
			return false;
		}
		if (!object.Equals(CrossLadderInfo, other.CrossLadderInfo))
		{
			return false;
		}
		if (!geneList_.Equals(other.geneList_))
		{
			return false;
		}
		if (!object.Equals(TreasureInfo, other.TreasureInfo))
		{
			return false;
		}
		if (!object.Equals(MineWarBaseInfo, other.MineWarBaseInfo))
		{
			return false;
		}
		if (!object.Equals(ServantInfo, other.ServantInfo))
		{
			return false;
		}
		if (!object.Equals(IncubateInfo, other.IncubateInfo))
		{
			return false;
		}
		if (!object.Equals(HeroFollowInfo, other.HeroFollowInfo))
		{
			return false;
		}
		if (!object.Equals(CrossPeakSelfInfo, other.CrossPeakSelfInfo))
		{
			return false;
		}
		if (!object.Equals(TruckSelfInfo, other.TruckSelfInfo))
		{
			return false;
		}
		if (!object.Equals(LegionWarBaseInfo, other.LegionWarBaseInfo))
		{
			return false;
		}
		if (!object.Equals(AdvertisementInfo, other.AdvertisementInfo))
		{
			return false;
		}
		if (!object.Equals(FacilitySelfInfo, other.FacilitySelfInfo))
		{
			return false;
		}
		if (!object.Equals(WorldLairInfo, other.WorldLairInfo))
		{
			return false;
		}
		if (!tacticList_.Equals(other.tacticList_))
		{
			return false;
		}
		if (!tacticFragList_.Equals(other.tacticFragList_))
		{
			return false;
		}
		if (!object.Equals(PvpInfo, other.PvpInfo))
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
		if (roleCommonInfo_ != null)
		{
			num ^= RoleCommonInfo.GetHashCode();
		}
		if (roleTeamInfo_ != null)
		{
			num ^= RoleTeamInfo.GetHashCode();
		}
		num ^= ((object)moneyList_).GetHashCode();
		num ^= ((object)propList_).GetHashCode();
		num ^= ((object)itemList_).GetHashCode();
		num ^= ((object)equipList_).GetHashCode();
		num ^= ((object)heroList_).GetHashCode();
		num ^= ((object)heroFragList_).GetHashCode();
		num ^= ((object)gemList_).GetHashCode();
		num ^= ((object)boxList_).GetHashCode();
		if (lockInfo_ != null)
		{
			num ^= LockInfo.GetHashCode();
		}
		if (homelandInfo_ != null)
		{
			num ^= HomelandInfo.GetHashCode();
		}
		if (warRoomInfo_ != null)
		{
			num ^= WarRoomInfo.GetHashCode();
		}
		if (defenseInfo_ != null)
		{
			num ^= DefenseInfo.GetHashCode();
		}
		if (equipSlotInfo_ != null)
		{
			num ^= EquipSlotInfo.GetHashCode();
		}
		if (mainQuestInfo_ != null)
		{
			num ^= MainQuestInfo.GetHashCode();
		}
		if (eliteQuestInfo_ != null)
		{
			num ^= EliteQuestInfo.GetHashCode();
		}
		if (heroTrialInfo_ != null)
		{
			num ^= HeroTrialInfo.GetHashCode();
		}
		if (worldInfo_ != null)
		{
			num ^= WorldInfo.GetHashCode();
		}
		if (peerageInfo_ != null)
		{
			num ^= PeerageInfo.GetHashCode();
		}
		if (dailyTaskInfo_ != null)
		{
			num ^= DailyTaskInfo.GetHashCode();
		}
		if (friendsInfo_ != null)
		{
			num ^= FriendsInfo.GetHashCode();
		}
		if (legionSelfInfo_ != null)
		{
			num ^= LegionSelfInfo.GetHashCode();
		}
		if (barInfo_ != null)
		{
			num ^= BarInfo.GetHashCode();
		}
		num ^= ((object)shopList_).GetHashCode();
		if (auxAwardInfo_ != null)
		{
			num ^= AuxAwardInfo.GetHashCode();
		}
		num ^= ((object)exchangeList_).GetHashCode();
		num ^= ((object)freeGiftList_).GetHashCode();
		num ^= ((object)headList_).GetHashCode();
		num ^= ((object)headFrameList_).GetHashCode();
		if (rechargeShopInfo_ != null)
		{
			num ^= RechargeShopInfo.GetHashCode();
		}
		if (rechargeActiveInfo_ != null)
		{
			num ^= RechargeActiveInfo.GetHashCode();
		}
		if (vipInfo_ != null)
		{
			num ^= VipInfo.GetHashCode();
		}
		if (subscribeInfo_ != null)
		{
			num ^= SubscribeInfo.GetHashCode();
		}
		if (voucherShopInfo_ != null)
		{
			num ^= VoucherShopInfo.GetHashCode();
		}
		if (activityInfo_ != null)
		{
			num ^= ActivityInfo.GetHashCode();
		}
		if (operateInfo_ != null)
		{
			num ^= OperateInfo.GetHashCode();
		}
		if (handbookInfo_ != null)
		{
			num ^= HandbookInfo.GetHashCode();
		}
		if (suitInfo_ != null)
		{
			num ^= SuitInfo.GetHashCode();
		}
		if (gunInfo_ != null)
		{
			num ^= GunInfo.GetHashCode();
		}
		if (openServerInfo_ != null)
		{
			num ^= OpenServerInfo.GetHashCode();
		}
		if (idleInfo_ != null)
		{
			num ^= IdleInfo.GetHashCode();
		}
		if (worldBossSelfInfo_ != null)
		{
			num ^= WorldBossSelfInfo.GetHashCode();
		}
		num ^= ((object)progressEventList_).GetHashCode();
		if (elementTrialInfo_ != null)
		{
			num ^= ElementTrialInfo.GetHashCode();
		}
		if (heroAwakenInfo_ != null)
		{
			num ^= HeroAwakenInfo.GetHashCode();
		}
		if (mechaInfo_ != null)
		{
			num ^= MechaInfo.GetHashCode();
		}
		if (endlessHordeInfo_ != null)
		{
			num ^= EndlessHordeInfo.GetHashCode();
		}
		if (journeyInfo_ != null)
		{
			num ^= JourneyInfo.GetHashCode();
		}
		if (wallInfo_ != null)
		{
			num ^= WallInfo.GetHashCode();
		}
		if (legendaryInfo_ != null)
		{
			num ^= LegendaryInfo.GetHashCode();
		}
		if (helpInfo_ != null)
		{
			num ^= HelpInfo.GetHashCode();
		}
		if (footholdInfo_ != null)
		{
			num ^= FootholdInfo.GetHashCode();
		}
		if (arenaInfo_ != null)
		{
			num ^= ArenaInfo.GetHashCode();
		}
		num ^= ((object)reminderList_).GetHashCode();
		if (crossLadderInfo_ != null)
		{
			num ^= CrossLadderInfo.GetHashCode();
		}
		num ^= ((object)geneList_).GetHashCode();
		if (treasureInfo_ != null)
		{
			num ^= TreasureInfo.GetHashCode();
		}
		if (mineWarBaseInfo_ != null)
		{
			num ^= MineWarBaseInfo.GetHashCode();
		}
		if (servantInfo_ != null)
		{
			num ^= ServantInfo.GetHashCode();
		}
		if (incubateInfo_ != null)
		{
			num ^= IncubateInfo.GetHashCode();
		}
		if (heroFollowInfo_ != null)
		{
			num ^= HeroFollowInfo.GetHashCode();
		}
		if (crossPeakSelfInfo_ != null)
		{
			num ^= CrossPeakSelfInfo.GetHashCode();
		}
		if (truckSelfInfo_ != null)
		{
			num ^= TruckSelfInfo.GetHashCode();
		}
		if (legionWarBaseInfo_ != null)
		{
			num ^= LegionWarBaseInfo.GetHashCode();
		}
		if (advertisementInfo_ != null)
		{
			num ^= AdvertisementInfo.GetHashCode();
		}
		if (facilitySelfInfo_ != null)
		{
			num ^= FacilitySelfInfo.GetHashCode();
		}
		if (worldLairInfo_ != null)
		{
			num ^= WorldLairInfo.GetHashCode();
		}
		num ^= ((object)tacticList_).GetHashCode();
		num ^= ((object)tacticFragList_).GetHashCode();
		if (pvpInfo_ != null)
		{
			num ^= PvpInfo.GetHashCode();
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
		if (roleCommonInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleCommonInfo);
		}
		if (roleTeamInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleTeamInfo);
		}
		moneyList_.WriteTo(ref output, _repeated_moneyList_codec);
		propList_.WriteTo(ref output, _repeated_propList_codec);
		itemList_.WriteTo(ref output, _repeated_itemList_codec);
		equipList_.WriteTo(ref output, _repeated_equipList_codec);
		heroList_.WriteTo(ref output, _repeated_heroList_codec);
		heroFragList_.WriteTo(ref output, _repeated_heroFragList_codec);
		gemList_.WriteTo(ref output, _repeated_gemList_codec);
		boxList_.WriteTo(ref output, _repeated_boxList_codec);
		if (lockInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)98);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LockInfo);
		}
		if (homelandInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)106);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)HomelandInfo);
		}
		if (warRoomInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)114);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WarRoomInfo);
		}
		if (defenseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)122);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DefenseInfo);
		}
		if (equipSlotInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)130, (byte)1);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)EquipSlotInfo);
		}
		if (mainQuestInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)138, (byte)1);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)MainQuestInfo);
		}
		if (eliteQuestInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)146, (byte)1);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)EliteQuestInfo);
		}
		if (heroTrialInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)154, (byte)1);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)HeroTrialInfo);
		}
		if (worldInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)162, (byte)1);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WorldInfo);
		}
		if (peerageInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)170, (byte)1);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)PeerageInfo);
		}
		if (dailyTaskInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)178, (byte)1);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DailyTaskInfo);
		}
		if (friendsInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)186, (byte)1);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FriendsInfo);
		}
		if (legionSelfInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)194, (byte)1);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionSelfInfo);
		}
		if (barInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)202, (byte)1);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BarInfo);
		}
		shopList_.WriteTo(ref output, _repeated_shopList_codec);
		if (auxAwardInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)218, (byte)1);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)AuxAwardInfo);
		}
		exchangeList_.WriteTo(ref output, _repeated_exchangeList_codec);
		freeGiftList_.WriteTo(ref output, _repeated_freeGiftList_codec);
		headList_.WriteTo(ref output, _repeated_headList_codec);
		headFrameList_.WriteTo(ref output, _repeated_headFrameList_codec);
		if (rechargeShopInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)130, (byte)2);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RechargeShopInfo);
		}
		if (rechargeActiveInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)138, (byte)2);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RechargeActiveInfo);
		}
		if (vipInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)146, (byte)2);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)VipInfo);
		}
		if (subscribeInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)154, (byte)2);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)SubscribeInfo);
		}
		if (voucherShopInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)162, (byte)2);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)VoucherShopInfo);
		}
		if (activityInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)170, (byte)2);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ActivityInfo);
		}
		if (operateInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)178, (byte)2);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)OperateInfo);
		}
		if (handbookInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)186, (byte)2);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)HandbookInfo);
		}
		if (suitInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)194, (byte)2);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)SuitInfo);
		}
		if (gunInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)202, (byte)2);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)GunInfo);
		}
		if (openServerInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)210, (byte)2);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)OpenServerInfo);
		}
		if (idleInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)218, (byte)2);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)IdleInfo);
		}
		if (worldBossSelfInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)226, (byte)2);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WorldBossSelfInfo);
		}
		progressEventList_.WriteTo(ref output, _repeated_progressEventList_codec);
		if (elementTrialInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)242, (byte)2);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ElementTrialInfo);
		}
		if (heroAwakenInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)250, (byte)2);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)HeroAwakenInfo);
		}
		if (mechaInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)130, (byte)3);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)MechaInfo);
		}
		if (endlessHordeInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)138, (byte)3);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)EndlessHordeInfo);
		}
		if (journeyInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)146, (byte)3);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)JourneyInfo);
		}
		if (wallInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)154, (byte)3);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WallInfo);
		}
		if (legendaryInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)162, (byte)3);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegendaryInfo);
		}
		if (helpInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)170, (byte)3);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)HelpInfo);
		}
		if (footholdInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)178, (byte)3);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FootholdInfo);
		}
		if (arenaInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)186, (byte)3);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ArenaInfo);
		}
		reminderList_.WriteTo(ref output, _repeated_reminderList_codec);
		if (crossLadderInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)202, (byte)3);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)CrossLadderInfo);
		}
		geneList_.WriteTo(ref output, _repeated_geneList_codec);
		if (treasureInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)218, (byte)3);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TreasureInfo);
		}
		if (mineWarBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)226, (byte)3);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)MineWarBaseInfo);
		}
		if (servantInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)234, (byte)3);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ServantInfo);
		}
		if (incubateInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)242, (byte)3);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)IncubateInfo);
		}
		if (heroFollowInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)250, (byte)3);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)HeroFollowInfo);
		}
		if (crossPeakSelfInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)130, (byte)4);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)CrossPeakSelfInfo);
		}
		if (truckSelfInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)138, (byte)4);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TruckSelfInfo);
		}
		if (legionWarBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)146, (byte)4);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionWarBaseInfo);
		}
		if (advertisementInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)154, (byte)4);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)AdvertisementInfo);
		}
		if (facilitySelfInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)162, (byte)4);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FacilitySelfInfo);
		}
		if (worldLairInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)170, (byte)4);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WorldLairInfo);
		}
		tacticList_.WriteTo(ref output, _repeated_tacticList_codec);
		tacticFragList_.WriteTo(ref output, _repeated_tacticFragList_codec);
		if (pvpInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)194, (byte)4);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)PvpInfo);
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
		if (roleCommonInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleCommonInfo);
		}
		if (roleTeamInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleTeamInfo);
		}
		num += moneyList_.CalculateSize(_repeated_moneyList_codec);
		num += propList_.CalculateSize(_repeated_propList_codec);
		num += itemList_.CalculateSize(_repeated_itemList_codec);
		num += equipList_.CalculateSize(_repeated_equipList_codec);
		num += heroList_.CalculateSize(_repeated_heroList_codec);
		num += heroFragList_.CalculateSize(_repeated_heroFragList_codec);
		num += gemList_.CalculateSize(_repeated_gemList_codec);
		num += boxList_.CalculateSize(_repeated_boxList_codec);
		if (lockInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LockInfo);
		}
		if (homelandInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)HomelandInfo);
		}
		if (warRoomInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WarRoomInfo);
		}
		if (defenseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DefenseInfo);
		}
		if (equipSlotInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)EquipSlotInfo);
		}
		if (mainQuestInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)MainQuestInfo);
		}
		if (eliteQuestInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)EliteQuestInfo);
		}
		if (heroTrialInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)HeroTrialInfo);
		}
		if (worldInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WorldInfo);
		}
		if (peerageInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)PeerageInfo);
		}
		if (dailyTaskInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DailyTaskInfo);
		}
		if (friendsInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FriendsInfo);
		}
		if (legionSelfInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionSelfInfo);
		}
		if (barInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BarInfo);
		}
		num += shopList_.CalculateSize(_repeated_shopList_codec);
		if (auxAwardInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)AuxAwardInfo);
		}
		num += exchangeList_.CalculateSize(_repeated_exchangeList_codec);
		num += freeGiftList_.CalculateSize(_repeated_freeGiftList_codec);
		num += headList_.CalculateSize(_repeated_headList_codec);
		num += headFrameList_.CalculateSize(_repeated_headFrameList_codec);
		if (rechargeShopInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RechargeShopInfo);
		}
		if (rechargeActiveInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RechargeActiveInfo);
		}
		if (vipInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)VipInfo);
		}
		if (subscribeInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)SubscribeInfo);
		}
		if (voucherShopInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)VoucherShopInfo);
		}
		if (activityInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ActivityInfo);
		}
		if (operateInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)OperateInfo);
		}
		if (handbookInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)HandbookInfo);
		}
		if (suitInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)SuitInfo);
		}
		if (gunInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)GunInfo);
		}
		if (openServerInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)OpenServerInfo);
		}
		if (idleInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)IdleInfo);
		}
		if (worldBossSelfInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WorldBossSelfInfo);
		}
		num += progressEventList_.CalculateSize(_repeated_progressEventList_codec);
		if (elementTrialInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ElementTrialInfo);
		}
		if (heroAwakenInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)HeroAwakenInfo);
		}
		if (mechaInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)MechaInfo);
		}
		if (endlessHordeInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)EndlessHordeInfo);
		}
		if (journeyInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)JourneyInfo);
		}
		if (wallInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WallInfo);
		}
		if (legendaryInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegendaryInfo);
		}
		if (helpInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)HelpInfo);
		}
		if (footholdInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FootholdInfo);
		}
		if (arenaInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ArenaInfo);
		}
		num += reminderList_.CalculateSize(_repeated_reminderList_codec);
		if (crossLadderInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)CrossLadderInfo);
		}
		num += geneList_.CalculateSize(_repeated_geneList_codec);
		if (treasureInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TreasureInfo);
		}
		if (mineWarBaseInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)MineWarBaseInfo);
		}
		if (servantInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ServantInfo);
		}
		if (incubateInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)IncubateInfo);
		}
		if (heroFollowInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)HeroFollowInfo);
		}
		if (crossPeakSelfInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)CrossPeakSelfInfo);
		}
		if (truckSelfInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TruckSelfInfo);
		}
		if (legionWarBaseInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionWarBaseInfo);
		}
		if (advertisementInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)AdvertisementInfo);
		}
		if (facilitySelfInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FacilitySelfInfo);
		}
		if (worldLairInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WorldLairInfo);
		}
		num += tacticList_.CalculateSize(_repeated_tacticList_codec);
		num += tacticFragList_.CalculateSize(_repeated_tacticFragList_codec);
		if (pvpInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize((IMessage)(object)PvpInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(GameData other)
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
		if (other.roleCommonInfo_ != null)
		{
			if (roleCommonInfo_ == null)
			{
				RoleCommonInfo = new RoleCommonInfo();
			}
			RoleCommonInfo.MergeFrom(other.RoleCommonInfo);
		}
		if (other.roleTeamInfo_ != null)
		{
			if (roleTeamInfo_ == null)
			{
				RoleTeamInfo = new RoleTeamInfo();
			}
			RoleTeamInfo.MergeFrom(other.RoleTeamInfo);
		}
		moneyList_.Add((IEnumerable<ResourceDetail>)other.moneyList_);
		propList_.Add((IEnumerable<PropDetail>)other.propList_);
		itemList_.Add((IEnumerable<ResourceDetail>)other.itemList_);
		equipList_.Add((IEnumerable<EquipDetail>)other.equipList_);
		heroList_.Add((IEnumerable<HeroDetail>)other.heroList_);
		heroFragList_.Add((IEnumerable<ResourceDetail>)other.heroFragList_);
		gemList_.Add((IEnumerable<ResourceDetail>)other.gemList_);
		boxList_.Add((IEnumerable<ResourceDetail>)other.boxList_);
		if (other.lockInfo_ != null)
		{
			if (lockInfo_ == null)
			{
				LockInfo = new LockInfo();
			}
			LockInfo.MergeFrom(other.LockInfo);
		}
		if (other.homelandInfo_ != null)
		{
			if (homelandInfo_ == null)
			{
				HomelandInfo = new HomelandInfo();
			}
			HomelandInfo.MergeFrom(other.HomelandInfo);
		}
		if (other.warRoomInfo_ != null)
		{
			if (warRoomInfo_ == null)
			{
				WarRoomInfo = new WarRoomInfo();
			}
			WarRoomInfo.MergeFrom(other.WarRoomInfo);
		}
		if (other.defenseInfo_ != null)
		{
			if (defenseInfo_ == null)
			{
				DefenseInfo = new DefenseInfo();
			}
			DefenseInfo.MergeFrom(other.DefenseInfo);
		}
		if (other.equipSlotInfo_ != null)
		{
			if (equipSlotInfo_ == null)
			{
				EquipSlotInfo = new EquipSlotInfo();
			}
			EquipSlotInfo.MergeFrom(other.EquipSlotInfo);
		}
		if (other.mainQuestInfo_ != null)
		{
			if (mainQuestInfo_ == null)
			{
				MainQuestInfo = new MainQuestInfo();
			}
			MainQuestInfo.MergeFrom(other.MainQuestInfo);
		}
		if (other.eliteQuestInfo_ != null)
		{
			if (eliteQuestInfo_ == null)
			{
				EliteQuestInfo = new EliteQuestInfo();
			}
			EliteQuestInfo.MergeFrom(other.EliteQuestInfo);
		}
		if (other.heroTrialInfo_ != null)
		{
			if (heroTrialInfo_ == null)
			{
				HeroTrialInfo = new HeroTrialInfo();
			}
			HeroTrialInfo.MergeFrom(other.HeroTrialInfo);
		}
		if (other.worldInfo_ != null)
		{
			if (worldInfo_ == null)
			{
				WorldInfo = new WorldInfo();
			}
			WorldInfo.MergeFrom(other.WorldInfo);
		}
		if (other.peerageInfo_ != null)
		{
			if (peerageInfo_ == null)
			{
				PeerageInfo = new PeerageInfo();
			}
			PeerageInfo.MergeFrom(other.PeerageInfo);
		}
		if (other.dailyTaskInfo_ != null)
		{
			if (dailyTaskInfo_ == null)
			{
				DailyTaskInfo = new DailyTaskInfo();
			}
			DailyTaskInfo.MergeFrom(other.DailyTaskInfo);
		}
		if (other.friendsInfo_ != null)
		{
			if (friendsInfo_ == null)
			{
				FriendsInfo = new FriendsInfo();
			}
			FriendsInfo.MergeFrom(other.FriendsInfo);
		}
		if (other.legionSelfInfo_ != null)
		{
			if (legionSelfInfo_ == null)
			{
				LegionSelfInfo = new LegionSelfInfo();
			}
			LegionSelfInfo.MergeFrom(other.LegionSelfInfo);
		}
		if (other.barInfo_ != null)
		{
			if (barInfo_ == null)
			{
				BarInfo = new BarInfo();
			}
			BarInfo.MergeFrom(other.BarInfo);
		}
		shopList_.Add((IEnumerable<ShopDetail>)other.shopList_);
		if (other.auxAwardInfo_ != null)
		{
			if (auxAwardInfo_ == null)
			{
				AuxAwardInfo = new AuxAwardInfo();
			}
			AuxAwardInfo.MergeFrom(other.AuxAwardInfo);
		}
		exchangeList_.Add((IEnumerable<MoneyExchangeDetail>)other.exchangeList_);
		freeGiftList_.Add((IEnumerable<FreeGiftDetail>)other.freeGiftList_);
		headList_.Add((IEnumerable<int>)other.headList_);
		headFrameList_.Add((IEnumerable<int>)other.headFrameList_);
		if (other.rechargeShopInfo_ != null)
		{
			if (rechargeShopInfo_ == null)
			{
				RechargeShopInfo = new RechargeShopInfo();
			}
			RechargeShopInfo.MergeFrom(other.RechargeShopInfo);
		}
		if (other.rechargeActiveInfo_ != null)
		{
			if (rechargeActiveInfo_ == null)
			{
				RechargeActiveInfo = new RechargeActiveInfo();
			}
			RechargeActiveInfo.MergeFrom(other.RechargeActiveInfo);
		}
		if (other.vipInfo_ != null)
		{
			if (vipInfo_ == null)
			{
				VipInfo = new VipInfo();
			}
			VipInfo.MergeFrom(other.VipInfo);
		}
		if (other.subscribeInfo_ != null)
		{
			if (subscribeInfo_ == null)
			{
				SubscribeInfo = new SubscribeInfo();
			}
			SubscribeInfo.MergeFrom(other.SubscribeInfo);
		}
		if (other.voucherShopInfo_ != null)
		{
			if (voucherShopInfo_ == null)
			{
				VoucherShopInfo = new VoucherShopInfo();
			}
			VoucherShopInfo.MergeFrom(other.VoucherShopInfo);
		}
		if (other.activityInfo_ != null)
		{
			if (activityInfo_ == null)
			{
				ActivityInfo = new ActivityInfo();
			}
			ActivityInfo.MergeFrom(other.ActivityInfo);
		}
		if (other.operateInfo_ != null)
		{
			if (operateInfo_ == null)
			{
				OperateInfo = new OperateInfo();
			}
			OperateInfo.MergeFrom(other.OperateInfo);
		}
		if (other.handbookInfo_ != null)
		{
			if (handbookInfo_ == null)
			{
				HandbookInfo = new HandbookInfo();
			}
			HandbookInfo.MergeFrom(other.HandbookInfo);
		}
		if (other.suitInfo_ != null)
		{
			if (suitInfo_ == null)
			{
				SuitInfo = new PlayerSuitInfo();
			}
			SuitInfo.MergeFrom(other.SuitInfo);
		}
		if (other.gunInfo_ != null)
		{
			if (gunInfo_ == null)
			{
				GunInfo = new PlayerGunInfo();
			}
			GunInfo.MergeFrom(other.GunInfo);
		}
		if (other.openServerInfo_ != null)
		{
			if (openServerInfo_ == null)
			{
				OpenServerInfo = new OpenServerInfo();
			}
			OpenServerInfo.MergeFrom(other.OpenServerInfo);
		}
		if (other.idleInfo_ != null)
		{
			if (idleInfo_ == null)
			{
				IdleInfo = new IdleInfo();
			}
			IdleInfo.MergeFrom(other.IdleInfo);
		}
		if (other.worldBossSelfInfo_ != null)
		{
			if (worldBossSelfInfo_ == null)
			{
				WorldBossSelfInfo = new WorldBossSelfInfo();
			}
			WorldBossSelfInfo.MergeFrom(other.WorldBossSelfInfo);
		}
		progressEventList_.Add((IEnumerable<int>)other.progressEventList_);
		if (other.elementTrialInfo_ != null)
		{
			if (elementTrialInfo_ == null)
			{
				ElementTrialInfo = new ElementTrialInfo();
			}
			ElementTrialInfo.MergeFrom(other.ElementTrialInfo);
		}
		if (other.heroAwakenInfo_ != null)
		{
			if (heroAwakenInfo_ == null)
			{
				HeroAwakenInfo = new HeroAwakenInfo();
			}
			HeroAwakenInfo.MergeFrom(other.HeroAwakenInfo);
		}
		if (other.mechaInfo_ != null)
		{
			if (mechaInfo_ == null)
			{
				MechaInfo = new MechaInfo();
			}
			MechaInfo.MergeFrom(other.MechaInfo);
		}
		if (other.endlessHordeInfo_ != null)
		{
			if (endlessHordeInfo_ == null)
			{
				EndlessHordeInfo = new EndlessHordeInfo();
			}
			EndlessHordeInfo.MergeFrom(other.EndlessHordeInfo);
		}
		if (other.journeyInfo_ != null)
		{
			if (journeyInfo_ == null)
			{
				JourneyInfo = new JourneyInfo();
			}
			JourneyInfo.MergeFrom(other.JourneyInfo);
		}
		if (other.wallInfo_ != null)
		{
			if (wallInfo_ == null)
			{
				WallInfo = new WallInfo();
			}
			WallInfo.MergeFrom(other.WallInfo);
		}
		if (other.legendaryInfo_ != null)
		{
			if (legendaryInfo_ == null)
			{
				LegendaryInfo = new LegendaryInfo();
			}
			LegendaryInfo.MergeFrom(other.LegendaryInfo);
		}
		if (other.helpInfo_ != null)
		{
			if (helpInfo_ == null)
			{
				HelpInfo = new HelpInfo();
			}
			HelpInfo.MergeFrom(other.HelpInfo);
		}
		if (other.footholdInfo_ != null)
		{
			if (footholdInfo_ == null)
			{
				FootholdInfo = new FootholdInfo();
			}
			FootholdInfo.MergeFrom(other.FootholdInfo);
		}
		if (other.arenaInfo_ != null)
		{
			if (arenaInfo_ == null)
			{
				ArenaInfo = new ArenaInfo();
			}
			ArenaInfo.MergeFrom(other.ArenaInfo);
		}
		reminderList_.Add((IEnumerable<int>)other.reminderList_);
		if (other.crossLadderInfo_ != null)
		{
			if (crossLadderInfo_ == null)
			{
				CrossLadderInfo = new CrossLadderInfo();
			}
			CrossLadderInfo.MergeFrom(other.CrossLadderInfo);
		}
		geneList_.Add((IEnumerable<ResourceDetail>)other.geneList_);
		if (other.treasureInfo_ != null)
		{
			if (treasureInfo_ == null)
			{
				TreasureInfo = new TreasureInfo();
			}
			TreasureInfo.MergeFrom(other.TreasureInfo);
		}
		if (other.mineWarBaseInfo_ != null)
		{
			if (mineWarBaseInfo_ == null)
			{
				MineWarBaseInfo = new MineWarBaseInfo();
			}
			MineWarBaseInfo.MergeFrom(other.MineWarBaseInfo);
		}
		if (other.servantInfo_ != null)
		{
			if (servantInfo_ == null)
			{
				ServantInfo = new ServantInfo();
			}
			ServantInfo.MergeFrom(other.ServantInfo);
		}
		if (other.incubateInfo_ != null)
		{
			if (incubateInfo_ == null)
			{
				IncubateInfo = new IncubateInfo();
			}
			IncubateInfo.MergeFrom(other.IncubateInfo);
		}
		if (other.heroFollowInfo_ != null)
		{
			if (heroFollowInfo_ == null)
			{
				HeroFollowInfo = new HeroFollowInfo();
			}
			HeroFollowInfo.MergeFrom(other.HeroFollowInfo);
		}
		if (other.crossPeakSelfInfo_ != null)
		{
			if (crossPeakSelfInfo_ == null)
			{
				CrossPeakSelfInfo = new CrossPeakSelfInfo();
			}
			CrossPeakSelfInfo.MergeFrom(other.CrossPeakSelfInfo);
		}
		if (other.truckSelfInfo_ != null)
		{
			if (truckSelfInfo_ == null)
			{
				TruckSelfInfo = new TruckSelfInfo();
			}
			TruckSelfInfo.MergeFrom(other.TruckSelfInfo);
		}
		if (other.legionWarBaseInfo_ != null)
		{
			if (legionWarBaseInfo_ == null)
			{
				LegionWarBaseInfo = new LegionWarBaseInfo();
			}
			LegionWarBaseInfo.MergeFrom(other.LegionWarBaseInfo);
		}
		if (other.advertisementInfo_ != null)
		{
			if (advertisementInfo_ == null)
			{
				AdvertisementInfo = new AdvertisementInfo();
			}
			AdvertisementInfo.MergeFrom(other.AdvertisementInfo);
		}
		if (other.facilitySelfInfo_ != null)
		{
			if (facilitySelfInfo_ == null)
			{
				FacilitySelfInfo = new FacilitySelfInfo();
			}
			FacilitySelfInfo.MergeFrom(other.FacilitySelfInfo);
		}
		if (other.worldLairInfo_ != null)
		{
			if (worldLairInfo_ == null)
			{
				WorldLairInfo = new WorldLairInfo();
			}
			WorldLairInfo.MergeFrom(other.WorldLairInfo);
		}
		tacticList_.Add((IEnumerable<TacticDetail>)other.tacticList_);
		tacticFragList_.Add((IEnumerable<ResourceDetail>)other.tacticFragList_);
		if (other.pvpInfo_ != null)
		{
			if (pvpInfo_ == null)
			{
				PvpInfo = new PvpInfo();
			}
			PvpInfo.MergeFrom(other.PvpInfo);
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
				if (roleCommonInfo_ == null)
				{
					RoleCommonInfo = new RoleCommonInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleCommonInfo);
				break;
			case 26u:
				if (roleTeamInfo_ == null)
				{
					RoleTeamInfo = new RoleTeamInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleTeamInfo);
				break;
			case 34u:
				moneyList_.AddEntriesFrom(ref input, _repeated_moneyList_codec);
				break;
			case 42u:
				propList_.AddEntriesFrom(ref input, _repeated_propList_codec);
				break;
			case 50u:
				itemList_.AddEntriesFrom(ref input, _repeated_itemList_codec);
				break;
			case 58u:
				equipList_.AddEntriesFrom(ref input, _repeated_equipList_codec);
				break;
			case 66u:
				heroList_.AddEntriesFrom(ref input, _repeated_heroList_codec);
				break;
			case 74u:
				heroFragList_.AddEntriesFrom(ref input, _repeated_heroFragList_codec);
				break;
			case 82u:
				gemList_.AddEntriesFrom(ref input, _repeated_gemList_codec);
				break;
			case 90u:
				boxList_.AddEntriesFrom(ref input, _repeated_boxList_codec);
				break;
			case 98u:
				if (lockInfo_ == null)
				{
					LockInfo = new LockInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LockInfo);
				break;
			case 106u:
				if (homelandInfo_ == null)
				{
					HomelandInfo = new HomelandInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)HomelandInfo);
				break;
			case 114u:
				if (warRoomInfo_ == null)
				{
					WarRoomInfo = new WarRoomInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WarRoomInfo);
				break;
			case 122u:
				if (defenseInfo_ == null)
				{
					DefenseInfo = new DefenseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DefenseInfo);
				break;
			case 130u:
				if (equipSlotInfo_ == null)
				{
					EquipSlotInfo = new EquipSlotInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)EquipSlotInfo);
				break;
			case 138u:
				if (mainQuestInfo_ == null)
				{
					MainQuestInfo = new MainQuestInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)MainQuestInfo);
				break;
			case 146u:
				if (eliteQuestInfo_ == null)
				{
					EliteQuestInfo = new EliteQuestInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)EliteQuestInfo);
				break;
			case 154u:
				if (heroTrialInfo_ == null)
				{
					HeroTrialInfo = new HeroTrialInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)HeroTrialInfo);
				break;
			case 162u:
				if (worldInfo_ == null)
				{
					WorldInfo = new WorldInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WorldInfo);
				break;
			case 170u:
				if (peerageInfo_ == null)
				{
					PeerageInfo = new PeerageInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)PeerageInfo);
				break;
			case 178u:
				if (dailyTaskInfo_ == null)
				{
					DailyTaskInfo = new DailyTaskInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DailyTaskInfo);
				break;
			case 186u:
				if (friendsInfo_ == null)
				{
					FriendsInfo = new FriendsInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FriendsInfo);
				break;
			case 194u:
				if (legionSelfInfo_ == null)
				{
					LegionSelfInfo = new LegionSelfInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionSelfInfo);
				break;
			case 202u:
				if (barInfo_ == null)
				{
					BarInfo = new BarInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BarInfo);
				break;
			case 210u:
				shopList_.AddEntriesFrom(ref input, _repeated_shopList_codec);
				break;
			case 218u:
				if (auxAwardInfo_ == null)
				{
					AuxAwardInfo = new AuxAwardInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)AuxAwardInfo);
				break;
			case 226u:
				exchangeList_.AddEntriesFrom(ref input, _repeated_exchangeList_codec);
				break;
			case 234u:
				freeGiftList_.AddEntriesFrom(ref input, _repeated_freeGiftList_codec);
				break;
			case 240u:
			case 242u:
				headList_.AddEntriesFrom(ref input, _repeated_headList_codec);
				break;
			case 248u:
			case 250u:
				headFrameList_.AddEntriesFrom(ref input, _repeated_headFrameList_codec);
				break;
			case 258u:
				if (rechargeShopInfo_ == null)
				{
					RechargeShopInfo = new RechargeShopInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RechargeShopInfo);
				break;
			case 266u:
				if (rechargeActiveInfo_ == null)
				{
					RechargeActiveInfo = new RechargeActiveInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RechargeActiveInfo);
				break;
			case 274u:
				if (vipInfo_ == null)
				{
					VipInfo = new VipInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)VipInfo);
				break;
			case 282u:
				if (subscribeInfo_ == null)
				{
					SubscribeInfo = new SubscribeInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)SubscribeInfo);
				break;
			case 290u:
				if (voucherShopInfo_ == null)
				{
					VoucherShopInfo = new VoucherShopInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)VoucherShopInfo);
				break;
			case 298u:
				if (activityInfo_ == null)
				{
					ActivityInfo = new ActivityInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ActivityInfo);
				break;
			case 306u:
				if (operateInfo_ == null)
				{
					OperateInfo = new OperateInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)OperateInfo);
				break;
			case 314u:
				if (handbookInfo_ == null)
				{
					HandbookInfo = new HandbookInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)HandbookInfo);
				break;
			case 322u:
				if (suitInfo_ == null)
				{
					SuitInfo = new PlayerSuitInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)SuitInfo);
				break;
			case 330u:
				if (gunInfo_ == null)
				{
					GunInfo = new PlayerGunInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)GunInfo);
				break;
			case 338u:
				if (openServerInfo_ == null)
				{
					OpenServerInfo = new OpenServerInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)OpenServerInfo);
				break;
			case 346u:
				if (idleInfo_ == null)
				{
					IdleInfo = new IdleInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)IdleInfo);
				break;
			case 354u:
				if (worldBossSelfInfo_ == null)
				{
					WorldBossSelfInfo = new WorldBossSelfInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WorldBossSelfInfo);
				break;
			case 360u:
			case 362u:
				progressEventList_.AddEntriesFrom(ref input, _repeated_progressEventList_codec);
				break;
			case 370u:
				if (elementTrialInfo_ == null)
				{
					ElementTrialInfo = new ElementTrialInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ElementTrialInfo);
				break;
			case 378u:
				if (heroAwakenInfo_ == null)
				{
					HeroAwakenInfo = new HeroAwakenInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)HeroAwakenInfo);
				break;
			case 386u:
				if (mechaInfo_ == null)
				{
					MechaInfo = new MechaInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)MechaInfo);
				break;
			case 394u:
				if (endlessHordeInfo_ == null)
				{
					EndlessHordeInfo = new EndlessHordeInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)EndlessHordeInfo);
				break;
			case 402u:
				if (journeyInfo_ == null)
				{
					JourneyInfo = new JourneyInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)JourneyInfo);
				break;
			case 410u:
				if (wallInfo_ == null)
				{
					WallInfo = new WallInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WallInfo);
				break;
			case 418u:
				if (legendaryInfo_ == null)
				{
					LegendaryInfo = new LegendaryInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegendaryInfo);
				break;
			case 426u:
				if (helpInfo_ == null)
				{
					HelpInfo = new HelpInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)HelpInfo);
				break;
			case 434u:
				if (footholdInfo_ == null)
				{
					FootholdInfo = new FootholdInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FootholdInfo);
				break;
			case 442u:
				if (arenaInfo_ == null)
				{
					ArenaInfo = new ArenaInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ArenaInfo);
				break;
			case 448u:
			case 450u:
				reminderList_.AddEntriesFrom(ref input, _repeated_reminderList_codec);
				break;
			case 458u:
				if (crossLadderInfo_ == null)
				{
					CrossLadderInfo = new CrossLadderInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)CrossLadderInfo);
				break;
			case 466u:
				geneList_.AddEntriesFrom(ref input, _repeated_geneList_codec);
				break;
			case 474u:
				if (treasureInfo_ == null)
				{
					TreasureInfo = new TreasureInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TreasureInfo);
				break;
			case 482u:
				if (mineWarBaseInfo_ == null)
				{
					MineWarBaseInfo = new MineWarBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)MineWarBaseInfo);
				break;
			case 490u:
				if (servantInfo_ == null)
				{
					ServantInfo = new ServantInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ServantInfo);
				break;
			case 498u:
				if (incubateInfo_ == null)
				{
					IncubateInfo = new IncubateInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)IncubateInfo);
				break;
			case 506u:
				if (heroFollowInfo_ == null)
				{
					HeroFollowInfo = new HeroFollowInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)HeroFollowInfo);
				break;
			case 514u:
				if (crossPeakSelfInfo_ == null)
				{
					CrossPeakSelfInfo = new CrossPeakSelfInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)CrossPeakSelfInfo);
				break;
			case 522u:
				if (truckSelfInfo_ == null)
				{
					TruckSelfInfo = new TruckSelfInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TruckSelfInfo);
				break;
			case 530u:
				if (legionWarBaseInfo_ == null)
				{
					LegionWarBaseInfo = new LegionWarBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionWarBaseInfo);
				break;
			case 538u:
				if (advertisementInfo_ == null)
				{
					AdvertisementInfo = new AdvertisementInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)AdvertisementInfo);
				break;
			case 546u:
				if (facilitySelfInfo_ == null)
				{
					FacilitySelfInfo = new FacilitySelfInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FacilitySelfInfo);
				break;
			case 554u:
				if (worldLairInfo_ == null)
				{
					WorldLairInfo = new WorldLairInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WorldLairInfo);
				break;
			case 562u:
				tacticList_.AddEntriesFrom(ref input, _repeated_tacticList_codec);
				break;
			case 570u:
				tacticFragList_.AddEntriesFrom(ref input, _repeated_tacticFragList_codec);
				break;
			case 578u:
				if (pvpInfo_ == null)
				{
					PvpInfo = new PvpInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)PvpInfo);
				break;
			}
		}
	}
}
