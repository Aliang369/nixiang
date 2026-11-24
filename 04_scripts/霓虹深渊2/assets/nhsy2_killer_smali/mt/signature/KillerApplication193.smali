.class public Lbin/mt/signature/KillerApplication193;
.super Landroid/app/Application;


# static fields
.field public static final URL:Ljava/lang/String; = "https://github.com/L-JINBIN/ApkSignatureKillerEx"


# direct methods
.method static constructor <clinit>()V
    .registers 4

    const-string v0, "@@CERT_B64_WRAPPED@@"   # 构建时由 build_killer_dex_zzbx2.sh 从官方底包 v2 签名块提取并替换

    const-string v1, "@@PKG@@"

    invoke-static {v1, v0}, Lbin/mt/signature/KillerApplication193;->killPM(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, ""

    const-string v2, ""

    const-string v3, ""

    invoke-static {v1, v3, v0, v2}, Lbin/mt/signature/KillerApplication193;->killOpen(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    return-void
.end method

.method public constructor <init>()V
    .registers 1

    invoke-direct {p0}, Landroid/app/Application;-><init>()V

    return-void
.end method

.method public static a(Ljava/lang/Class;Ljava/lang/String;)Ljava/lang/reflect/Field;
    .registers 5

    const/4 v0, 0x1

    :try_start_1
    invoke-virtual {p0, p1}, Ljava/lang/Class;->getDeclaredField(Ljava/lang/String;)Ljava/lang/reflect/Field;

    move-result-object v1

    invoke-virtual {v1, v0}, Ljava/lang/reflect/AccessibleObject;->setAccessible(Z)V
    :try_end_8
    .catch Ljava/lang/NoSuchFieldException; {:try_start_1 .. :try_end_8} :catch_9

    return-object v1

    :catch_9
    move-exception v1

    :goto_a
    invoke-virtual {p0}, Ljava/lang/Class;->getSuperclass()Ljava/lang/Class;

    move-result-object p0

    if-eqz p0, :cond_22

    const-class v2, Ljava/lang/Object;

    invoke-virtual {p0, v2}, Ljava/lang/Object;->equals(Ljava/lang/Object;)Z

    move-result v2

    if-nez v2, :cond_22

    :try_start_18
    invoke-virtual {p0, p1}, Ljava/lang/Class;->getDeclaredField(Ljava/lang/String;)Ljava/lang/reflect/Field;

    move-result-object v2

    invoke-virtual {v2, v0}, Ljava/lang/reflect/AccessibleObject;->setAccessible(Z)V
    :try_end_1f
    .catch Ljava/lang/NoSuchFieldException; {:try_start_18 .. :try_end_1f} :catch_20

    return-object v2

    :catch_20
    nop

    goto :goto_a

    :cond_22
    throw v1
.end method

.method public static b(Ljava/lang/String;Ljava/lang/String;)Z
    .registers 14

    const-string v0, "/"

    invoke-virtual {p1, v0}, Ljava/lang/String;->startsWith(Ljava/lang/String;)Z

    move-result v1

    const/4 v2, 0x0

    if-eqz v1, :cond_bb

    const-string v1, ".apk"

    invoke-virtual {p1, v1}, Ljava/lang/String;->endsWith(Ljava/lang/String;)Z

    move-result v1

    if-nez v1, :cond_13

    goto/16 :goto_bb

    :cond_13
    const/4 v1, 0x1

    invoke-virtual {p1, v1}, Ljava/lang/String;->substring(I)Ljava/lang/String;

    move-result-object p1

    const/4 v3, 0x6

    invoke-virtual {p1, v0, v3}, Ljava/lang/String;->split(Ljava/lang/String;I)[Ljava/lang/String;

    move-result-object p1

    array-length v0, p1

    const-string v4, "base.apk"

    const-string v5, "mnt"

    const-string v6, "data"

    const/4 v7, 0x4

    const/4 v8, 0x2

    const-string v9, "app"

    if-eq v0, v7, :cond_73

    const/4 v10, 0x5

    if-ne v0, v10, :cond_2e

    goto :goto_73

    :cond_2e
    const/4 v11, 0x3

    if-ne v0, v11, :cond_48

    aget-object v0, p1, v2

    invoke-virtual {v0, v6}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_bb

    aget-object v0, p1, v1

    invoke-virtual {v0, v9}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_bb

    aget-object p1, p1, v8

    invoke-virtual {p1, p0}, Ljava/lang/String;->startsWith(Ljava/lang/String;)Z

    move-result p0

    return p0

    :cond_48
    if-ne v0, v3, :cond_bb

    aget-object v0, p1, v2

    invoke-virtual {v0, v5}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_bb

    aget-object v0, p1, v1

    const-string v1, "expand"

    invoke-virtual {v0, v1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_bb

    aget-object v0, p1, v11

    invoke-virtual {v0, v9}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_bb

    aget-object v0, p1, v10

    invoke-virtual {v0, v4}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_bb

    aget-object p1, p1, v7

    invoke-virtual {p1, p0}, Ljava/lang/String;->endsWith(Ljava/lang/String;)Z

    move-result p0

    return p0

    :cond_73
    :goto_73
    aget-object v3, p1, v2

    invoke-virtual {v3, v6}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v3

    if-eqz v3, :cond_95

    aget-object v3, p1, v1

    invoke-virtual {v3, v9}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v3

    if-eqz v3, :cond_95

    add-int/lit8 v3, v0, -0x1

    aget-object v3, p1, v3

    invoke-virtual {v3, v4}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v3

    if-eqz v3, :cond_95

    sub-int/2addr v0, v8

    aget-object p1, p1, v0

    invoke-virtual {p1, p0}, Ljava/lang/String;->startsWith(Ljava/lang/String;)Z

    move-result p0

    return p0

    :cond_95
    aget-object v3, p1, v2

    invoke-virtual {v3, v5}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v3

    if-eqz v3, :cond_bb

    aget-object v1, p1, v1

    const-string v3, "asec"

    invoke-virtual {v1, v3}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v1

    if-eqz v1, :cond_bb

    add-int/lit8 v1, v0, -0x1

    aget-object v1, p1, v1

    const-string v3, "pkg.apk"

    invoke-virtual {v1, v3}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v1

    if-eqz v1, :cond_bb

    sub-int/2addr v0, v8

    aget-object p1, p1, v0

    invoke-virtual {p1, p0}, Ljava/lang/String;->startsWith(Ljava/lang/String;)Z

    move-result p0

    return p0

    :cond_bb
    :goto_bb
    return v2
.end method

.method private static killOpen(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V
    .registers 12

    const-string v0, "mt.signature.killer.path2"

    const-string v1, "mt.signature.killer.path1"

    invoke-virtual {p1}, Ljava/lang/String;->isEmpty()Z

    move-result v2

    if-nez v2, :cond_15c

    invoke-virtual {p2}, Ljava/lang/String;->isEmpty()Z

    move-result v2

    if-nez v2, :cond_15c

    invoke-virtual {p3}, Ljava/lang/String;->isEmpty()Z

    move-result v2

    if-eqz v2, :cond_18

    goto/16 :goto_15c

    :cond_18
    :try_start_18
    new-instance v2, Ljava/io/BufferedReader;

    new-instance v3, Ljava/io/FileReader;

    const-string v4, "/proc/self/maps"

    invoke-direct {v3, v4}, Ljava/io/FileReader;-><init>(Ljava/lang/String;)V

    invoke-direct {v2, v3}, Ljava/io/BufferedReader;-><init>(Ljava/io/Reader;)V
    :try_end_24
    .catch Ljava/lang/Exception; {:try_start_18 .. :try_end_24} :catch_3f

    :cond_24
    :try_start_24
    invoke-virtual {v2}, Ljava/io/BufferedReader;->readLine()Ljava/lang/String;

    move-result-object v3

    if-eqz v3, :cond_45

    const-string v4, "\\s+"

    invoke-virtual {v3, v4}, Ljava/lang/String;->split(Ljava/lang/String;)[Ljava/lang/String;

    move-result-object v3

    array-length v4, v3

    add-int/lit8 v4, v4, -0x1

    aget-object v3, v3, v4

    invoke-static {p0, v3}, Lbin/mt/signature/KillerApplication193;->b(Ljava/lang/String;Ljava/lang/String;)Z

    move-result v4
    :try_end_39
    .catchall {:try_start_24 .. :try_end_39} :catchall_42

    if-eqz v4, :cond_24

    :try_start_3b
    invoke-virtual {v2}, Ljava/io/BufferedReader;->close()V

    goto :goto_49

    :catch_3f
    move-exception p0

    goto/16 :goto_156

    :catchall_42
    move-exception p0

    goto/16 :goto_14d

    :cond_45
    invoke-virtual {v2}, Ljava/io/BufferedReader;->close()V
    :try_end_48
    .catch Ljava/lang/Exception; {:try_start_3b .. :try_end_48} :catch_3f

    const/4 v3, 0x0

    :goto_49
    if-nez v3, :cond_53

    sget-object p0, Ljava/lang/System;->err:Ljava/io/PrintStream;

    const-string p1, "Get apk path failed"

    invoke-virtual {p0, p1}, Ljava/io/PrintStream;->println(Ljava/lang/String;)V

    return-void

    :cond_53
    new-instance v2, Ljava/io/File;

    invoke-direct {v2, v3}, Ljava/io/File;-><init>(Ljava/lang/String;)V

    new-instance v3, Ljava/io/File;

    invoke-static {}, Landroid/os/Environment;->getExternalStorageDirectory()Ljava/io/File;

    move-result-object v4

    invoke-virtual {v4}, Ljava/io/File;->getName()Ljava/lang/String;

    move-result-object v4

    const-string v5, "\\d+"

    invoke-virtual {v4, v5}, Ljava/lang/String;->matches(Ljava/lang/String;)Z

    move-result v5

    if-eqz v5, :cond_8c

    new-instance v5, Ljava/io/File;

    new-instance v6, Ljava/lang/StringBuilder;

    const-string v7, "/data/user/"

    invoke-direct {v6, v7}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v6, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    const-string v4, "/"

    invoke-virtual {v6, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    invoke-virtual {v6, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    invoke-virtual {v6}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-direct {v5, v4}, Ljava/io/File;-><init>(Ljava/lang/String;)V

    invoke-virtual {v5}, Ljava/io/File;->canWrite()Z

    move-result v4

    if-eqz v4, :cond_8c

    goto :goto_9f

    :cond_8c
    new-instance v5, Ljava/io/File;

    new-instance v4, Ljava/lang/StringBuilder;

    const-string v6, "/data/data/"

    invoke-direct {v4, v6}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v4, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    invoke-virtual {v4}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object p0

    invoke-direct {v5, p0}, Ljava/io/File;-><init>(Ljava/lang/String;)V

    :goto_9f
    invoke-direct {v3, v5, p3}, Ljava/io/File;-><init>(Ljava/io/File;Ljava/lang/String;)V

    invoke-virtual {v3}, Ljava/io/File;->getParentFile()Ljava/io/File;

    move-result-object p0

    invoke-virtual {p0}, Ljava/io/File;->mkdirs()Z

    :try_start_a9
    new-instance p0, Ljava/util/zip/ZipFile;

    invoke-direct {p0, v2}, Ljava/util/zip/ZipFile;-><init>(Ljava/io/File;)V
    :try_end_ae
    .catch Ljava/io/IOException; {:try_start_a9 .. :try_end_ae} :catch_10f

    :try_start_ae
    invoke-virtual {p0, p2}, Ljava/util/zip/ZipFile;->getEntry(Ljava/lang/String;)Ljava/util/zip/ZipEntry;

    move-result-object p3

    if-eqz p3, :cond_127

    invoke-virtual {v3}, Ljava/io/File;->exists()Z

    move-result p2

    if-eqz p2, :cond_ca

    invoke-virtual {v3}, Ljava/io/File;->length()J

    move-result-wide v4

    invoke-virtual {p3}, Ljava/util/zip/ZipEntry;->getSize()J

    move-result-wide v6

    cmp-long p2, v4, v6

    if-eqz p2, :cond_ec

    goto :goto_ca

    :catchall_c7
    move-exception p1

    goto/16 :goto_13e

    :cond_ca
    :goto_ca
    invoke-virtual {p0, p3}, Ljava/util/zip/ZipFile;->getInputStream(Ljava/util/zip/ZipEntry;)Ljava/io/InputStream;

    move-result-object p2
    :try_end_ce
    .catchall {:try_start_ae .. :try_end_ce} :catchall_c7

    :try_start_ce
    new-instance p3, Ljava/io/FileOutputStream;

    invoke-direct {p3, v3}, Ljava/io/FileOutputStream;-><init>(Ljava/io/File;)V
    :try_end_d3
    .catchall {:try_start_ce .. :try_end_d3} :catchall_111

    const v4, 0x19000

    :try_start_d6
    new-array v4, v4, [B

    :goto_d8
    invoke-virtual {p2, v4}, Ljava/io/InputStream;->read([B)I

    move-result v5

    const/4 v6, -0x1

    if-eq v5, v6, :cond_e6

    const/4 v6, 0x0

    invoke-virtual {p3, v4, v6, v5}, Ljava/io/OutputStream;->write([BII)V
    :try_end_e3
    .catchall {:try_start_d6 .. :try_end_e3} :catchall_e4

    goto :goto_d8

    :catchall_e4
    move-exception p1

    goto :goto_113

    :cond_e6
    :try_start_e6
    invoke-virtual {p3}, Ljava/io/OutputStream;->close()V
    :try_end_e9
    .catchall {:try_start_e6 .. :try_end_e9} :catchall_111

    :try_start_e9
    invoke-virtual {p2}, Ljava/io/InputStream;->close()V
    :try_end_ec
    .catchall {:try_start_e9 .. :try_end_ec} :catchall_c7

    :cond_ec
    :try_start_ec
    invoke-virtual {p0}, Ljava/util/zip/ZipFile;->close()V
    :try_end_ef
    .catch Ljava/io/IOException; {:try_start_ec .. :try_end_ef} :catch_10f

    :try_start_ef
    invoke-virtual {v2}, Ljava/io/File;->getAbsolutePath()Ljava/lang/String;

    move-result-object p0

    invoke-static {v1, p0}, Ljava/lang/System;->setProperty(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    invoke-virtual {v3}, Ljava/io/File;->getAbsolutePath()Ljava/lang/String;

    move-result-object p0

    invoke-static {v0, p0}, Ljava/lang/System;->setProperty(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    invoke-static {p1}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V
    :try_end_100
    .catchall {:try_start_ef .. :try_end_100} :catchall_107

    invoke-static {v1}, Ljava/lang/System;->clearProperty(Ljava/lang/String;)Ljava/lang/String;

    invoke-static {v0}, Ljava/lang/System;->clearProperty(Ljava/lang/String;)Ljava/lang/String;

    return-void

    :catchall_107
    move-exception p0

    invoke-static {v1}, Ljava/lang/System;->clearProperty(Ljava/lang/String;)Ljava/lang/String;

    invoke-static {v0}, Ljava/lang/System;->clearProperty(Ljava/lang/String;)Ljava/lang/String;

    throw p0

    :catch_10f
    move-exception p0

    goto :goto_147

    :catchall_111
    move-exception p1

    goto :goto_11c

    :goto_113
    :try_start_113
    invoke-virtual {p3}, Ljava/io/OutputStream;->close()V
    :try_end_116
    .catchall {:try_start_113 .. :try_end_116} :catchall_117

    goto :goto_11b

    :catchall_117
    move-exception p3

    :try_start_118
    invoke-virtual {p1, p3}, Ljava/lang/Throwable;->addSuppressed(Ljava/lang/Throwable;)V

    :goto_11b
    throw p1
    :try_end_11c
    .catchall {:try_start_118 .. :try_end_11c} :catchall_111

    :goto_11c
    if-eqz p2, :cond_126

    :try_start_11e
    invoke-virtual {p2}, Ljava/io/InputStream;->close()V
    :try_end_121
    .catchall {:try_start_11e .. :try_end_121} :catchall_122

    goto :goto_126

    :catchall_122
    move-exception p2

    :try_start_123
    invoke-virtual {p1, p2}, Ljava/lang/Throwable;->addSuppressed(Ljava/lang/Throwable;)V

    :cond_126
    :goto_126
    throw p1

    :cond_127
    new-instance p1, Ljava/lang/RuntimeException;

    new-instance p3, Ljava/lang/StringBuilder;

    invoke-direct {p3}, Ljava/lang/StringBuilder;-><init>()V

    const-string v0, "Entry not found: "

    invoke-virtual {p3, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    invoke-virtual {p3, p2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    invoke-virtual {p3}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object p2

    invoke-direct {p1, p2}, Ljava/lang/RuntimeException;-><init>(Ljava/lang/String;)V

    throw p1
    :try_end_13e
    .catchall {:try_start_123 .. :try_end_13e} :catchall_c7

    :goto_13e
    :try_start_13e
    invoke-virtual {p0}, Ljava/util/zip/ZipFile;->close()V
    :try_end_141
    .catchall {:try_start_13e .. :try_end_141} :catchall_142

    goto :goto_146

    :catchall_142
    move-exception p0

    :try_start_143
    invoke-virtual {p1, p0}, Ljava/lang/Throwable;->addSuppressed(Ljava/lang/Throwable;)V

    :goto_146
    throw p1
    :try_end_147
    .catch Ljava/io/IOException; {:try_start_143 .. :try_end_147} :catch_10f

    :goto_147
    new-instance p1, Ljava/lang/RuntimeException;

    invoke-direct {p1, p0}, Ljava/lang/RuntimeException;-><init>(Ljava/lang/Throwable;)V

    throw p1

    :goto_14d
    :try_start_14d
    invoke-virtual {v2}, Ljava/io/BufferedReader;->close()V
    :try_end_150
    .catchall {:try_start_14d .. :try_end_150} :catchall_151

    goto :goto_155

    :catchall_151
    move-exception p1

    :try_start_152
    invoke-virtual {p0, p1}, Ljava/lang/Throwable;->addSuppressed(Ljava/lang/Throwable;)V

    :goto_155
    throw p0
    :try_end_156
    .catch Ljava/lang/Exception; {:try_start_152 .. :try_end_156} :catch_3f

    :goto_156
    new-instance p1, Ljava/lang/RuntimeException;

    invoke-direct {p1, p0}, Ljava/lang/RuntimeException;-><init>(Ljava/lang/Throwable;)V

    throw p1

    :cond_15c
    :goto_15c
    return-void
.end method

.method private static killPM(Ljava/lang/String;Ljava/lang/String;)V
    .registers 5

    const-class v0, Landroid/os/Parcel;

    new-instance v1, Landroid/content/pm/Signature;

    const/4 v2, 0x0

    invoke-static {p1, v2}, Landroid/util/Base64;->decode(Ljava/lang/String;I)[B

    move-result-object p1

    invoke-direct {v1, p1}, Landroid/content/pm/Signature;-><init>([B)V

    sget-object p1, Landroid/content/pm/PackageInfo;->CREATOR:Landroid/os/Parcelable$Creator;

    new-instance v2, Lbin/mt/signature/KillerApplication193$l;

    invoke-direct {v2, p1, p0, v1}, Lbin/mt/signature/KillerApplication193$l;-><init>(Landroid/os/Parcelable$Creator;Ljava/lang/String;Landroid/content/pm/Signature;)V

    :try_start_13
    const-class p0, Landroid/content/pm/PackageInfo;

    const-string p1, "CREATOR"

    invoke-static {p0, p1}, Lbin/mt/signature/KillerApplication193;->a(Ljava/lang/Class;Ljava/lang/String;)Ljava/lang/reflect/Field;

    move-result-object p0

    const/4 p1, 0x0

    invoke-virtual {p0, p1, v2}, Ljava/lang/reflect/Field;->set(Ljava/lang/Object;Ljava/lang/Object;)V
    :try_end_1f
    .catch Ljava/lang/Exception; {:try_start_13 .. :try_end_1f} :catch_7c

    sget p0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0x1c

    if-lt p0, v1, :cond_44

    const-string p0, "Landroid/app"

    const-string v1, "Landroid/os/Parcel;"

    const-string v2, "Landroid/content/pm"

    filled-new-array {v1, v2, p0}, [Ljava/lang/String;

    move-result-object p0

    sget-object v1, Lbin/mt/signature/KillerApplication193$j;->f:Ljava/util/HashSet;

    invoke-static {p0}, Ljava/util/Arrays;->asList([Ljava/lang/Object;)Ljava/util/List;

    move-result-object p0

    invoke-interface {v1, p0}, Ljava/util/Set;->addAll(Ljava/util/Collection;)Z

    invoke-virtual {v1}, Ljava/util/HashSet;->size()I

    move-result p0

    new-array p0, p0, [Ljava/lang/String;

    invoke-virtual {v1, p0}, Ljava/util/HashSet;->toArray([Ljava/lang/Object;)[Ljava/lang/Object;

    invoke-static {p0}, Lbin/mt/signature/KillerApplication193$j;->b([Ljava/lang/String;)Z

    :cond_44
    :try_start_44
    const-class p0, Landroid/content/pm/PackageManager;

    const-string v1, "sPackageInfoCache"

    invoke-static {p0, v1}, Lbin/mt/signature/KillerApplication193;->a(Ljava/lang/Class;Ljava/lang/String;)Ljava/lang/reflect/Field;

    move-result-object p0

    invoke-virtual {p0, p1}, Ljava/lang/reflect/Field;->get(Ljava/lang/Object;)Ljava/lang/Object;

    move-result-object p0

    invoke-virtual {p0}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v1

    const-string v2, "clear"

    invoke-virtual {v1, v2, p1}, Ljava/lang/Class;->getMethod(Ljava/lang/String;[Ljava/lang/Class;)Ljava/lang/reflect/Method;

    move-result-object v1

    invoke-virtual {v1, p0, p1}, Ljava/lang/reflect/Method;->invoke(Ljava/lang/Object;[Ljava/lang/Object;)Ljava/lang/Object;
    :try_end_5d
    .catchall {:try_start_44 .. :try_end_5d} :catchall_5d

    :catchall_5d
    :try_start_5d
    const-string p0, "mCreators"

    invoke-static {v0, p0}, Lbin/mt/signature/KillerApplication193;->a(Ljava/lang/Class;Ljava/lang/String;)Ljava/lang/reflect/Field;

    move-result-object p0

    invoke-virtual {p0, p1}, Ljava/lang/reflect/Field;->get(Ljava/lang/Object;)Ljava/lang/Object;

    move-result-object p0

    check-cast p0, Ljava/util/Map;

    invoke-interface {p0}, Ljava/util/Map;->clear()V
    :try_end_6c
    .catchall {:try_start_5d .. :try_end_6c} :catchall_6c

    :catchall_6c
    :try_start_6c
    const-string p0, "sPairedCreators"

    invoke-static {v0, p0}, Lbin/mt/signature/KillerApplication193;->a(Ljava/lang/Class;Ljava/lang/String;)Ljava/lang/reflect/Field;

    move-result-object p0

    invoke-virtual {p0, p1}, Ljava/lang/reflect/Field;->get(Ljava/lang/Object;)Ljava/lang/Object;

    move-result-object p0

    check-cast p0, Ljava/util/Map;

    invoke-interface {p0}, Ljava/util/Map;->clear()V
    :try_end_7b
    .catchall {:try_start_6c .. :try_end_7b} :catchall_7b

    :catchall_7b
    return-void

    :catch_7c
    move-exception p0

    new-instance p1, Ljava/lang/RuntimeException;

    invoke-direct {p1, p0}, Ljava/lang/RuntimeException;-><init>(Ljava/lang/Throwable;)V

    throw p1
.end method
