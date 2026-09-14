.class final Lcom/starry/magic/sign/YnttLoginRequest$1;
.super Ljava/lang/Object;
.source "YnttLoginRequest.java"

.implements Landroid/os/Parcelable$Creator;

.method constructor <init>()V
    .registers 1

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method

.method public createFromParcel(Landroid/os/Parcel;)Lcom/starry/magic/sign/YnttLoginRequest;
    .registers 2

    new-instance v0, Lcom/starry/magic/sign/YnttLoginRequest;

    invoke-direct {v0, p1}, Lcom/starry/magic/sign/YnttLoginRequest;-><init>(Landroid/os/Parcel;)V

    return-object v0
.end method

.method public bridge synthetic createFromParcel(Landroid/os/Parcel;)Ljava/lang/Object;
    .registers 2

    invoke-virtual {p0, p1}, Lcom/starry/magic/sign/YnttLoginRequest$1;->createFromParcel(Landroid/os/Parcel;)Lcom/starry/magic/sign/YnttLoginRequest;

    move-result-object p1

    return-object p1
.end method

.method public newArray(I)[Lcom/starry/magic/sign/YnttLoginRequest;
    .registers 2

    new-array v0, p1, [Lcom/starry/magic/sign/YnttLoginRequest;

    return-object v0
.end method

.method public bridge synthetic newArray(I)[Ljava/lang/Object;
    .registers 2

    invoke-virtual {p0, p1}, Lcom/starry/magic/sign/YnttLoginRequest$1;->newArray(I)[Lcom/starry/magic/sign/YnttLoginRequest;

    move-result-object p1

    return-object p1
.end method
