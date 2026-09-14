.class public Lcom/starry/magic/sign/YnttLoginRequest;
.super Lcom/taptap/sdk/LoginRequest;
.source "YnttLoginRequest.java"

# TapSDK 3.30.2 old ActivityHandler route has no loginType switch.  This request
# subtype is installed by TapLoginForceWeb before TapTapActivity.onCreate; its
# getRequestCode() throws, making LoginClient fall back to the official
# WebViewHandler without modifying the official dex.

.field public static final CREATOR:Landroid/os/Parcelable$Creator;

.method static constructor <clinit>()V
    .registers 1

    new-instance v0, Lcom/starry/magic/sign/YnttLoginRequest$1;

    invoke-direct {v0}, Lcom/starry/magic/sign/YnttLoginRequest$1;-><init>()V

    sput-object v0, Lcom/starry/magic/sign/YnttLoginRequest;->CREATOR:Landroid/os/Parcelable$Creator;

    return-void
.end method

.method public constructor <init>(Ljava/lang/String;[Ljava/lang/String;)V
    .registers 3

    invoke-direct {p0, p1, p2}, Lcom/taptap/sdk/LoginRequest;-><init>(Ljava/lang/String;[Ljava/lang/String;)V

    return-void
.end method

.method public constructor <init>(Landroid/os/Parcel;)V
    .registers 2

    invoke-direct {p0, p1}, Lcom/taptap/sdk/LoginRequest;-><init>(Landroid/os/Parcel;)V

    return-void
.end method

.method public getRequestCode()I
    .registers 2

    new-instance v0, Ljava/lang/IllegalStateException;

    const-string v1, "starry_force_web_login"

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0
.end method
