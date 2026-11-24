.class public final synthetic Lbin/mt/signature/KillerApplication193$k;
.super Ljava/lang/Object;


# direct methods
.method public static bridge synthetic a(Landroid/content/pm/PackageInfo;)Landroid/content/pm/SigningInfo;
    .registers 1

    iget-object p0, p0, Landroid/content/pm/PackageInfo;->signingInfo:Landroid/content/pm/SigningInfo;

    return-object p0
.end method

.method public static bridge synthetic b(Landroid/content/pm/SigningInfo;)[Landroid/content/pm/Signature;
    .registers 1

    invoke-virtual {p0}, Landroid/content/pm/SigningInfo;->getApkContentsSigners()[Landroid/content/pm/Signature;

    move-result-object p0

    return-object p0
.end method
