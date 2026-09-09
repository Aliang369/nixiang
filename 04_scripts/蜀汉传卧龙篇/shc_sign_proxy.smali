.class public Lcom/bantu/trgame/ShcSignProxy;
.super Ljava/lang/Object;

.field private static cache:[Landroid/content/pm/Signature;

.method public constructor <init>()V
    .locals 0
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V
    return-void
.end method

.method public getApkContentsSigners()[Landroid/content/pm/Signature;
    .locals 1
    invoke-static {}, Lcom/bantu/trgame/ShcSignProxy;->official()[Landroid/content/pm/Signature;
    move-result-object v0
    return-object v0
.end method

.method public getSigningCertificateHistory()[Landroid/content/pm/Signature;
    .locals 1
    invoke-static {}, Lcom/bantu/trgame/ShcSignProxy;->official()[Landroid/content/pm/Signature;
    move-result-object v0
    return-object v0
.end method

.method public hasMultipleSigners()Z
    .locals 1
    const/4 v0, 0x0
    return v0
.end method

.method private static official()[Landroid/content/pm/Signature;
    .locals 7
    sget-object v0, Lcom/bantu/trgame/ShcSignProxy;->cache:[Landroid/content/pm/Signature;
    if-eqz v0, :cond_build
    return-object v0
    :cond_build
    const-string v0, "30820303308201EBA00302010202045E8B75AE300D06092A864886F70D01010B05003031310B300906035504061302434E3111300F060355040813085368616E67686169310F300D060355040A130642616E7475733020170D3134303931363037313130355A180F32303934303832373037313130355A3031310B300906035504061302434E3111300F060355040813085368616E67686169310F300D060355040A130642616E74757330820122300D06092A864886F70D01010105000382010F003082010A02820101009FA3D6413C7C4D19EAF3EA2A1085F9DC7AFBDFBCD1D7DB2AE950CC5FBBEE7FA9362A47F98CE739DE20AF545925F63AC19C53CBCA88DDE570D1404F2BA739405FEEBDE3E3F7DB3E2ED5F0EAAF4FEC8AD764C7003DA1D0050DD7324AFD0EAFD1F1A5AFB6196D69B6125237786674A29D9496BF41A097C8F6DB529D0313A2FDE149FA8308B411AB8B6A162E4C403B4982F0974E8C873D6E299CF1264196BA6C0913C3B79F9ECFE544A92CC8AB4C04BD87AFF5D64AC002A8BF1DD4E8FB33ED03E8F5B493271E7C5F011A0D67D9F33CA1C82C3D483EE64B3969CF50DE0747CAD17E163A5CB89697A807D76699769119AB365F0E988C23D1BB1DA9CF794376AA6FA6410203010001A321301F301D0603551D0E04160414F2252BF54F03FC8ACD3DFCDD543FF5C559F2D7BC300D06092A864886F70D01010B05000382010100216DC4AF795344EF17FDEDB0F5A0F4CC6D38E04F7482E3C24390497D7C648FE044579A05F69EF8DF4DFA103686078F89C508F9982DCD3E19164C6DA2D2970F525D1CCF093213EA5FCDCA91B95A3B6AF5EE329BF315DC2CEFB27DC606C56D87BAB27BCDCC1F17EEC2082FB5E07180EB9EA5E2942D4DDB97BC9FD1BE6934B06F90380290A9C512D0DEE04CF8B710C2D1DCEC9DBAA0DFA13969A50FCE8A456ADB2FB39FB6F4B44A79214714EA7DEC286D27A0C4BD342D144BE38AACF4DD069AD987B63C861AE8103D37A4DF51AF8F44483E8DA0C977A9D663EA9308050FEFEFBE071A1CF390891F652DAA631883FDDD86AE6CB4238659C5F68EED3BBD50756ED58B"
    invoke-virtual {v0}, Ljava/lang/String;->length()I
    move-result v6
    shr-int/lit8 v1, v6, 0x1
    new-array v1, v1, [B
    const/16 v4, 0x10
    const/4 v2, 0x0
    :loop
    shl-int/lit8 v5, v2, 0x1
    if-ge v5, v6, :done
    invoke-virtual {v0, v5}, Ljava/lang/String;->charAt(I)C
    move-result v3
    invoke-static {v3, v4}, Ljava/lang/Character;->digit(CI)I
    move-result v3
    shl-int/lit8 v3, v3, 0x4
    add-int/lit8 v5, v5, 0x1
    invoke-virtual {v0, v5}, Ljava/lang/String;->charAt(I)C
    move-result v5
    invoke-static {v5, v4}, Ljava/lang/Character;->digit(CI)I
    move-result v5
    or-int/2addr v3, v5
    int-to-byte v3, v3
    aput-byte v3, v1, v2
    add-int/lit8 v2, v2, 0x1
    goto :loop
    :done
    new-instance v3, Landroid/content/pm/Signature;
    invoke-direct {v3, v1}, Landroid/content/pm/Signature;-><init>([B)V
    const/4 v1, 0x1
    new-array v1, v1, [Landroid/content/pm/Signature;
    const/4 v2, 0x0
    aput-object v3, v1, v2
    sput-object v1, Lcom/bantu/trgame/ShcSignProxy;->cache:[Landroid/content/pm/Signature;
    return-object v1
.end method
