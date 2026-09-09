.class public Lcom/starry/bootcache/Boot;
.super Ljava/lang/Object;
.source "Boot.java"


# static fields
.field static final CRC_HEX:Ljava/lang/String; = "1bbe1b8d"

.field static final GUID:Ljava/lang/String; = "a573b3a8f4724464c4fa01d9bc9d0827"

.field static final SIZE:I = 0x6da814


# direct methods
.method public constructor <init>()V
    .registers 1

    .line 5
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method

.method public static run(Landroid/content/Context;)V
    .registers 12

    const/4 v0, 0x0

    .line 12
    :try_start_1
    invoke-virtual {p0, v0}, Landroid/content/Context;->getExternalFilesDir(Ljava/lang/String;)Ljava/io/File;

    move-result-object v0

    if-nez v0, :cond_9

    goto/16 :goto_94

    .line 14
    :cond_9
    new-instance v1, Ljava/io/File;

    .line 15
    const-string v2, "yoo/DefaultPackage/CacheRawFiles/a5/a573b3a8f4724464c4fa01d9bc9d0827"

    invoke-direct {v1, v0, v2}, Ljava/io/File;-><init>(Ljava/io/File;Ljava/lang/String;)V

    .line 16
    new-instance v0, Ljava/io/File;

    const-string v2, "__data.rawfile"

    invoke-direct {v0, v1, v2}, Ljava/io/File;-><init>(Ljava/io/File;Ljava/lang/String;)V

    .line 17
    invoke-virtual {v0}, Ljava/io/File;->length()J

    move-result-wide v2

    const-wide/32 v4, 0x6da814

    const/4 v6, 0x0

    cmp-long v7, v2, v4

    if-eqz v7, :cond_4c

    .line 18
    invoke-virtual {v1}, Ljava/io/File;->mkdirs()Z

    .line 19
    invoke-virtual {p0}, Landroid/content/Context;->getAssets()Landroid/content/res/AssetManager;

    move-result-object p0

    const-string v2, "yoo/DefaultPackage/a573b3a8f4724464c4fa01d9bc9d0827.rawfile"

    invoke-virtual {p0, v2}, Landroid/content/res/AssetManager;->open(Ljava/lang/String;)Ljava/io/InputStream;

    move-result-object p0

    .line 20
    new-instance v2, Ljava/io/FileOutputStream;

    invoke-direct {v2, v0}, Ljava/io/FileOutputStream;-><init>(Ljava/io/File;)V

    const/high16 v0, 0x10000

    .line 21
    new-array v0, v0, [B

    .line 23
    :goto_39
    invoke-virtual {p0, v0}, Ljava/io/InputStream;->read([B)I

    move-result v3

    if-lez v3, :cond_43

    invoke-virtual {v2, v0, v6, v3}, Ljava/io/FileOutputStream;->write([BII)V

    goto :goto_39

    .line 24
    :cond_43
    invoke-virtual {v2}, Ljava/io/FileOutputStream;->flush()V

    .line 25
    invoke-virtual {v2}, Ljava/io/FileOutputStream;->close()V

    .line 26
    invoke-virtual {p0}, Ljava/io/InputStream;->close()V

    .line 28
    :cond_4c
    new-instance p0, Ljava/io/File;

    const-string v0, "__info"

    invoke-direct {p0, v1, v0}, Ljava/io/File;-><init>(Ljava/io/File;Ljava/lang/String;)V

    .line 29
    invoke-virtual {p0}, Ljava/io/File;->length()J

    move-result-wide v0

    const-wide/16 v2, 0x12

    cmp-long v7, v0, v2

    if-eqz v7, :cond_94

    const/16 v0, 0x12

    .line 30
    new-array v0, v0, [B

    const/16 v1, 0x8

    .line 31
    aput-byte v1, v0, v6

    const/4 v2, 0x1

    .line 32
    aput-byte v6, v0, v2

    .line 33
    const-string v2, "1bbe1b8d"

    const-string v3, "UTF-8"

    invoke-virtual {v2, v3}, Ljava/lang/String;->getBytes(Ljava/lang/String;)[B

    move-result-object v2

    const/4 v3, 0x2

    .line 34
    invoke-static {v2, v6, v0, v3, v1}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    :goto_74
    if-ge v6, v1, :cond_86

    add-int/lit8 v2, v6, 0xa

    mul-int/lit8 v3, v6, 0x8

    shr-long v7, v4, v3

    const-wide/16 v9, 0xff

    and-long/2addr v7, v9

    long-to-int v3, v7

    int-to-byte v3, v3

    .line 36
    aput-byte v3, v0, v2

    add-int/lit8 v6, v6, 0x1

    goto :goto_74

    .line 37
    :cond_86
    new-instance v1, Ljava/io/FileOutputStream;

    invoke-direct {v1, p0}, Ljava/io/FileOutputStream;-><init>(Ljava/io/File;)V

    .line 38
    invoke-virtual {v1, v0}, Ljava/io/FileOutputStream;->write([B)V

    .line 39
    invoke-virtual {v1}, Ljava/io/FileOutputStream;->flush()V

    .line 40
    invoke-virtual {v1}, Ljava/io/FileOutputStream;->close()V
    :try_end_94
    .catchall {:try_start_1 .. :try_end_94} :catchall_94

    :catchall_94
    :cond_94
    :goto_94
    return-void
.end method
