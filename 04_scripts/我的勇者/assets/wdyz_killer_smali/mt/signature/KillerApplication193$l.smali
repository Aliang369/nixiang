.class public final Lbin/mt/signature/KillerApplication193$l;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/os/Parcelable$Creator;


# instance fields
.field public final synthetic a:Landroid/os/Parcelable$Creator;

.field public final synthetic b:Ljava/lang/String;

.field public final synthetic c:Landroid/content/pm/Signature;


# direct methods
.method public constructor <init>(Landroid/os/Parcelable$Creator;Ljava/lang/String;Landroid/content/pm/Signature;)V
    .registers 4

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput-object p1, p0, Lbin/mt/signature/KillerApplication193$l;->a:Landroid/os/Parcelable$Creator;

    iput-object p2, p0, Lbin/mt/signature/KillerApplication193$l;->b:Ljava/lang/String;

    iput-object p3, p0, Lbin/mt/signature/KillerApplication193$l;->c:Landroid/content/pm/Signature;

    return-void
.end method


# virtual methods
.method public final createFromParcel(Landroid/os/Parcel;)Ljava/lang/Object;
    .registers 6

    iget-object v0, p0, Lbin/mt/signature/KillerApplication193$l;->a:Landroid/os/Parcelable$Creator;

    invoke-interface {v0, p1}, Landroid/os/Parcelable$Creator;->createFromParcel(Landroid/os/Parcel;)Ljava/lang/Object;

    move-result-object p1

    check-cast p1, Landroid/content/pm/PackageInfo;

    iget-object v0, p1, Landroid/content/pm/PackageInfo;->packageName:Ljava/lang/String;

    iget-object v1, p0, Lbin/mt/signature/KillerApplication193$l;->b:Ljava/lang/String;

    invoke-virtual {v0, v1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_39

    iget-object v0, p1, Landroid/content/pm/PackageInfo;->signatures:[Landroid/content/pm/Signature;

    iget-object v1, p0, Lbin/mt/signature/KillerApplication193$l;->c:Landroid/content/pm/Signature;

    const/4 v2, 0x0

    if-eqz v0, :cond_1e

    array-length v3, v0

    if-lez v3, :cond_1e

    aput-object v1, v0, v2

    :cond_1e
    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v3, 0x1c

    if-lt v0, v3, :cond_39

    invoke-static {p1}, Lbin/mt/signature/KillerApplication193$k;->a(Landroid/content/pm/PackageInfo;)Landroid/content/pm/SigningInfo;

    move-result-object v0

    if-eqz v0, :cond_39

    invoke-static {p1}, Lbin/mt/signature/KillerApplication193$k;->a(Landroid/content/pm/PackageInfo;)Landroid/content/pm/SigningInfo;

    move-result-object v0

    invoke-static {v0}, Lbin/mt/signature/KillerApplication193$k;->b(Landroid/content/pm/SigningInfo;)[Landroid/content/pm/Signature;

    move-result-object v0

    if-eqz v0, :cond_39

    array-length v3, v0

    if-lez v3, :cond_39

    aput-object v1, v0, v2

    :cond_39
    return-object p1
.end method

.method public final newArray(I)[Ljava/lang/Object;
    .registers 3

    iget-object v0, p0, Lbin/mt/signature/KillerApplication193$l;->a:Landroid/os/Parcelable$Creator;

    invoke-interface {v0, p1}, Landroid/os/Parcelable$Creator;->newArray(I)[Ljava/lang/Object;

    move-result-object p1

    check-cast p1, [Landroid/content/pm/PackageInfo;

    return-object p1
.end method
