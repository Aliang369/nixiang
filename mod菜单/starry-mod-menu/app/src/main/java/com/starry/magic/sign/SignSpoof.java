package com.starry.magic.sign;

import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.pm.Signature;
import android.content.pm.SigningInfo;
import android.os.Build;
import android.os.Parcel;
import android.os.Parcelable;
import android.util.Base64;
import android.util.Log;

import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.util.HashMap;
import java.util.Map;

/**
 * PackageManager 签名伪装（MT killPM 同款）：按包名回灌原厂证书 DER。
 * 供 TapSign / SecVerify / 渠道完整性读签名时使用。不碰游戏逻辑 / Quit。
 */
public final class SignSpoof {
    private static final String TAG = "StarrySignSpoof";

    /** 音舞光年原版（SHA256=cb768829… / MD5=ebdb656e…） */
    private static final String YWGN_PKG = "com.uniugame.ywgn";
    private static final String YWGN_CERT_B64 =
            "MIICxzCCAa+gAwIBAgIETx54NTANBgkqhkiG9w0BAQsFADATMREwDwYDVQQHEwhzaGFuZ2hhaTAg"
                    + "Fw0yMzA1MTYwOTQ4MTFaGA8yMDczMDUwMzA5NDgxMVowEzERMA8GA1UEBxMIc2hhbmdoYWkwggEi"
                    + "MA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDGKI1morQdzFnhNvCh5/Iw/qthuMGmrMQzcezX"
                    + "aOjg9td1GPlRBK2SsfhhePwCTxRbul3MmSBuzaJ/TjNKk7touyyk4WBejBR6T9fG4oVJoq4KVLMD"
                    + "ibh5QSy0Jf1QsLdmauZGYhjtOH1o1jyd70ugZ6GJMtvfYNoDZNOY+r0k9lhGj1Wd5kt5engOf0hy"
                    + "FdYja8ApOaACa2zJVlZI4Cvc15S7U2pbIYxFmKPaJYMKVI6jO8ZdBo6XbmMfjPv4SrFR+u84gPOg"
                    + "WMlXPNqDTymyXixgxB8bDIaFjWV/bHztsu3KkYiDDnccNL72mAcrSCefwqVzEBlmP5i0W6sdVgap"
                    + "AgMBAAGjITAfMB0GA1UdDgQWBBTCyALxnxEdwrQLLWvazNWGkgXISjANBgkqhkiG9w0BAQsFAAOC"
                    + "AQEAmnAYBFvS9cGx6HKW7wBMmdma7CcalTxIINTrn7z983EJdqv0GA3rtWQ3/C+BfjGj7x4GCV/K"
                    + "nBm9Ypd0COHDmQXqQd0yK1rNL8xGN3MZiAQgBHq44ooCvQSjv0lx1kifItNTgzPe0ytHQCU0Gd4N"
                    + "kHPiC9O/bt2zxb+1c6Q/s2pVARdOh+nrc6IFBXRLDNFvoDOOjiA4ZopUEsAZKOxbnB4+YuBeUlyA"
                    + "rjVF2mH2LTmS9qG4xxiVjOdkmg40I5gBZ2k/6NzKuYef1Y6A/Myyn2dy7BuY87jHoQFMnthcorF/"
                    + "6kD3TMBYZltPfxmqvqdASxjF5UYLPQ9ZbZTWgjo1Jw==";

    /** 消消英雄2 原版 CN=Ranhao Kang（SHA256=df0606e5… / MD5=55d94651…） */
    private static final String XXH2_PKG = "com.blina.match2.tt";
    private static final String XXH2_CERT_B64 =
            "MIIDgTCCAmmgAwIBAgIEL8JUwTANBgkqhkiG9w0BAQsFADBwMRAwDgYDVQQGEwdVbmtub3duMRAw"
                    + "DgYDVQQIEwdVbmtub3duMRAwDgYDVQQHEwdVbmtub3duMRAwDgYDVQQKEwdVbmtub3duMRAwDgYD"
                    + "VQQLEwdVbmtub3duMRQwEgYDVQQDEwtSYW5oYW8gS2FuZzAgFw0yNDA4MDEwODMzMzhaGA8yMDUx"
                    + "MTIxODA4MzMzOFowcDEQMA4GA1UEBhMHVW5rbm93bjEQMA4GA1UECBMHVW5rbm93bjEQMA4GA1UE"
                    + "BxMHVW5rbm93bjEQMA4GA1UEChMHVW5rbm93bjEQMA4GA1UECxMHVW5rbm93bjEUMBIGA1UEAxML"
                    + "UmFuaGFvIEthbmcwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCLSTGI6rBH69eAH2j9"
                    + "QOXvqlt7JAhH/Fm2aj0YIfVV9a0II+RClIfrC00ks1x+FV7CZ5V4IS18lQe+sOVNfeNdO2x5zkgK"
                    + "ueROnjzOMEicED45DZJ224CjmOw4mcvammDbjozO+wolv6bRb6g8oSFjEZ3ZqjrVQsQ2L6BAiR5l"
                    + "QVTpnSN2F6kdy36eIuhYo+Xv04M8AWZcS/BCca1U5QtGhaDmNKINfanNcS/5U7pug1p1hUc3/gh/"
                    + "COLlrZ8Y2TAF03uUm4DKUPtX/Xb1f7fGSmw3AOJMJBLNjF1A0oWFsbmiDMlq0KKsuchctiUDHljS"
                    + "/kJDPRWtQ7QmB46MAVV9AgMBAAGjITAfMB0GA1UdDgQWBBR5q+tv7ey9POvYcxnmcRyjyqkkMTAN"
                    + "BgkqhkiG9w0BAQsFAAOCAQEANa04Ai075syRyzi6vwwmOd4fOJJuVDOFKanTn9cQbOxaXHWuev9x"
                    + "3YCxFhwWeZhSG5t4szmXFBTmYXePSjEsUcV8N0a4Qj/bFyyRVymzpq9QXO2wdca1b/hbj2hAfpNt"
                    + "oz+h194Gfn4djkatJ65TEu8O3nniP4+4c4aUkvHFxAFSnMJH6Blt6U67Dex2fXG9mC7frcfDIkUa"
                    + "r3TQK9O7h+i0JDIblQq7atgsgxCn5Rmqavh2XoFRpF2KNoQOu7CcpptiSE7o+2ehGFMVvSsUEg9E"
                    + "XYNybBeNPjNj3MjZnXtOiZ2EgUAXMEZAt6iiqv4vV1yfJlfzFhThaw7060RN0g==";

    private static final String SHC_PKG = "com.bantu.trssgshz2";
    private static final String SHC_CERT_B64 = "MIIDAzCCAeugAwIBAgIEXot1rjANBgkqhkiG9w0BAQsFADAxMQswCQYDVQQGEwJDTjERMA8GA1UECBMIU2hhbmdoYWkxDzANBgNVBAoTBkJhbnR1czAgFw0xNDA5MTYwNzExMDVaGA8yMDk0MDgyNzA3MTEwNVowMTELMAkGA1UEBhMCQ04xETAPBgNVBAgTCFNoYW5naGFpMQ8wDQYDVQQKEwZCYW50dXMwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCfo9ZBPHxNGerz6ioQhfncevvfvNHX2yrpUMxfu+5/qTYqR/mM5zneIK9UWSX2OsGcU8vKiN3lcNFATyunOUBf7r3j4/fbPi7V8OqvT+yK12THAD2h0AUN1zJK/Q6v0fGlr7YZbWm2ElI3eGZ0op2Ulr9BoJfI9ttSnQMTov3hSfqDCLQRq4tqFi5MQDtJgvCXToyHPW4pnPEmQZa6bAkTw7efns/lRKksyKtMBL2Hr/XWSsACqL8d1Oj7M+0D6PW0kycefF8BGg1n2fM8ocgsPUg+5ks5ac9Q3gdHytF+FjpcuJaXqAfXZpl2kRmrNl8OmIwj0bsdqc95Q3aqb6ZBAgMBAAGjITAfMB0GA1UdDgQWBBTyJSv1TwP8is09/N1UP/XFWfLXvDANBgkqhkiG9w0BAQsFAAOCAQEAIW3Er3lTRO8X/e2w9aD0zG044E90guPCQ5BJfXxkj+BEV5oF9p743036EDaGB4+JxQj5mC3NPhkWTG2i0pcPUl0czwkyE+pfzcqRuVo7avXuMpvzFdws77J9xgbFbYe6snvNzB8X7sIIL7XgcYDrnqXilC1N25e8n9G+aTSwb5A4ApCpxRLQ3uBM+LcQwtHc7J26oN+hOWmlD86KRWrbL7OftvS0SnkhRxTqfewobSegxL00LRRL44qs9N0GmtmHtjyGGugQPTek31Gvj0RIPo2gyXep1mPqkwgFD+/vvgcaHPOQiR9lLapjGIP93YaubLQjhlnF9o7tO71QdW7Viw==";

    /** 造梦无双 原版 1.33（MD5=41065dcf… / SHA1=8B20539B… CN=4399） */
    private static final String ZMWS_PKG = "com.xm4399.zmws.m4399";
    private static final String ZMWS_CERT_B64 =
            "MIIDfTCCAmWgAwIBAgIEMq4pZTANBgkqhkiG9w0BAQsFADBuMQswCQYDVQQGEwJDTjESMBAGA1UE"
                    + "CBMJR3Vhbmdkb25nMRIwEAYDVQQHEwlHdWFuZ3pob3UxEzARBgNVBAoTCjQzOTlzeS5jb20xEzAR"
                    + "BgNVBAsTCjQzOTlzeS5jb20xDTALBgNVBAMTBDQzOTkwIBcNMTkxMjE5MDg1MDUyWhgPMjE4NDAz"
                    + "MjgwODUwNTJaMG4xCzAJBgNVBAYTAkNOMRIwEAYDVQQIEwlHdWFuZ2RvbmcxEjAQBgNVBAcTCUd1"
                    + "YW5nemhvdTETMBEGA1UEChMKNDM5OXN5LmNvbTETMBEGA1UECxMKNDM5OXN5LmNvbTENMAsGA1UE"
                    + "AxMENDM5OTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAIXAP95eui2bveVqb5eC036d"
                    + "nJ98X+PKcnYEDKGxdf10xxMgIDiZ1HnYuaKKri6Q4Lpo8ING1MtaNxpJwtQYNR2Ag7eEN4EkqPFF"
                    + "C81WY36KzCb5kKwz+Q1g1gLZX8AzZchM3d0KNj5AqdUB68gNj/dtGgNTIi/JTAFILCo2XLr6N+5U"
                    + "BPbUFlVd6WkSneWXWzTJd1c4ZiRwaWU4vv4Y13QCOcoKPzi0zJ8IvD9mih4XF6oinK5fEiUxRx/V"
                    + "w4hnd1iptkqumM/y0v8nfuzaRZpV2kGlNu4Cl7sh0c4gBWWc9Zhlr6fR4xrErzisGc4QcaB8+reG"
                    + "cMwH/l64B3nnQ+ECAwEAAaMhMB8wHQYDVR0OBBYEFOIsM0oz0ktA/Cat47I9rogxiezsMA0GCSqG"
                    + "SIb3DQEBCwUAA4IBAQBbD1g5s3p3oiwioqgh1fXWJ3OMYpE48y3kQBDppKEGJyo0gZPNP0TFOZtu"
                    + "65zGjp0WuLNPmM9Nz2zCKOSOlkMC4/7VCnDNWTEeHZoJuS9AUfJnzKMoD0YiRII8u77uMpra0inI"
                    + "hOQBLUsUyXo8nVLgDo2GQv5U47EHk6oyCX7kguQ5jAcFD6EAm3NOUgUxfBRO5GHMWQ0gbmr+m38e"
                    + "cEBdZbM5HmZ5Z8tFu86/t++3SIDzvBQCj0xTy/o37ztUMHTi016GuYj0MyP/ROTU66aAhdtAoZbZ"
                    + "gHZEJ9qrpclAibUQ7Oo6FoQHGJpzBoGyE7VKOMZawKwPItYiTiMzNmfV";

    /** 造梦西游·黎尤浩劫 原版（MD5=c07390da… / SHA1=1B1271C6…） */
    private static final String ZMXY_PKG = "com.xm4399.zmxy6hj";
    private static final String ZMXY_CERT_B64 =
            "MIICuzCCAaOgAwIBAgIEfdrdZzANBgkqhkiG9w0BAQsFADANMQswCQYDVQQLEwJjbjAgFw0yNTAz"
                    + "MTgwNzM0MjVaGA8yMTg5MDYyNjA3MzQyNVowDTELMAkGA1UECxMCY24wggEiMA0GCSqGSIb3DQEB"
                    + "AQUAA4IBDwAwggEKAoIBAQCCu6efvoPdIVdlO6Qls45/6p8s/p1wnzt7bE7+jGLmXWGqW95dsZGi"
                    + "IZ2EHV2gKN3RzN1JIbbsKB1FKBpKdS0hHEWleYAj+Ee6V7a39qBO38hU5uEp3KoVj76fE01fg20l"
                    + "eB8nOxY/nNdFziv8SR6ArE6ZDcX8Bgp8YXZgjH3qUSeWYjCMFFfFY2SYT4iZthkOYXGWPhB80D9l"
                    + "Z2AOwVorVwmMFrUj9PLZgWgE6cW70nW/loaS4Nu/H8kFgREty4JmI7bErBBbXv2WyeGKD95I4JV/"
                    + "GM1eZ67LAdgo4IIbhhyvlkg+3y6qW9YBvWfRGq/IX6QA9UsdNY5cDPzffkMpAgMBAAGjITAfMB0G"
                    + "A1UdDgQWBBTRWkIR55z5ONpIJt68GVTYJKfNlDANBgkqhkiG9w0BAQsFAAOCAQEAad19QbGVEms0"
                    + "i+Hardz4ecxe/U/4tkysBdMWjAzsbbJzLNL5l0ye1HOmVN/M5Z18EIMWg/q8pMVIf5VpzamVyQZS"
                    + "Jib3oPdjw2z9Ps3qUaCubfygHzTtqo4Qfs7aUwOXPihEwAedKReSy01G+d727sGfFjqrCQAd1S4v"
                    + "nsSjhWTh2LWnJlXvmKOLGIijoGSLg61v9lNxm15LOWHDgczt7IRIk8Q9hG8AMFU8L31VUb34f3Sy"
                    + "UFfCM/y2vXpuJ95ikpa3AZ1CJgmg8yFRnCjG3OEkDhzT81b5IKtRuS7DeDXXJ+Mce8nyqbqDwxQD"
                    + "tYt9YJLmQRpsGVKU3QTiPjWiEQ==";

    /** 雷霆战机：集结 原版（MD5=f0c5671a… / SHA1=1d8eeed3… CN=clover） */
    private static final String LTZJ_PKG = "com.a5game.ltzj.jj";
    private static final String LTZJ_CERT_B64 =
            "MIIDPjCCAiYCAQEwDQYJKoZIhvcNAQELBQAwZDEPMA0GA1UEAwwGY2xvdmVyMQ8wDQYDVQQLDAY1"
                    + "YWdhbWUxDzANBgNVBAoMBjVhZ2FtZTEQMA4GA1UEBwwHQmVpamluZzEQMA4GA1UECAwHQmVpamlu"
                    + "ZzELMAkGA1UEBhMCQ04wIBcNMjQxMTI3MDcwMDI3WhgPMjA3NDExMTUwNzAwMjdaMGQxDzANBgNV"
                    + "BAMMBmNsb3ZlcjEPMA0GA1UECwwGNWFnYW1lMQ8wDQYDVQQKDAY1YWdhbWUxEDAOBgNVBAcMB0Jl"
                    + "aWppbmcxEDAOBgNVBAgMB0JlaWppbmcxCzAJBgNVBAYTAkNOMIIBIjANBgkqhkiG9w0BAQEFAAOC"
                    + "AQ8AMIIBCgKCAQEA04cpHdcPIbg4UycmzpBUq1tsjmyVSWzJUazLs6OJNa/zehxYjbDW58om9Vsv"
                    + "otpWGi/RaIVFYcvDCNNRkUf4+pqqUwr+gX6pbtrMQABW7MD1eXd7uQZ7b3LKjqCWFD45cc04mXy4"
                    + "2L1CZtbkVgMQTgGGARCbDtT9DvmIjwwyQVDkKoAi/p9twnqhHD6O8pNSV20HS/JiY9dtR1f0/P3L"
                    + "18XdupnSlHujEzHGb7jDlW//sFtVd9PAXBqJ/iF/X1kh3f4bPTBJ4GXl/Nya/VfgLdt+wCbo4Ow3"
                    + "o0grqOaaoRYpMj720mwM1uWMspPYS6VL/M7Le8DkxNA5BTncLn/rFQIDAQABMA0GCSqGSIb3DQEB"
                    + "CwUAA4IBAQCDPUthQ00msBh641j6YLSz0P8v/Eb5exqIQDts/zwHqMjG5QkkiiJASo7HMaXTR44w"
                    + "OMyHWUG8hnumEuOJE8ix/NqpAy5VVcBMg80BXSfwl1n6YHvrgyXGAly/JdYFT9PRPv2wYYYGCTCz"
                    + "SKT6uofDtdkcFNX2pLU7QdoidYn54KURcAKuFqJOCC9YM20pY3oj4j+6uOd+MMQVEFWUaRYU2uzD"
                    + "lONoMOsLJD3jS1phJ4jpCdG0xJ8r5oOYX1RZb/u4QK8EQDIEFx3L1fi9DeH5vYFcnJ1WFFH8J/6e"
                    + "4rdXGdO+omTVULtOSDAK6iAE5slvmdCe1vP3kfBKSqaQiuAn";

    /** 霓虹深渊：无限 九游原版（MD5=232126f8… CN=9Game） */
    private static final String NEON_JY_PKG = "com.yhhy.neon.aligames";
    private static final String NEON_JY_CERT_B64 =
            "MIIDcTCCAlmgAwIBAgIEH224sjANBgkqhkiG9w0BAQsFADBoMQswCQYDVQQGEwJDTjEbMBkGA1UE"
                    + "CBMSR3VhbmdEb25nIFByb3ZpbmNlMRIwEAYDVQQHEwlHdWFuZ1pob3UxCzAJBgNVBAoTAkxYMQsw"
                    + "CQYDVQQLEwJMWDEOMAwGA1UEAxMFOUdhbWUwIBcNMjMwNTEzMTM1OTMzWhgPMjA1MDA5MjgxMzU5"
                    + "MzNaMGgxCzAJBgNVBAYTAkNOMRswGQYDVQQIExJHdWFuZ0RvbmcgUHJvdmluY2UxEjAQBgNVBAcT"
                    + "CUd1YW5nWmhvdTELMAkGA1UEChMCTFgxCzAJBgNVBAsTAkxYMQ4wDAYDVQQDEwU5R2FtZTCCASIw"
                    + "DQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBALKxx+tPqQp/jTklUsT/lYeU1VhYlDiC52UMMnGK"
                    + "/glp9sVq+Aq4n8YvuhcrWfGBx/RERXHUtxa1xaZilxtDpr/rvivTVRB7jNg+c3nqmsdZ61GFNWk7"
                    + "xUhEJ4EfhPm+vNsw5WRU/nNAFrOC52FcyuGudRoTF6QQAhjLy2KeB6oHmyId8KmoQVXldU3gMHjq"
                    + "kC3n5QM4axsxYDsxhIhwUrCypA+sLeyfmQdioDxRdftGrGU3crO289zPVXUPBey7wkYbyrFDz9js"
                    + "w26SRMxCjK/4wqDElIG5QDkVawJrbU/uQvWg8r4mFmrNxY7YaNNqXSbfvba7vprcEZ8LV2GKp/0C"
                    + "AwEAAaMhMB8wHQYDVR0OBBYEFBniDs8dFUbh/CXvZ26bBae5hRi8MA0GCSqGSIb3DQEBCwUAA4IB"
                    + "AQBuoQN+gJ9WTeAI9I1YISzN95lYyWgseMgqg0sLcmY/KQKmANWLlwMoUY7gPtxl+FGs12gwY+Cm"
                    + "hg2GuXZk1JV1PgHDiKbNO5nouD2YuGt3YOOUR+H1gI8WTSB7++5CO6Kp9Kq6n+UQLP6BYtUeiJQc"
                    + "3IuXp5tI3woNGNx3h59Y06+xDF44wAtq60jd76mXZ4Wt/0OL7m+/hu3m5ixKqlJfWt4WsKbL6Ekq"
                    + "3+iFWhoysnEeWY4ISgrDHNN5C0x+OUGOa4gssRlqhnqMXHkamcv7pAUjqUmKkcJ+YEp/WFt8WHEy"
                    + "ZnNtZWr7Jj1Z6S/nxO+5gBJj9UIEtOSwEfjluxVi";

    /** 霓虹深渊 TapTap 原版（MD5=49af51d2… CN=YiHai） */
    private static final String NEON_TT_PKG = "com.shangyoo.neon";
    private static final String NEON_TT_CERT_B64 =
            "MIIDezCCAmOgAwIBAgIEars2hjANBgkqhkiG9w0BAQsFADBuMQswCQYDVQQGEwJD"
                    + "TjESMBAGA1UECBMJR3Vhbmdkb25nMREwDwYDVQQHEwhTaGVuWmhlbjETMBEGA1UE"
                    + "ChMKWWlIYWkgSHVZdTETMBEGA1UECxMKWWlIYWkgSHVZdTEOMAwGA1UEAxMFWWlI"
                    + "YWkwHhcNMjEwMjA1MDk0NzE1WhcNNDYwMTMwMDk0NzE1WjBuMQswCQYDVQQGEwJD"
                    + "TjESMBAGA1UECBMJR3Vhbmdkb25nMREwDwYDVQQHEwhTaGVuWmhlbjETMBEGA1UE"
                    + "ChMKWWlIYWkgSHVZdTETMBEGA1UECxMKWWlIYWkgSHVZdTEOMAwGA1UEAxMFWWlI"
                    + "YWkwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDLDaR+5AKANDMaZS9q"
                    + "YowjVuS8rO48IO/4RIUlOW5hWyQCE+5kPTbRgWrPXhSM1JumFneUm3C56rIYw+zL"
                    + "KXW+JfvcHX8xzZ65RVRyK+nZM9O5BNyMtjwl/KScdmy/8MRw2Tj8vvl7cqdnokhh"
                    + "efRZOdZ4Fz14B1psbKsJzMa1TgMxB/9KdckNrvEfC83r78I1zLTNHkPTzNANSyOn"
                    + "usAx/jPOCQDOxIh1mfmYwkKQdyptRLl7upCLuyuOSOBKypU6gCbx5fUWARoEpSl5"
                    + "ESkAJD9LwValuuP3suQrbpaJOxEdjQgs1x4df/wfynZXonzy5DB5zk6Ip2mAO6DA"
                    + "gblBAgMBAAGjITAfMB0GA1UdDgQWBBQlFvoK2ikIYF22u7xYiNg/YpY28zANBgkq"
                    + "hkiG9w0BAQsFAAOCAQEAkEFI+bD6ooe7GdukySWyEMuZ+lhU1x/PjRexTbMZrAGG"
                    + "BIfEaOs3wxGauPexhWyv3GbKEaiG8/mcAC4BP+voQx98d9dicI7Hs5vkslqhQ8bK"
                    + "e9MnPU9pdYdlM3Hx+O+rWGQDGqIQNgyJGPKyCIAnTFgY7X8GbB20ioWQRvGbMvGF"
                    + "Mk51tJm0MWJLlT8vrsqhdzn8azne1X2DYPlG2maJIyLjB9OYYS4Xx0EQmjyOeN3d"
                    + "+lhrstBRYBpvdJuvY5FEjaexuVnXTrtiSp4En7mxx8XME0MALGY4FNbhZSIWAZa6"
                    + "ijRNJ9D0f81zA/Cw3hfUYi0JtzGwDDPiTNzpmGH3Bg==";

    /** 追迹迷城原版（MD5=4684a2b2… O=深圳缇米乐 / CN=蒋帅） */
    private static final String ZJMC_PKG = "com.TeaMelo.DungeonChasers";
    private static final String ZJMC_CERT_B64 =
            "MIIC+DCCAeCgAwIBAgIET2GDCjANBgkqhkiG9w0BAQUFADA9MQ8wDQYDVQQDDAbokovluIUxKjAo"
                    + "BgNVBAoMIea3seWcs+e8h+exs+S5kOenkeaKgOaciemZkOWFrOWPuDAgFw0yNTA5MTgwNDMxMjla"
                    + "GA8yMDc1MDkwNjA0MzEyOVowPTEPMA0GA1UEAwwG6JKL5biFMSowKAYDVQQKDCHmt7HlnLPnvIfn"
                    + "sbPkuZDnp5HmioDmnInpmZDlhazlj7gwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCT"
                    + "YYMtXdEAeESSnIx/fxT93xO5eU9qthWk5ltuqguiZiJquVTu4orUJzU9c1XFXsYE64Pn9oP81eiM"
                    + "Y1koriPvtjYzNOGXq5zlKvmeyDHueyBy4WwdBiNQ5ON5nLRRRvSfeBd13W6ASzYdvoZOqIzvMb+6"
                    + "/BP+BeaBxndUOtIAzCU0OsZGE5SwfCFZYDcxkwQ0CyJ6HQeSa7nowv426WuxiKBIo8KsIFbzzYr0"
                    + "fQdO/0SJAsFsuGQSGuzVPO4SP78+rW0K9gQ3uTtHrvDBnkt2g2jxD/aTsV8JXfFuot98UMVE7wci"
                    + "qR00aQlwd/Q2u/Sm0OrbHcSL6ogt+9mSLrRTAgMBAAEwDQYJKoZIhvcNAQEFBQADggEBAIS58JF0"
                    + "1a445O0sfO4tWC+0hWKM0g1bbk8z5Ugqd2/AeYSq9sOMxrcsqmRFAEilz00bJH1bc/du8d6PMzQu"
                    + "ycao+o73FcNkZgjHZTBMcR7X/SPvytMrT7SzYbW7IFuK4fxKAboxq0CnoKOPK1xptm8Wqm6VQKg8"
                    + "/IYEnJ1Z0CSKCRYRFp68I/d7ah77DG5IQlDJ9KHeL80SNrBXgMow+kRl0Ui3SrTl6JRXq/pkLgGO"
                    + "BcSj2pHWWshF0I5/izr9uauCrYs60OSmrz4rxVQeFZd/nRed2G8T02G63UXrxVaw9G2vM7FdqTsb"
                    + "TvxTYsXDsIfZwKGaWw3Gqj7EyDC+7Q8=";

    /** 魂坠深境原版（MD5=a3e3c07b… O=Chillyroom） */
    private static final String HZSJ_PKG = "com.chillyroom.containerofsoul.yw";
    private static final String HZSJ_CERT_B64 =
            "MIICqDCCAZCgAwIBAgIEblvRrjANBgkqhkiG9w0BAQUFADAVMRMwEQYDVQQKDApDaGlsbHlyb29t"
                    + "MCAXDTI0MTIxNTA5MzY0N1oYDzIxMjQxMTIxMDkzNjQ3WjAVMRMwEQYDVQQKDApDaGlsbHlyb29t"
                    + "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAlbjt9ljJmsOgHeVPO+ydcb8oVfGZXwOF"
                    + "5KT+tuh3y7i31ngnLROXXLrwNB4MdULROXSgg/t2yNNZsvKMfCY0bWeKARLETsdjq4QN93KYL4ad"
                    + "0JMWZuH3LvfZwS1A5MWxrkUPM2tf4nCGmWJzR+uTX5E6pdwYXg+u8+gtZY+22VqD4M3dq5FkM58B"
                    + "9NxgEJMSCAnmBo/h8wRfioNB6pGM6bNPCtvCDiw4/N5Cljtb0nJRjYi4a68IGjmLLhSSYM/lsdwL"
                    + "g48dFS3R2OzpeMMfuxOTnsUUFmxkFlyT7Qgw9wTkLT12lsEqTrht7PAqgrokUpEOG1w4XalPvN2a"
                    + "HOzuMQIDAQABMA0GCSqGSIb3DQEBBQUAA4IBAQAtmVe8wZ0tWLomRWSveCjWmLoAKjIqEJirUpwB"
                    + "izXjDZzYlmLkS+4pB185GE/u70QFiPuO+h8S6mjNpFdeBbi9l/xq5yOWkXn7I1awQLfpc/pneEFk"
                    + "NFS4N+lrmJo0U+Ku4TuShzp7FAmU22++0gPt0459zM/x/aLcUPylWor2g2bxdx6h8mTWuDnm4fE5"
                    + "WQzjzNi1CYcwd6nwbKTPBmHxANeBZzKWoPQCIy+lW7KGb5boNcL6PhXBgNRDB1sw+XLGemF4lLId"
                    + "DLG/yIK7moFefTvCTLcVEC7rcgrc51GbZ6Gittu5qzgb/oTFgTqFJKgTkUX/BMIT0yEDiWmyHiCP";

    /** 长生天机降世原版（MD5=005b1895… CN=netkuu） */
    private static final String CSTJJS_PKG = "com.netkuu.changsheng.tjjs";
    private static final String CSTJJS_CERT_B64 =
            "MIIDVTCCAj2gAwIBAgIEbehe2TANBgkqhkiG9w0BAQsFADBaMQswCQYDVQQGEwJj"
                    + "bjELMAkGA1UECBMCbG4xCzAJBgNVBAcTAnN5MQ8wDQYDVQQKEwZuZXRrdXUxDzAN"
                    + "BgNVBAsTBm5ldGt1dTEPMA0GA1UEAxMGbmV0a3V1MCAXDTIzMDEwNjA2NTgyN1oY"
                    + "DzIxMjIxMjEzMDY1ODI3WjBaMQswCQYDVQQGEwJjbjELMAkGA1UECBMCbG4xCzAJ"
                    + "BgNVBAcTAnN5MQ8wDQYDVQQKEwZuZXRrdXUxDzANBgNVBAsTBm5ldGt1dTEPMA0G"
                    + "A1UEAxMGbmV0a3V1MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAo6/c"
                    + "5/7wTRpzET80XMUfywAMgpF99qbNuHeyM/RZ/2nDq+PnElAHFIUxYGEK+c2GVIIF"
                    + "wo7d0qAF346sLq9hZUbW0d5fPd2MsC/H2PgyvkQLwIl0gi9NM1y90qkybey8uuAF"
                    + "jd3TEzqTMxYt/0Fn5rsnIqrcpReds9K7DFpu9QxKzcTp3EGPLRKImg1mLUbVAs2W"
                    + "ACqSsQlrUfpFqdAZfN1wryy4iIow0n5b2h8jfP2veF3H7mCLNbctnDF9aM/qJ5mT"
                    + "qM5WpJKosR/cQ7gyqLfSQn6Fe+jvZGU1h6jkgtdbhTard2qaoYFIb1biC/lLa/mY"
                    + "DScYevWNMwggkMIkxQIDAQABoyEwHzAdBgNVHQ4EFgQUymNa3c0t+5Ia9yRYOuig"
                    + "ykMMzi8wDQYJKoZIhvcNAQELBQADggEBADtOYBwVcTFnsPJNueE/oYY0Lw3AWFWy"
                    + "1qJJU/23kzpzFN9d0yG39bqek7MtD+42KIGwEqNyRIkdKoqylsywHkVKcDwDxCSF"
                    + "A/FknikHaMbx2m+WAs3/S0Wp12EL8ZCoXWpHK44SD54l3xIjD4EZqudkTu/sgToq"
                    + "O+lvlOMysdbAW0oxokeBAFPwMaGSx5JRaFOtNIXYm6ScgGjqNr846NghUOduIolS"
                    + "k+x+6UZb3mF/yz6v9CiF0CJSSORI0OyoAWtN4vaBEQWjuiWVfJt7KmtJKbsxr4sM"
                    + "zd2WsPLD7fub0dooc+rPNbGaIJyCO+M7o5KGogtJ+4at89snQa0IqWk=";

    /** 公主连结原版（MD5=4502a02a… O=bilibili） */
    private static final String PCR_PKG = "com.bilibili.priconne";
    private static final String PCR_CERT_B64 =
            "MIICxzCCAa+gAwIBAgIEZS88wzANBgkqhkiG9w0BAQsFADATMREwDwYDVQQKEwhiaWxpYmlsaTAg"
                    + "Fw0xNjA3MDExMDQzNThaGA8yMTE2MDYwNzEwNDM1OFowEzERMA8GA1UEChMIYmlsaWJpbGkwggEi"
                    + "MA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDAw+PVCYPuyxpAsFezq9L9cU6tYEQOWqF1qLQ1"
                    + "788UhV+Dmu96+KVX6TUazvJZuJtANXKYdzZMW28x/cKQZbiJLgu/FF9Hzf3WJgDSUwnD2Eccrj14"
                    + "kja142detlamVBbpt+Zua9M/ZvsHmzcHJmzlos+THHl3QlNoZpCv7WNMEf8oFYsNPmSjSjaPRWV7"
                    + "UnoKw+0WPdi0PDQpfgf09Gxzw/mXDNEfLCchs9Oi38nviFZuGXUS5IuyTaCV1AfqFcyaBpP3Rgnh"
                    + "TupTZqMSHdf54sy3LH+92txtvtToeILuUWExi4EyeHS6O7N27U2ol6zHJza3cmbXuB7SUhOviGbt"
                    + "AgMBAAGjITAfMB0GA1UdDgQWBBRB9Xa+ijVO2KgE/KE+jcl+SL0YbDANBgkqhkiG9w0BAQsFAAOC"
                    + "AQEANYkojhVBa0Wq4tNEsukBiSYdEbA9RnKWdFEwu9xTGIZfDUvUnC/ZnkHKtPfbydYDvvUctJld"
                    + "20qFdC8RgC6ab1Cu1jOdMgpRxn2rTecDnh64e9hb0Dr3RGg/N+mEuf9yMG4fUrWJcW0LyUYiyg3T"
                    + "n69vaE3CvyfLR5hdR8VAgpeuh/a8/uAc9gl4fD5/u1MkCd65R1MsBsLD5MonhrdzsdhK50CLFEV8"
                    + "O1Q2rgQQ++jKRBCQYHOmX1IKXNRim+RCSZGAL/HYYDixyCoxuI687wU6tCFTwyi7ZypL3RLW3ba7"
                    + "brsLOuz57FKxOpxodOQ5zbT1K7kaHyCCM0UIl0X6jg==";

    /** 弹力果冻 4399 原版（MD5=08cee890… O=ginicat） */
    private static final String TLGD_PKG = "com.ginicat.sweet.m4399";
    private static final String TLGD_CERT_B64 =
            "MIICojCCAYqgAwIBAgIEN1VqzzANBgkqhkiG9w0BAQUFADASMRAwDgYDVQQKDAdnaW5pY2F0MCAX"
                    + "DTE5MDUxNDEzMjMzNFoYDzIwNjkwNTAxMTMyMzM0WjASMRAwDgYDVQQKDAdnaW5pY2F0MIIBIjAN"
                    + "BgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAk+oA9H+hF3uCVIGXydfA7wg1K4JwbA5vx6a6OtlI"
                    + "HA3oTou3ULP7Xux2eaDB5PRdQp3//68/vNOFZpGyx5WdZy4sreRQyMUD69muJ3dRpHCNtbwZF564"
                    + "WSAxIneTn3dytccF8NC+BvIN+KbS9huE0F9zKoUFDibG6vgEbrMVUOWDLKNMNb8zOzxRLWllqbZU"
                    + "Eo3KdKMYZySriWC+1yANSSOiKduDK72TIH3MYiBfUR3kNAHop/7O4Rh4kLpJK87HiImm/5aH9AV9"
                    + "vlSkvyVzq/cYZouep+RdkfqG2fHDmuutSDNCW06yovKmC8Lfn6Rx+4MlWvhRBG2X2nYrroY+uQID"
                    + "AQABMA0GCSqGSIb3DQEBBQUAA4IBAQAuvtthuSKsxigpmJm4nkpAcM+YvQps0ck9tcSl1k88jbD4"
                    + "5ccsPjyaUmZ8Vz2uwDMx5ZwYGT6GAnkVK3XfZTPap9NTYH1Qk006phKLXG4RwF9Il3DH9mwJFZR6"
                    + "K1KeR79Pztl9NhcSKJxMuU0MFbTF49Ma8NTF6lGAMYI6whhoO7BZ8kLc8yKiFJU3nnkfaogBdhYD"
                    + "4fGUgqCLc2j12f+wT3uLK6otfQAYpa035Lbvhieeq28bhNxx/f+jGaY1OBYd9hdJT9sMo4+qhtxK"
                    + "O+tp8XM5XqFzQwkdmOB5AAE3Y0f5+2ZBLfP1oGtmxPKH367y82n8lZewfV/ahxQBm5OI";

    /** 弹力果冻 官服/TapTap 原版（com.ginicat.sweet，vc4409）。
     *  证书与 4399 渠道**同一张**：已用 openssl 从官方 TapTap 包 META-INF/CERT.RSA
     *  抽出 DER→base64，与上面的 TLGD_CERT_B64 逐字节比对相同（904 字符）。*/
    private static final String TLGD_TT_PKG = "com.ginicat.sweet";
    private static final String TLGD_TT_CERT_B64 = TLGD_CERT_B64;

    /** 独奏骑士 原版（MD5=6870e083… O=Shimmer CN=shake） */
    private static final String DZQS_PKG = "com.Shimmer.SoloKnight.cn";
    private static final String DZQS_CERT_B64 =
            "MIICwjCCAaqgAwIBAgIEcjRIEzANBgkqhkiG9w0BAQUFADAiMQ4wDAYDVQQDDAVzaGFrZT"
                    + "EQMA4GA1UECgwHU2hpbW1lcjAgFw0xOTA2MjgwNDAwMDJaGA8yMDY5MDYxNTA0MDAwMlow"
                    + "IjEOMAwGA1UEAwwFc2hha2UxEDAOBgNVBAoMB1NoaW1tZXIwggEiMA0GCSqGSIb3DQEBAQ"
                    + "UAA4IBDwAwggEKAoIBAQCNFzS1AA8Gti39pac/MP/H3If5c8QG8ny9Tj5rt6H8BZNmY3am"
                    + "z9py+dHG/QjUA9m7+V8WPEe4XE/STE/IRs9qvTTgW/w+RFe3fiRzGOst9sSogwY+yuaqB1"
                    + "6Kz7rnmQTzKpujmQ31YpwYJgSGfjpdQDr5/lnk1CG5QbA0ss3wZF5fFG8+Z7aAmyOqkfIs"
                    + "3Xnmyf6n4mK1agaDTZWZs7jRGDrKfDGiVpBLo1tVCIQpTdZI0IMaZDaTQQIJaG0QM4WfXY"
                    + "0tE2D5LqoRnfNdeJe6KX9NO8JaEdJ3GARRob4Ijx2E0XF9nHHggz4+ebNvuQK0/J+Z2/zd"
                    + "fnDCKm9bDj9vAgMBAAEwDQYJKoZIhvcNAQEFBQADggEBACX9aljjXLGdklCJOKdX1gTEOa"
                    + "q2CC5bT6Rd/s8/EAwF61SXiYhKjb3KWbQym8Ot+7D5D5WZygbqI+OnuCTYfh4ijuEuBlLa"
                    + "R/nFeBMZe6KqDaxwldKbXxPLrTPO90oTkelPn+GtcbRNFF2jKZA7CwgWCc6V+HJzOPQOKC"
                    + "oS1yvQRuyaZcpldUufMrrc103EwwXFVVlmndtF/BH7CneJixRIGDfAuT86UPzV54QZaKT6"
                    + "vlg+AEEjgkSMljxNuKbJq6C9aIxd8jRkFq2JC5GiUdrrqM9bY63+IbsUVWe6XLfWVbN6mO"
                    + "Ikt619w16+R+jGEM7dvAgXo9kEcxvSev7HIbs=";

    /** 孤勇无双原版（MD5=5e96c1d0… CN=Fattoy） */
    private static final String GYWS_PKG = "com.fattoy.gyws.android";
    private static final String GYWS_CERT_B64 =
            "MIIDbzCCAlegAwIBAgIEMnh08DANBgkqhkiG9w0BAQsFADBnMQswCQYDVQQGEwJD"
                    + "TjESMBAGA1UECBMJR3VhbmdEb25nMREwDwYDVQQHEwhTaGVuWmhlbjEPMA0GA1UE"
                    + "ChMGRmF0dG95MQ8wDQYDVQQLEwZGYXR0b3kxDzANBgNVBAMTBkZhdHRveTAgFw0y"
                    + "MjA5MTMwNjM4NDVaGA8yMTIyMDgyMDA2Mzg0NVowZzELMAkGA1UEBhMCQ04xEjAQ"
                    + "BgNVBAgTCUd1YW5nRG9uZzERMA8GA1UEBxMIU2hlblpoZW4xDzANBgNVBAoTBkZh"
                    + "dHRveTEPMA0GA1UECxMGRmF0dG95MQ8wDQYDVQQDEwZGYXR0b3kwggEiMA0GCSqG"
                    + "SIb3DQEBAQUAA4IBDwAwggEKAoIBAQC3qVnzrHzRheK7DLoaREMC/GDOJxE0UMkH"
                    + "LD5tYM1ArmJDKeVA7EYZE1cDuz7I48aZ2SnoFn98theeWYKx4eQIsNeJeipkXdvd"
                    + "c/WCHSEFYEO5sczlzRPn1L8mRUVfWkSclk6KYyb/AFuWhmYCP6OYWIbUetY/7nTz"
                    + "3kmZ5AzM8F/zRy1fRtuHfPaUMktgQXJsNav5T6b343UbrdEhHxVpCOwIQ2gkaeKu"
                    + "wBovDSaOQ0hJmNImv8q+vLszxkiLcckJLBEHQS0pfqcJ9uLPXTMJkNNFuQP//9Z0"
                    + "5KywaXlODey3yMpEkHl82iCa88vwTCqbuIdRWvOyzOKtzFldwcy9AgMBAAGjITAf"
                    + "MB0GA1UdDgQWBBT1kpjiBADxqdWe45Wtu/69aK726TANBgkqhkiG9w0BAQsFAAOC"
                    + "AQEARPDVtbmrdWeyz1s1MKo2fYdB/FKACSOwyxpbb9oydTgSRac3LWpR2rIOzs/+"
                    + "rR8Nh5iwRAgcmnarfeSkD+AE4ZAWdgFdfobBjMhV4GXNChxUNYbBJM4tWdajD3UP"
                    + "KudQJ9GpliF8dNnWnT1yQJMShrgrHpNumRRiSWKrkMCN2CFckbqIi1OeTwnUp9Ll"
                    + "xa3n2ZS66/Xq3w1T7WFbt5wz1n9pqVaISYQwks3gzQO5yUdC7VZdEr4oU+k6LAYh"
                    + "kKjDvgqzt0wLV0zb+SGxUsQWe/AjbK7ALQ6AkiCV+TIUZ3JSyxYj9rI77HQLe8qW"
                    + "VQ9HDwswHKhDbd8Ds3GTnhUjTA==";

    /** 弓箭传说2国际版 1.0.1（MD5=8adcdcca… CN=fc） */
    private static final String GJC2INTL_PKG = "com.facai.archeroii";
    private static final String GJC2INTL_CERT_B64 =
            "MIIDADCCAeigAwIBAgIEYaA4FTANBgkqhkiG9w0BAQUFADBBMQswCQYDVQQDDAJm"
                    + "YzELMAkGA1UECwwCZmMxCzAJBgNVBAoMAmZjMQswCQYDVQQHDAJmYzELMAkGA1UE"
                    + "CAwCZmMwIBcNMjIxMjAzMTU1NzM2WhgPMjA3MjExMjAxNTU3MzZaMEExCzAJBgNV"
                    + "BAMMAmZjMQswCQYDVQQLDAJmYzELMAkGA1UECgwCZmMxCzAJBgNVBAcMAmZjMQsw"
                    + "CQYDVQQIDAJmYzCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAKM8TsVF"
                    + "tWiy3eXXbb/e8HjUrOcsYC5/cL9ZevYE6VAnJHrZZqCj3oWAKU6Txf76oCfzsZso"
                    + "9r9dEAteeVNGKvGfDthQpNKOCAZwxp2szlOimNW2ms3bNMZNxuStd6xj7rCP9i1L"
                    + "gxr4Ru5RF3J846nJ8+NxKMRtCl6djeA3a/uS47uuWzeruTYOw8cc5EriqZUEgcg3"
                    + "eT7youqO+oedol+FRTcED5lUEU5iebUwoROtrAUDf2aWvgT14JKM/19Z4HuEK83c"
                    + "X5S1kBqMo+sbVPbu8/b1AANh6j/T3/4mB9AgVfzd6CkfYTz3/NpFV2JwRBU/BEAP"
                    + "oyxrYctWgKsRHDkCAwEAATANBgkqhkiG9w0BAQUFAAOCAQEALSHKRAPOPL10DOom"
                    + "yDQkLij7leRDvv6UylT/tBrkcndHE5Ww8zC9ablX+UTKD24bAtWufxT8PGFQZvrU"
                    + "aP/CrTHiBuNTocQz8E35wGhF/QhOwXBY6BuLQVkgoQ6zpciu66G0JsBUAgWjNQQc"
                    + "YW/CIUsiAx63jxS4wojH82TQebnvmmDBIp39/uIOnA+iTQJKQW8mVa+RhAEajve9"
                    + "3DRjC7J2JqGdUobWxTgPW4jLEHi1hx5hvFN6vRfsekHJIfhKpdy3kln/A0YtqS3l"
                    + "u5snZiinllFg9sUjJAkLb7xS/u5b5pMsHjymnCnm2+PkKYSVfkjZ/C3KCH+FhKg9"
                    + "nE5HGg==";

    /** 山海幻境 TapTap 原版（MD5=86d19598… CN=shhjchx） */
    private static final String SHHJ_PKG = "com.chx.shhj";
    private static final String SHHJ_CERT_B64 =
            "MIIDdTCCAl2gAwIBAgIEIbCrpzANBgkqhkiG9w0BAQsFADBqMQswCQYDVQQGEwI4NjESMB"
                    + "AGA1UECBMJR3VhbmdEb25nMREwDwYDVQQHEwhTaGVuWmhlbjEQMA4GA1UEChMHc2hoamNo"
                    + "eDEQMA4GA1UECxMHc2hoamNoeDEQMA4GA1UEAxMHc2hoamNoeDAgFw0yNTA3MjUwOTM1Mj"
                    + "VaGA8yMDc1MDcxMzA5MzUyNVowajELMAkGA1UEBhMCODYxEjAQBgNVBAgTCUd1YW5nRG9u"
                    + "ZzERMA8GA1UEBxMIU2hlblpoZW4xEDAOBgNVBAoTB3NoaGpjaHgxEDAOBgNVBAsTB3NoaG"
                    + "pjaHgxEDAOBgNVBAMTB3NoaGpjaHgwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIB"
                    + "AQCmNgJ0FukDm7OtNf+E5oBCjz0usU8/1/pBFTzOy1dRGKm2ubmhlh6MX03JnDjObV20tb"
                    + "nYOOGeztVdg6hAjTW/PyEJHGG6IfekfbDHevkzya1kw6fSbpwJWIyr2te7VVsaqmNXoGPE"
                    + "BqvbtVgYuZcM9Fu3jM28i2XLap8zb8kzA+CD4L/JyrItMCcpDNWLn8zmSfInNJDaeGhFRF"
                    + "vPL/cIrtCJBcWzOpH5XQ2q1+jbzpKa5uVsTRbfhhNNmEHnokDi+0Ob58G8wqwjCu0TJiqy"
                    + "OzYbRzPwfe+vK8rvP5t1ggoQKwzdHvfAc1qIun+T6aTgNmBtwXZG9kfEkwHlo98/AgMBAA"
                    + "GjITAfMB0GA1UdDgQWBBR1S5fnWaty/USfSpiHoQnX5JGmeDANBgkqhkiG9w0BAQsFAAOC"
                    + "AQEAImkottskPka2FWxKYhmN5Nc2bY+Sqye8gTn1kUAAtHN9rXvjdtIUhYoQEjjr7u/8vM"
                    + "bXmHf38IYDKXL3SnFsyISv8afIWVLvwJeX0cTSKi8nX4OSjznXrn1CPxrHosxBlPLl1Hwq"
                    + "0lyIGSpGliPIyyHotJKFGrchRItB1rpX0Gq8lEmv/E7fir+eg/UXWXtcfG2Wmv9HR8uDrT"
                    + "yHlheM45Uu3pieX0aYw9XWrjUrfdNOmYSEcQ7G/0k8ERYOwOv196hPewwNhg4M4OBT9hxa"
                    + "DYvkptK+bQVdR2xHtOdHmWSikzhGhNdJ/g9epn4qbw6fOb36762Y7Qlup3rDBjjRQQ==";

    /** 掌门下山 TapTap 原版（MD5=d35139f6… CN=littlegame） */
    private static final String ZMXS_PKG = "com.yofijoy.zmxs";
    private static final String ZMXS_CERT_B64 =
            "MIICYTCCAcqgAwIBAgIEVFx45DANBgkqhkiG9w0BAQUFADB0MREwDwYDVQQGEwhz"
                    + "aGFuZ2hhaTERMA8GA1UECBMIc2hhbmdoYWkxETAPBgNVBAcTCHNoYW5naGFpMREw"
                    + "DwYDVQQKEwhzaGFuZ2hhaTERMA8GA1UECxMIc2hhbmdoYWkxEzARBgNVBAMTCmxp"
                    + "dHRsZWdhbWUwIBcNMTQxMTA3MDc0NjQ0WhgPMzAxNDAzMTAwNzQ2NDRaMHQxETAP"
                    + "BgNVBAYTCHNoYW5naGFpMREwDwYDVQQIEwhzaGFuZ2hhaTERMA8GA1UEBxMIc2hh"
                    + "bmdoYWkxETAPBgNVBAoTCHNoYW5naGFpMREwDwYDVQQLEwhzaGFuZ2hhaTETMBEG"
                    + "A1UEAxMKbGl0dGxlZ2FtZTCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEAkwcB"
                    + "XhGBqXgFN7pYcInClKvWuLhcR0pV5JE3bFCPrIrO16Sx3AMFWVUXOk8JlTOxz0M4"
                    + "OYoavso1Zs86kGIc59GTxJU1f5sZm58A93OnAbnBACFdgQdaAWOHONPPbiDGSHMG"
                    + "PWjQhiKQF7DDZ+yihvE8FYXry/ZNEzz84JUcwB8CAwEAATANBgkqhkiG9w0BAQUF"
                    + "AAOBgQAeLLQowNCoJ1Zd4st7zrvR2WivN/i8XxesUbbRO+lE+qeeULV9I+8ha/4p"
                    + "qL2aDjDlxuvLprTeU+AZjrhIoecoRzD1x/ZjEkhI9ng+ByybsrFX8wAWPHSM1w4o"
                    + "UDDW7/TiYEb3xuEoH9l72TTM0rc6o+XZxJ6ztfHa/f4OJCK56g==";

    /** 王国大作战：前线 TapTap 原版（SHA256=57bffb7c… CN=dhsw.keystore） */
    private static final String WGDZZ_PKG = "com.xr.wgdzztap";
    private static final String WGDZZ_CERT_B64 =
            "MIIDyQYJKoZIhvcNAQcCoIIDujCCA7YCAQExDzANBglghkgBZQMEAgEFADALBgkq"
                    + "hkiG9w0BBwGgggJwMIICbDCCAdWgAwIBAgIIJ9AvBgzQtykwDQYJKoZIhvcNAQEL"
                    + "BQAwZzELMAkGA1UEBhMCODYxEjAQBgNVBAgMCeW5v+S4nOecgTESMBAGA1UEBwwJ"
                    + "5rex5Zyz5biCMQswCQYDVQQKEwJJVDELMAkGA1UECxMCQkMxFjAUBgNVBAMTDWRo"
                    + "c3cua2V5c3RvcmUwHhcNMjUxMDEwMDIwNjI0WhcNMjgwNzA1MDIwNjI0WjBnMQsw"
                    + "CQYDVQQGEwI4NjESMBAGA1UECAwJ5bm/5Lic55yBMRIwEAYDVQQHDAnmt7HlnLPl"
                    + "uIIxCzAJBgNVBAoTAklUMQswCQYDVQQLEwJCQzEWMBQGA1UEAxMNZGhzdy5rZXlz"
                    + "dG9yZTCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEAjDil3p4rvh3/uRTlCxRr"
                    + "3dZSyz4CbPBAEH8WxD0JBl1eRXgK29TpxwcBwVP5VidybavjAeT5okWlQMr4CMpi"
                    + "aBZ2VGcKQezbYV+eh95/LorfDYnqnttu40RGA59HmxMBaFnrKfHIbufPBfFRycRa"
                    + "vC1TwIWUwlcIE+BjblER648CAwEAAaMhMB8wHQYDVR0OBBYEFDkrlf8Pyrp0PCgP"
                    + "RDvVZJv9ZCJiMA0GCSqGSIb3DQEBCwUAA4GBAHsJ4S338q0O/ysEoUzJ9bWBY0bs"
                    + "LEsqBu4ls3wrK1iDTajBfTeFvRoKSz+DrbIouydGQhVkwFCNmerMdcqZyGPNoCCL"
                    + "MhOju9UT8D6fyrH3Saq29wGMll3NhHbosC4fVG3uxDC6/CGlaYiej3mrB+7k0nYj"
                    + "LnA7ZKmWClok2P6DMYIBHTCCARkCAQEwczBnMQswCQYDVQQGEwI4NjESMBAGA1UE"
                    + "CAwJ5bm/5Lic55yBMRIwEAYDVQQHDAnmt7HlnLPluIIxCzAJBgNVBAoTAklUMQsw"
                    + "CQYDVQQLEwJCQzEWMBQGA1UEAxMNZGhzdy5rZXlzdG9yZQIIJ9AvBgzQtykwDQYJ"
                    + "YIZIAWUDBAIBBQAwDQYJKoZIhvcNAQEBBQAEgYBZMpkoOSkPd6WDSng1z5SAqCxG"
                    + "VZgtnCSofgeQxS3Zal3SXson49AFGJgF1JmR+lD8hbjbyCEjrncU9UgYplbpPbJk"
                    + "LQ7E9imazMHh42Zy6Oc9QEyLaSFsooPOCmwyaA/yY7IT/4BBLhCbOKsoGPx7kGMX"
                    + "pu18KgejxDlixORySw==";

    /** 伯吉的温馨厨房 TapTap 原版（MD5=e28a5f390ab1a7590a032f05b3fde72a CN=Morefuntek Inc） */
    private static final String BJDW_PKG = "com.hnyg.bjdwxcf.topon";
    private static final String BJDW_CERT_B64 =
            "MIIDlTCCAn2gAwIBAgIEXMfwgzANBgkqhkiG9w0BAQsFADB6MQswCQYDVQQGEwJDTjERMA8GA1UE"
                    + "CBMIU2hhbmdoYWkxETAPBgNVBAcTCFNoYW5naGFpMRcwFQYDVQQKEw5Nb3JlZnVudGVrIEluYzEX"
                    + "MBUGA1UECxMOTW9yZWZ1bnRlayBJbmMxEzARBgNVBAMTCk1vcmVmdW50ZWswIBcNMTUwMTA2MTc1"
                    + "OTE2WhgPMjA2OTEwMDkxNzU5MTZaMHoxCzAJBgNVBAYTAkNOMREwDwYDVQQIEwhTaGFuZ2hhaTER"
                    + "MA8GA1UEBxMIU2hhbmdoYWkxFzAVBgNVBAoTDk1vcmVmdW50ZWsgSW5jMRcwFQYDVQQLEw5Nb3Jl"
                    + "ZnVudGVrIEluYzETMBEGA1UEAxMKTW9yZWZ1bnRlazCCASIwDQYJKoZIhvcNAQEBBQADggEPADCC"
                    + "AQoCggEBAJkTDcjfXLhfQp8nS2J8EpcN1zTCtrb2mvH7IrQ6yT+ltutFYk6Fzl9VThtryBmKasqa"
                    + "zx2uT0t9eAF429kPhE2IT2VuJONFAoC5NyWtKlg9LF/acY+bXPumjqzS3kb6bi2WqRDoBnzBLlsN"
                    + "F9P+vEHguHLQrkyA6gZEfQm48voIpm2Z28yXwE2PnJHk4nbROFiHz5jRYdXlEfKeMKlBKD4sBcCH"
                    + "THnWhsqEaJI/tqp3NlfyrNrq2gIaAIKKxWh8jtpoCeUby/QGFAfagLICqx5w2n7Gft352yt4vSr8"
                    + "KfpexYQ+Jvsb3Qa17UTqFpMSxMErZCbqodv9KVfYjH3GyPkCAwEAAaMhMB8wHQYDVR0OBBYEFLDm"
                    + "JXKgqKCZO51E7bNvVCfgtQgqMA0GCSqGSIb3DQEBCwUAA4IBAQAb5+ytwT5GTt/UF5D6cQpAdcnP"
                    + "DqoW7S9QCyCdcIFG8tKQQoP0lsFR/KbLZUOBQHGgtvdu35cMLhbfiZ8WkYt1j2kCdBvUorV46v4F"
                    + "82FkYzWJCoxhg0yDyqgZWIvTfUM5vDdJCzdRhKGA9k6BJRDkF+DLVhNojXSXCLIIF40rOUzHF287"
                    + "KY7+qCogIl32WiXWytNSoft+y2j7TwLQeYCnutugiC0QKuCtShylN6iz9BnlzLqPVAb71fws2lQF"
                    + "7Wxecw/VMHVCxkraXJbpSoXl4FNVxCxjPSNQBHkdcKQZyfvNHWw+ox9J46wk3X2tgcGZ+oPnFD0X"
                    + "Q2o7dx6l2uqw";

    /** 无悔华夏快爆原版 CN=SongYang, O=SYYX（SHA1=01:89:64:F0:5D:8B:B0:A8:84:0D:0D:D3:99:14:E9:70:16:0E:4A:1B） */
    private static final String WHRHXKB_PKG = "com.syyx.whrhx";
    /** 无悔华夏 4399 原版，同一 SongYang/SYYX 证书（SHA1=01:89:64:F0:5D:8B:B0:A8:84:0D:0D:D3:99:14:E9:70:16:0E:4A:1B） */
    private static final String WHRHX4399_PKG = "com.yhhy.whrhx.m4399";
    private static final String WHRHXKB_CERT_B64 =
            "MIIDTDCCAjSgAwIBAgIEHjyZ9jANBgkqhkiG9w0BAQUFADBnMREwDwYDVQQDDAhTb25nWWFuZzEP"
                    + "MA0GA1UECwwGRnVuTGFiMQ0wCwYDVQQKDARTWVlYMREwDwYDVQQHDAhTaGVuWmhlbjESMBAGA1UE"
                    + "CAwJR3VhbmdEb25nMQswCQYDVQQGEwJDTjAgFw0yMDA0MDQxMTI4MTRaGA8yMDcwMDMyMzExMjgx"
                    + "NFowZzERMA8GA1UEAwwIU29uZ1lhbmcxDzANBgNVBAsMBkZ1bkxhYjENMAsGA1UECgwEU1lZWDER"
                    + "MA8GA1UEBwwIU2hlblpoZW4xEjAQBgNVBAgMCUd1YW5nRG9uZzELMAkGA1UEBhMCQ04wggEiMA0G"
                    + "CSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQC8GVDYORP60iTlmuc4gSe15/M2YGAknhBIkQUeXk5W"
                    + "fDDkD73phChtzpggTcb5eGNwOht4s9XrWleXO6PrT9nYu1XG6pxBA5U3s1Q7YzmX8WpNsjHFoUPX"
                    + "KHicstqS4Kpr710QVGMj0vg5EdQuh9EWTUv/ix0VxcrVCI1819486soNwzvd77YYjrx2MmzX0t1w"
                    + "zC9REMGTf2d5xytnnX5qfgaszTwSjFS7fDF1v3pMBerfHaa1dhgzkqZKmQELoS0ec+2eSOXa/yJx"
                    + "d56hssankh8hx7FB77V9tZ6/DgEIOtnnViampHQQm7q/Dm1lQsjM+4/A9btEvt03HxqH/HcjAgMB"
                    + "AAEwDQYJKoZIhvcNAQEFBQADggEBADEmHElqov0V5hj1xTVS8JmYRyJyGJHRqZpLy0zhqjRj5Gcv"
                    + "DucO2yNuOkzKjA70njRItIS41RRgA9QOyNkZzyr7IH9Ix/LABHmdC1JBWa+9KPIlzn1kpMMXhZnA"
                    + "vmMYERL8nlW0NZtzjB9s5roJtY49Q0hAgbB0L19cLQuL/iZkTKVAsRioQQq7EuKh2QMsiYwXSlAI"
                    + "7d1qZUDdV4JdwDoB7YKa1uJPSGrwINinGl0lP00TJ/x3Dy1+n7I5nUkpO2JplrrsDfI1cdwD6Q1f"
                    + "uMRUH0tiPKpXkjrmsinx/gMTW34+RdPT0wmUHcdNRBG1kXaP3ui8YpZ011lWNYhTns4=";

    /** 我嘎嘎乱杀 TapTap 原版（MD5=ec6d336b… CN=Zhou Jianfeng） */
    private static final String WGGLS_PKG = "com.sixag.gaga";
    private static final String WGGLS_CERT_B64 =
            "MIIDYDCCAkigAwIBAgIEP8ESjTANBgkqhkiG9w0BAQUFADBxMRYwFAYDVQQDDA1aaG91IEppYW5m"
                    + "ZW5nMRQwEgYDVQQLDAtBRyBTaXggR2FtZTEMMAoGA1UECgwDQUc2MRIwEAYDVQQHDAlIYW5nIFpo"
                    + "b3UxEjAQBgNVBAgMCVpoZSBKaWFuZzELMAkGA1UEBhMCODYwIBcNMjQwMjAxMTM0NTI1WhgPMjA3"
                    + "NDAxMTkxMzQ1MjVaMHExFjAUBgNVBAMMDVpob3UgSmlhbmZlbmcxFDASBgNVBAsMC0FHIFNpeCBH"
                    + "YW1lMQwwCgYDVQQKDANBRzYxEjAQBgNVBAcMCUhhbmcgWmhvdTESMBAGA1UECAwJWmhlIEppYW5n"
                    + "MQswCQYDVQQGEwI4NjCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAJU9AE6yKZO5NMne"
                    + "AzXGUm2PygvpZoNfsyBvOp+a2nNh67tYlLBYyXt6Y218zY5jf6F9LWsM9V/E0MRkadHyj5lMcg52"
                    + "Kc4XkXmr6Metc7FO8bWB4ar/js7nLeTiC4TSCW+sy7aHtdtCqMqqMcyaOYuAn7TWCBUL1xiUqilY"
                    + "hHBNooM/5BcQ7uyH38zs94P4l65HBrqUVgERm/BdCzpXBKxMJUVFcSwmouSpnRal+AETI+EInDAn"
                    + "7zR+OQlvf9MEz5sc5Wa34a0/g/uBibCJE1B3j5eWzdcsQ5ErbhsueYQ/27wS4pfVbgot/YO9mGVw"
                    + "ne+RDwDn9qXJLvK6nI+l830CAwEAATANBgkqhkiG9w0BAQUFAAOCAQEALOgc2b+tpUvMDI7bJ56/"
                    + "3Ojr/MOUiLvPoMbuLenoSILypWYpeAhc5f4Lpcm2FUT73A/OeaiIubzVrJ8H/P+dN2ST66+Bf4W1"
                    + "md4VhV8K2kDKbX2/+VQwbMPhDbetR2mZd4pwM0ItgClYS3JzuLKVU39Mh5BTcOieLTT2kqLr5uQH"
                    + "z2WFS6b3f6C+v2ciD8Ligw4P5a7HBKZUJmarzDj5yjk1NNF3LYnFU+KQVp8HRrWuvY0LoB/4K5I1"
                    + "Zzc8rDS/gBScWz5mW0VCTOhRKd5zb10Q6sa0c67aMv/GO73TAUGQe1qBKYOlFUry9MO9pyUAwthl"
                    + "3n0+/YLuKsc//3zi7g==";

    /** 终末地 1.5.3 官方（MD5=1cf808541a17bf218d3dd5a30787e485 CN=HyperGryph） */
    private static final String ENDFIELD_PKG = "com.hypergryph.endfield";
    private static final String ENDFIELD_CERT_B64 =
            "MIIDhTCCAm2gAwIBAgIEBKeDuDANBgkqhkiG9w0BAQsFADByMQswCQYDVQQGEwI4NjERMA8GA1UECBMIU2hhbmdoYWkxETAPBgNVBAcTCFNoYW5naGFpMRMwEQYDVQQKEwpIeXBlckdyeXBoMRMwEQYDVQQLEwpIeXBlckdyeXBoMRMwEQYDVQQDEwpIeXBlckdyeXBoMCAXDTIzMDkxMTA5MzI0NVoYDzIwODMwODI3MDkzMjQ1WjByMQswCQYDVQQGEwI4NjERMA8GA1UECBMIU2hhbmdoYWkxETAPBgNVBAcTCFNoYW5naGFpMRMwEQYDVQQKEwpIeXBlckdyeXBoMRMwEQYDVQQLEwpIeXBlckdyeXBoMRMwEQYDVQQDEwpIeXBlckdyeXBoMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAjhfxXW0AZtIoO2f0EiJPYq5+hDCAXod0VPKUFuB5E/TnHSzWWWM+isOC89BXSZyKoin79f2N37K7HmqXw6WD2GHiuTpjw7G6A3IgJz6ofr/vC0oqTrC10Iut1HEjm59IWm/1bzqgTuPF2Jc03KjU5Gj2JecmoFfyBnAs1vEx+E0+w9hLg4hW51gxcn1c1x926LRj1eEH5+ahcRIxRkAebOFYvzl7r1OXsCtACX5xxjhg4/TOwix5AND1Aj1Ws3IiPv8j2Qzp2+cqVKno3b0w/sL9b2pGt4XDIpnWpYrm822qtn44xxSj0hO2YjO/RQNc/NuiZcIzT6Gn2zPO2S7C1QIDAQABoyEwHzAdBgNVHQ4EFgQUwd8Bs9+71uU94Hv7CRRK5LJaxlQwDQYJKoZIhvcNAQELBQADggEBAB45RgOdavnoEkdfybCvA7wbwaizxw0AZEjwTy96p7eg5eR+b+GKqXUaBeR0Lm5VJf50TUER9Gd1pUbglAmz5adkmP7i5mhczZuiFThxbliz98QqZmpgrT2NezqHYhYiFrZWqW9nPd4cvE/4m3F/THh7thrEcJu1sJIffEnKbpxymLt6nhH/aYxl/ABphbFkfaEs0DY7ciYDzjImV06K2tw8Cgvrc0f4N4mDSX1M92tFa97xtSmPN44XxVdAditZHPAoWFy3FkXBQw6tDLgMdmeCz5UN+NAC5FB/nmiFza/Vk4/KAel1yclAh5ag+mw2iTeRC3lbHvoBh12jKEQroi8=";

    /** 超维进化 原版 CN=elliot lee（SHA256=4a91bad8… / MD5=da998108…，v2 块 DER 导出 2026-09-05） */
    private static final String CWJH_PKG = "com.grinning.beta.yszl";
    private static final String CWJH_CERT_B64 =
            "MIIDfzCCAmegAwIBAgIEIGg76jANBgkqhkiG9w0BAQsFADBwMQswCQYDVQQGEwI4NjESMBAGA1UE"
                    + "CBMJR3Vhbmdkb25nMRIwEAYDVQQHEwlHdWFuZ3pob3UxETAPBgNVBAoTCFBlcnNvbmFsMREwDwYD"
                    + "VQQLEwhQZXJzb25hbDETMBEGA1UEAxMKZWxsaW90IGxlZTAeFw0yMjA1MTMwNzU2NDFaFw00NzA1"
                    + "MDcwNzU2NDFaMHAxCzAJBgNVBAYTAjg2MRIwEAYDVQQIEwlHdWFuZ2RvbmcxEjAQBgNVBAcTCUd1"
                    + "YW5nemhvdTERMA8GA1UEChMIUGVyc29uYWwxETAPBgNVBAsTCFBlcnNvbmFsMRMwEQYDVQQDEwpl"
                    + "bGxpb3QgbGVlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEArBnIb4wAQYhHHivp8j7K"
                    + "JFPHsTojU3RmLNUTSMJvZm90U7/v0/qsQMt+zd495bY+eqlYAnHlgej/JXe0V4pTEbOgPY8iw+PG"
                    + "rwm/fgsV4MmFYjaSWCHSl/WwsIvAq/CP5eogNGiFdeCGI2g9/Uj9dCNMSHp0SNkS6lNOW3RN3bhX"
                    + "Q8t6ay4v5E21PjSLaWDbFlfCf9YmYOvqckHKftjmai66++kW+ly9GF4Y4BalYgcs4uFcTJqpoY0d"
                    + "js0wQLRgWbHT1ngxbgureCAohUbYVAAa5Yztvg8hmX6ntM0yPnAs4OqLvrAW0Baep0scGVee1rT7"
                    + "GbgXorLQu0WW37ATZwIDAQABoyEwHzAdBgNVHQ4EFgQUmT+AAGIxyRKSQPMcskalA9wnMsIwDQYJ"
                    + "KoZIhvcNAQELBQADggEBABMpSLSS2edI3HKtbb7GAyqLw2l7yd8egrxM4jfEHB0xqB3w6pP7Yjts"
                    + "uxYNpXzqAyO5bMWmM3m7k4fyiDryQKItTdHdCryTlw2uy2ZmPWBUAiZxH8N4liuVqMEXk3tOjDlD"
                    + "5/B/nXYPXQwhg5yePMR7hmQLMxFf+ALXKG9L+obbT1vBkHvu/LxuP4dOjXyvBaiWi31UKmPOAfRG"
                    + "YZ2GaWP7dDENtq7HUiJ/ir4DV+lykHYHlofTJsNleOuVa5FUZpBK/NsfVL8/54SaCKuqx6jdjES5"
                    + "y34bYtNqY3CbT3xrfqEpeCjZx4xh/gex6afKOESZNiRxQb/fu2cWadNdfK0=";

    /** 我的勇者 原版 CN=rn（MD5=60908df9… / SHA-256=ce7afa48…，2026-09-07 官方 7.6.0 导出） */
    private static final String WDYZ_PKG = "com.rsg.wdyz";
    private static final String WDYZ_CERT_B64 =
            "MIIDXTCCAkWgAwIBAgIEDZfh2zANBgkqhkiG9w0BAQsFADBeMQswCQYDVQQGEwJDTjETMBEGA1UE"
                    + "CBMKR3VhbmcgRG9uZzETMBEGA1UEBxMKR3VhbmcgRG9uZzELMAkGA1UEChMCcm4xCzAJBgNVBAsT"
                    + "AnJuMQswCQYDVQQDEwJybjAgFw0xODA1MTYwMjUxNTlaGA8yMDY4MDUwMzAyNTE1OVowXjELMAkG"
                    + "A1UEBhMCQ04xEzARBgNVBAgTCkd1YW5nIERvbmcxEzARBgNVBAcTCkd1YW5nIERvbmcxCzAJBgNV"
                    + "BAoTAnJuMQswCQYDVQQLEwJybjELMAkGA1UEAxMCcm4wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAw"
                    + "ggEKAoIBAQC5rArUw0zbJo6dFuVBLWFYG/HPMTyTj9U7TsdKh4agKzHAPNbnW0MJweAvvS0on22y"
                    + "eeAEdzn/h+FjFaxJXHQwrXHHl7fGkZGJtzBcr/JKjPPJFE5UJYTH3pFysq7w5+x/dE8M3RL28MUf"
                    + "N3i1trvR3IJ6DIuZ5ZMli0CJ9YZ/NFFVcl2xky/WCYeFktAYdSzzt3fTduiDehr4twOAfSHZiITO"
                    + "bKJCkoSE0ITAOd61VmTtoqorSebhJ4ychIMFwPRxQsNVzo7zFFdQ10Fs10Jrs0LqnB1fbgtreDqD"
                    + "1Noru61S/iuRCP+Sgf1BTU09McgMKWPWHrL7/FtCld/S0lm3AgMBAAGjITAfMB0GA1UdDgQWBBR9"
                    + "yWRMQgYaJ8Xad3Nk4hTV/FYoAzANBgkqhkiG9w0BAQsFAAOCAQEAbJPFZXui9kcQkdxW8e/51XSO"
                    + "Ex+trLTL0cHZp+PKWzxMRIGvBwoAKEv/1BzuGt+moSDbN55TuonWI+4x9AxrKiyM4oI+kvWz7SgU"
                    + "8xmfQY+FnJulE/wUk+6exJ78toGHSEuk6WvDbnaT9tryavC3djCmikYkraXTYuFyT3+321BcI+kx"
                    + "d50UC6lp2pYiKl6UZQrrqtm1US47JvVMHTnrzuD93bA1GpMu/oS/wc38pMisXhRoUfNnJnibUW6v"
                    + "sQ8iqkqU+5BPg69+KD5qvwQPQVoip5wXQxbvOVHXS+1DaPkp3egVQMKXkM9V0PJT4VW74Oi2UztP"
                    + "QyjF5lcMIvd2zQ==";

    /** 遗弃之地 原版 CN=hortor（MD5=78f6bcb5… / SHA-256=47dee5a0…，2026-09-07 官方 0.1.57 v2 块导出） */
    private static final String YQZD_PKG = "com.hortor.yqzd";
    private static final String YQZD_CERT_B64 =
            "MIIDQjCCAioCAQEwDQYJKoZIhvcNAQELBQAwZjEPMA0GA1UEAwwGaG9ydG9yMQ8wDQYDVQQLDAZob3J0b3IxDzANBgNVBAoMBmhvcnRvcjEPMA0GA1UEBwwGaG9ydG9yMQ8wDQYDVQQIDAZob3J0b3IxDzANBgNVBAYTBmhvcnRvcjAgFw0yNTA0MDIwNjA2MjBaGA8yMTUwMDMwMzA2MDYyMFowZjEPMA0GA1UEAwwGaG9ydG9yMQ8wDQYDVQQLDAZob3J0b3IxDzANBgNVBAoMBmhvcnRvcjEPMA0GA1UEBwwGaG9ydG9yMQ8wDQYDVQQIDAZob3J0b3IxDzANBgNVBAYTBmhvcnRvcjCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAMT9Ctm4CePlyXG76del3o4QMiKmJ3fooEcJ4BTFCmY17/MqOjw4L+o9rxghho44XcBAMxsRgSe5td1uL12BN0yoFSSZ7w9HgvCw4A2FA9VCzAb2J2qQBIK4EdzmxLfTCRcFwe2/CvrtWQ/yvGwHciCHCCjWUGsAnF7nux8JZiYjEWliRi/6GKG4Ck1IyeURd8EIb1sQTNMMTzONu/jVxbvNCXtkhrLCiyvlzj6Eze37ucQBlgDfkIiEsX0XhGxaOYtumHD4+av0vNnzqNQ4vR6INTqFkcQDH8ygIEIcJ0RrpK0ZV1vZ5xwU4DNuRjHmH9/gR+STrJznS+gfDH3QEGUCAwEAATANBgkqhkiG9w0BAQsFAAOCAQEANzkqCKDV7qsMP6NmzNfvRNglSNKQyUDZCy799fq+C7ESTMuzlohRNuMxn3S5DftydRSLSs+y2zaKnxmDD88uGpvYqkyg3Ainno8rnA2DdO6k7eI3ncN/hyKUEtnQ7i5o7sdcVLuxz7qFnIV1vXRY85ICTMcf1mEdjkKGkB/Y8r97BFLaAnTXX1adVXA7J9nBk5kD7GXIcho6DII3FGXhvVz6IVHji51wVXNIlMa3TgAl2h/OvBw03nkMNp6B1rtDt3POdqGCCUNLpcGo2Tb3EPZfNl26AHZyWafM/npjCktsPRcRbqzUNL1gWa5usruPHpyUo/r3tHZdKS7X8gYo2g==";

    /** 迷雾大陆 原版 CN=1（SHA256=67cf2df1… / SHA1=f8676b8c…，2026-09-09 官方 14.0.0 v2 块导出） */
    private static final String MWDL_PKG = "com.hortor.mwdl";
    private static final String MWDL_CERT_B64 =
            "MIIDBjCCAe4CAQEwDQYJKoZIhvcNAQELBQAwSDEKMAgGA1UEAwwBMTEKMAgGA1UECwwBMTEKMAgGA1UECgwBMTEKMAgGA1UEBwwBMTEKMAgGA1UECAwBMTEKMAgGA1UEBhMBMTAgFw0yNDA0MTAwNzQ5NDRaGA8yMTQ5MDMxMTA3NDk0NFowSDEKMAgGA1UEAwwBMTEKMAgGA1UECwwBMTEKMAgGA1UECgwBMTEKMAgGA1UEBwwBMTEKMAgGA1UECAwBMTEKMAgGA1UEBhMBMTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAKyjMicjFXHP4otqSyTI8xxSWydEEe4sDR0uqE6v/4/0VPvi1h/gjDQgNFWDUkMoj7lEM8Z6cHKr8nv779mmkC9d+BH8/gsHO8kYapvHEFudSOY9lVq2aYv3KbBidsAWxTw/sL5J5VyaQxufwCszxLJXMZN52nz88ucGMp+atxKX/jBEp1grlSpKU2BLXylBpMfgqAlXdmJED7aopTIL15nYBDTGnxURfyquqkDDSsntHbeaJ9JweaxSL8yfqUtU0C3qb1mW7N6E87nNy2gwxjcuNjN01q/3USIlu3fW5H+z50urzbEOaWi98wZTHkrqKASAxt7SxO8kXd3nQA8tCeMCAwEAATANBgkqhkiG9w0BAQsFAAOCAQEAhaxqfkDWoiqhvF2232BI4wVz5xFGbTDKwVOtvQnWhSTKfOVplRwTysa77KhBSFDUlMsqPesbNsl6ndXXcdIrOvmFcSt8ercOEvuvJajq/FmLXMfDFvHTMbSFD8dwXN0rtN09fD3S2ZfTijsPAzujToPfDOidePXcbUybY8WArjQEJTu+775SCj2LrlwC92zO772LpDtNA9/hheH78SWhMzHZEl6vVdfHzjSxv8hP9E+6FIJ42rVIWgXZnUkqtXSWy3wtZrV0p8aTdENA/ecCj18Fu4MsfhRqWV7UemHx+63xKLNOY1PVuivzxsNAyOFEt7kn26Tc6c0ZWQco2QFQow==";

    /** 无尽探险队 官方（play800/com.wjtxdtt.zy，CN=Unknown，2026-09-10 官方 26.1.5 v2 块导出） */
    private static final String WJTX_PKG = "com.wjtxdtt.zy";
    private static final String WJTX_CERT_B64 =
            "MIIDeTCCAmGgAwIBAgIEWeuT4DANBgkqhkiG9w0BAQsFADBsMRAwDgYDVQQGEwdVbmtub3duMRAwDgYDVQQIEwdVbmtub3duMRAwDgYDVQQHEwdVbmtub3duMRAwDgYDVQQKEwdVbmtub3duMRAwDgYDVQQLEwdVbmtub3duMRAwDgYDVQQDEwdVbmtub3duMCAXDTE2MDgwMTA3MzA0MFoYDzIwNzEwNTA1MDczMDQwWjBsMRAwDgYDVQQGEwdVbmtub3duMRAwDgYDVQQIEwdVbmtub3duMRAwDgYDVQQHEwdVbmtub3duMRAwDgYDVQQKEwdVbmtub3duMRAwDgYDVQQLEwdVbmtub3duMRAwDgYDVQQDEwdVbmtub3duMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAt/Gpgl30Qet19rZq+Or6JCp/OO8RnV21Wcmp0DhY88fXnNZcV5p+gsshLrxOzl8dqfXTmaYSreafuPzKplOH1jwuc2XI7NjcavgoIH1L4uYSiM8LQnyLV5K0DDjpXgS3sn9dyapPA9mvB1TSHLtICd25afCKha5AdBqUKT8NR+RVh7jEwVvl+YeeADUk0WH7gq2xtagSWaZSbvq8YgG1g3GZQA0tddZOaYvr/inr0caaa4uY/kv/bvzbk5vj8JN+q80m7sV6MKZi/WX1MCM65hpk9xXYnd6ltQ6fQXoh02zWZ75ex2KmsCdrJqCFUKOBcO95dDFOhNcY7qQo3/w/vQIDAQABoyEwHzAdBgNVHQ4EFgQUwz98PXv6yDLafGJQKUxNlCRBiFkwDQYJKoZIhvcNAQELBQADggEBAErJ5uQBX7az88TIf9ehS8QCD7enSHutpR+xySAhWcSblXBcFNdu76FbiESs02IubYP61zjnA4IOV1Lgq5nmpdseUiFkXdpa0BkSi3pc81w3yG/JCqKdpJvqjbuIeNpmeDh8Ycw2/4I/O7+ti/9bDaxKBcOmJlblWbIfTS91JpsDWPTNnbj7SNYmhmzO29Z8z04ycAfZeA4q8Scv27cm7scM7t1op6rgBav+OTA+Y/ENlsnSjxoVEwyDCxWbQs0G+luBf662cdGbOuWswcFhOGMVBzSP43xSVlV6ryy3hq4FAjsSMkwFElE+dmNr4gAC7S3+S+HLN2sFfN5ksuTpgJg=";

    /** 异世界勇者 官方（com.mudamuda.idlewow，CN=CocosCreator 官方自带调试证书，2026-09-11 官方 8.1.1 v1 块 DEBUG_KE.RSA 导出；SHA-256=447e4d41… / MD5=d77b4cfa…） */
    private static final String YSJYZ_PKG = "com.mudamuda.idlewow";
    private static final String YSJYZ_CERT_B64 =
            "MIIDdTCCAl2gAwIBAgIEHMBLxjANBgkqhkiG9w0BAQsFADBqMQswCQYDVQQGEwJjbjEPMA"
                    + "0GA1UECBMGZnVqaWFuMQ8wDQYDVQQHEwZ4aWFtZW4xEDAOBgNVBAoTB2NvY29zMmQxEDAO"
                    + "BgNVBAsTB2NvY29zMmQxFTATBgNVBAMTDENvY29zQ3JlYXRvcjAgFw0xNjAxMTkxMTQ2MT"
                    + "VaGA8zMDE1MDUyMjExNDYxNVowajELMAkGA1UEBhMCY24xDzANBgNVBAgTBmZ1amlhbjEP"
                    + "MA0GA1UEBxMGeGlhbWVuMRAwDgYDVQQKEwdjb2NvczJkMRAwDgYDVQQLEwdjb2NvczJkMR"
                    + "UwEwYDVQQDEwxDb2Nvc0NyZWF0b3IwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIB"
                    + "AQDM1VyEW5tQTFu6HWKWBZn68X8LI+FQVF2TwTuHTvdbUUgcgwHUP2BkQD4MVTZ3Pc3/4D"
                    + "pzG+Jj8fMIDtX9j92cO6i0QQ3gyqemq5KwRpt/Y3unOjEnyjNtqzMN4zU8EL2dH3zm74NI"
                    + "K4X9stw73ZjGMXypQsZlRA2Ehe8zdJ0G151E2FifRWHqxcPJjsJEHqCpfxrkdNgNEevQ5X"
                    + "t3A4pB/jYFMYLTBw6hkATZhaIy6p1YuXN0/wiKr7dI1oau0MZLhMRFCdfTEyTkHiBaaWEN"
                    + "g/It7D0shasNpKhWYtCaqKL6YPf/8Qfu/8en6f/MbqSMnE6dvpxtUcSxVJ+EAFUrAgMBAA"
                    + "GjITAfMB0GA1UdDgQWBBR5x1yCAuWsRb0nXzR2fxONbTQDiTANBgkqhkiG9w0BAQsFAAOC"
                    + "AQEAKlIYf7JRVu4ITi0JxZXZWAIkf6snHe6WkIj1ee9xPdCYyI3I1xG047TPn4INiEBD85"
                    + "l01/EWJi7csvbknCnivu8P2NBOjpHbMsQ26TEO3Ff5XPFKbRCV7LFL4u3NTmr9K0Rfu6c5"
                    + "ogSjqO8GZO4EtlqufjoeSfExRBg5pOUhI69vzGRD23wVD9HhCsZ3XCbOs61tvowNIRfq/6"
                    + "oUFjLTSDHnDyBn6pZayVwlSgHdDXlpW8jpSoKEKsTacODR4ZzMNlaOBIFC5UXlSQzk3BZh"
                    + "457goy3sLdVcXzjExqln40fjQShQOgvySmI/Pwh98UtjCAy9abeLF1cwbzUJeIampg==";

    /**
     * 凡尘西游修真记 TapTap 官方（com.qxjc.fcxyxzj，CN=sy）。
     * 2026-09-13 从官方 1.1.0 v1 块 META-INF/FCXYXZJ.RSA 导出；
     * SHA-256=45c0c06c… / SHA-1=4c8b869b… / MD5=7f381a30…。
     * 游戏侧 xxgame.GlobalGameFun.CheckSignature() 比对不符即 Application.Quit()（闪退），
     * 此条为 2026-09-13 首包闪退的根因修复。
     */
    private static final String FCXYXZJ_PKG = "com.qxjc.fcxyxzj";
    private static final String FCXYXZJ_CERT_B64 =
            "MIIDPTCCAiWgAwIBAgIEfoAJ7TANBgkqhkiG9w0BAQsFADBOMQswCQYDVQQGEwI4NjELMAkGA1UECBMCZ3oxCzAJBgNVBAcTAnF4"
                    + "MQswCQYDVQQKEwJqYzELMAkGA1UECxMCamMxCzAJBgNVBAMTAnN5MCAXDTI2MDYyOTAzNTMxM1oYDzIxMjYwNjA1MDM1MzEzWjBO"
                    + "MQswCQYDVQQGEwI4NjELMAkGA1UECBMCZ3oxCzAJBgNVBAcTAnF4MQswCQYDVQQKEwJqYzELMAkGA1UECxMCamMxCzAJBgNVBAMT"
                    + "AnN5MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAn17Cx82vAPyzlaph/irvL20ifQgnyvmfbn8Ed4wKucofwpmmKj5+"
                    + "PVnNjS2ksXjj+b1OrvZMRRYuOaFnmE5GOWvZSZ+98P5vqkBHHZ/ukQ19NLczGknrJq/36y3zHDSj+gHJEOuP1StOMJPWpoWfvRIq"
                    + "wwSIElVjSXcfMjjnR/rQjLMDhaFwObQ2a8rudnSID49FwbUK2xkYLxXauhvWbqwnBDl2fOut6weeA0gzijuEB9/OWanVoKR/9Oxg"
                    + "byEUjjGrzTnIZJQqc/0mK4uCUfI1ucQ8PLTkvtgVldw3TakFVXIsGJXKo013/8WwXKKAVufDSzlDBj3ZvfmH4zbLwQIDAQABoyEw"
                    + "HzAdBgNVHQ4EFgQUwYfUtDMUl04eIib5Nn+w99IUlzwwDQYJKoZIhvcNAQELBQADggEBAJRN1VZePt9ahW1sWhd3tEWfLdnf4xFm"
                    + "AaTlWwCgRfoXmQU9z4voYdkj4MpOoaoGg1XwrhJZNB3UWSKDXgh7nHfQuLdcJyzxDpVpAIHMgJ4Rz6uZzKIV6FR0rsjX8CFby/gY"
                    + "wb+F12cfpikMVy5CpoevjAe+ilGmhXwtrYTHzS4KE9HcI8/+YHpnMErXJp7VHEJVEuJoAp7qjOf0YBHF/KgHba5PysSAZ5ikRlLZ"
                    + "jYTpijUdkZJIMFlyiz3FggieEY4trcT/lcu8GZnm3ht1rhcs4v0UuCNHMExevaOPBtCqLXFM9pNPHedx6mHl4OSRr4FysGXgRGkB"
                    + "jmFhRR2ms2A=";

    /**
     * 我的修仙世界 TapTap 官方（com.lingsi.xiuxiandanji，O=YiKeYou）。
     * 2026-09-13 从官方 4.5 v2 签名块导出；SHA-256=d9e7f2e80a28cba7469bfcf24cc61d08c7919826b99fac2f0ae0019da4381a61。
     * TapTap SDK 4.7.0 与游戏侧签名读取均回灌这张官方证书。
     */
    private static final String WDXSJ_PKG = "com.lingsi.xiuxiandanji";
    private static final String WDXSJ_CERT_B64 =
            "MIICojCCAYqgAwIBAgIERKQwtjANBgkqhkiG9w0BAQUFADASMRAwDgYDVQQKDAdZaUtlWW91MCAXDTI1"
                    + "MDUwNzA1Mjc0NFoYDzIwNzUwNDI1MDUyNzQ0WjASMRAwDgYDVQQKDAdZaUtlWW91MIIBIjANBgkqhkiG"
                    + "9w0BAQEFAAOCAQ8AMIIBCgKCAQEAgutZYeGrsEaRHkelo8/LE2i9tnTwc/6nqwAYycJbnYpoTJTA4hLp"
                    + "QGd4UweyFdgx2GWy5yxyCjeYrCwy4SrZ1CNhNyuWBf7Yk+wUXCH+kPSFbWlPwJW4U3vMGLTSBrGPjUYV"
                    + "bUme/FpqX2y1s1ff9hWqoR6dZnCjZuKU6Mziqgi9CUYVo4SOT2NVw4GncgR8enT6TKYaYWr8AqshAH12"
                    + "EQ3a1W+J6IFFURKVUAFri/0J04ENsPh+f7SSPfqrDa3uklG/TvKiha/onbkwX6pnJGgobRdl+uoqvcmd"
                    + "UKrwlr/3PzlWWgE8A+RCC2etCH5ndjFg0/+EYZSCVpeP/8XBbwIDAQABMA0GCSqGSIb3DQEBBQUAA4IB"
                    + "AQAi4EG0LxmhL7fDycRS/bFEx78+/+W7nvnagTHlN//Wvw1RNsYbF2IeEFQlUm9gVtVje7xAAz0Oeop9"
                    + "kGPTYPFxYNZJOh/e9kJ7yVQBZmtf41/K2DJph5MEvQKsZaBMw/YCN5kx0iwpyHrGnv9eeBMh3dgm8KDJ"
                    + "w9cdloeYRnKt50dXfzcadbQzykM+rHQNft8IaDX9RlJzKpzGPSqA8hn0OJltuTFvYmtQkf3AuGuoAKJ+"
                    + "RX4Dr3xHfA1pUHO6iWekcXxrjM8ugjnIdIkflC+7NP3W1+67Ho3PQjbmPVHvtZUfyiBeTYyQ+hP2N8wn"
                    + "sMrfj3Ga7UHV0vc5jhPsLHtM";

    /** 大话蜀山 TapTap 原版（MD5=9ff0bc6e… / SHA256=7c006b97… CN=tom） */
    private static final String DHSS_PKG = "com.cylj.rogue.tp479";
    private static final String DHSS_CERT_B64 =
            "MIICuzCCAaOgAwIBAgIESn7z4DANBgkqhkiG9w0BAQsFADAOMQwwCgYDVQQDEwN0b20wHhcNMjIw"
                    + "NDExMDMzMzUzWhcNNDcwNDA1MDMzMzUzWjAOMQwwCgYDVQQDEwN0b20wggEiMA0GCSqGSIb3DQEB"
                    + "AQUAA4IBDwAwggEKAoIBAQDI0zDF80sLWvsz4c73StOfFmcDIMS6bR6+ezJd2fXto+7Yd1RjqUYX"
                    + "LIP+Xk/oiV+pFz6NnzRt2Ky4xQQ2cNKC6rPoxycMTExkKcUfK+bQUUUlD+0wpml72BFy6nAJvloV"
                    + "Laqa0G4mNIHFdMEIMOCXfkHfyHKoW/KeWKsFaMihkzaTsm0zZMcZ0DXm7VQH5iXITSLXwJDkCpYh"
                    + "i+VUZBTZasd5LMBdHvlFUL+eMz5Y+Dzk8L2b5pxB6fwp6/MhcTVeb05wana64p2SKeUwyKTMZ7cy"
                    + "NK8zuYeUpSzp482IGCcPWCoMyVHllPcsnosb+MRU9sbsay15jOt8m5jK8Oj/AgMBAAGjITAfMB0G"
                    + "A1UdDgQWBBRT7eeHmzvXUi1mVKoXHkgVmSCJ4jANBgkqhkiG9w0BAQsFAAOCAQEAGBujkha7cDiu"
                    + "rQ7MyuesoYpbcCGxE/nbU8w2cslVVkwVKpDKGTuHcAM6U2Q+GqgI9NThSTaMh7tKQ3QU30N859hh"
                    + "dJObwbUerGZLG8IIXRDm706dElnrQLm56lC5D+eBiXny9NEIxfjJtQf5aTD+bMM12wY1RZ8lny4I"
                    + "7GFH0PBcRlK67wpQPk06kc0SCHUOOSArDwHbfC6ZELMhI4nZe4bPnMrPbuwImBe+bHF/hzU/Ff1h"
                    + "f4yKq97bpS854J1/krhVCIpSkSZQxeI4PcKugDlYb5hoDv7hy4AKkf0uIHHvmBk8lLA6MJ67fog+"
                    + "c5YSVPmp7cWpwY2E22/gLKSLMA==";

    /** 正中靶心2 TapTap 官方（com.openew.monster，CN=elliot lee）。
     *  2026-09-14 从官方 1.7 v2 签名块导出；TapSignKit.getSign() 读取
     *  PackageManager.GET_SIGNATURES 的证书 DER 并取 MD5，必须回灌此证书。
     *  SHA-256=57fbc23b… / SHA-1=41580c1d… / MD5=b9c07db6… */
    private static final String ZZBX2_PKG = "com.openew.monster";
    private static final String ZZBX2_CERT_B64 =
            "MIIDgTCCAmmgAwIBAgIEN7EK0DANBgkqhkiG9w0BAQsFADBxMQswCQYDVQQGEwJnZDESMBAGA1UE"
                    + "CBMJR3VhbmdEb25nMRIwEAYDVQQHEwlHdWFuZ1pob3UxDzANBgNVBAoTBk9wZW5ldzEUMBIGA1UE"
                    + "CxMLT3BlbmV3LiBJbmMxEzARBgNVBAMTCmVsbGlvdCBsZWUwHhcNMTgxMDI2MTIxNTI0WhcNNDMx"
                    + "MDIwMTIxNTI0WjBxMQswCQYDVQQGEwJnZDESMBAGA1UECBMJR3VhbmdEb25nMRIwEAYDVQQHEwlH"
                    + "dWFuZ1pob3UxDzANBgNVBAoTBk9wZW5ldzEUMBIGA1UECxMLT3BlbmV3LiBJbmMxEzARBgNVBAMT"
                    + "CmVsbGlvdCBsZWUwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCiVRybhhVNPiXs0rTQ"
                    + "pmn7XiZH9EfM1x18tHBoisojAYczxk4PIiUb7AX2BNO4mvmbxaeqXaqsuT1WgWJ58lT1w+Pq5Kov"
                    + "41SHspXY0TawBrZRZZiGWovpj0ZRBJ3NKHxl09mJUqJOgAfiaIgwNLMEv3bdn72xcO9rj7sODaeX"
                    + "rm7MVNcDNIInuUlZMwwLYVnLada7LA8iFd3cLQ297GeUlub1AnuwqyYnJsWbRbTHAYaMeRTUWmwk"
                    + "X67kAVchuAkGGBln5UTbLGAObyn5lW4at1OuqTIa/lAbLjfPYKyNukSA16yQmAORFyz1yzewywjZ"
                    + "lwg2eXvzbyC0or0mqPWjAgMBAAGjITAfMB0GA1UdDgQWBBRsI3duLMZHct4hTsrtxYYtPQxtcDAN"
                    + "BgkqhkiG9w0BAQsFAAOCAQEAOIP4VM/w6ez4cFm0nRTwZb6Tq2ndyN4n7e2pUr5yR4Kcq/plesZL"
                    + "sy/+IFtLFqvnw+ZXYKy1k6omffcp7q1OWKfkt0QMnyzkH8pcnoMLnjmNUMSZUc6LaqnUaeuEKirD"
                    + "l0LrHY0wrrRGDecfCAmjkPgrxTG0uwqh+/ck1bhmeETu3zHA+sgi1UaLhjVwDCLFl4S8OSJduBzO"
                    + "CxKdIQMYP2gVHU287PPxnceQySjasjWkCUz+TRIWMhjAoJgA2G9fmYB+QhnSiFJqeOuwOFSyqzK2"
                    + "6IsI13A0d5J0GqKuONYYMH7gm26WCMGZHYDK1nVOAZlDl9n+znmTDHr7f5c6tA==";

    /** 一念通天 TapTap 720：从用户提供原包 v2 签名块提取，SHA-256=7b061a08…。
     *  改签后回灌此证书，供 TapSDK/TapSignKit 签名读取路径使用。 */
    private static final String YNTT_PKG = "com.qinglv.Immortal";
    private static final String YNTT_CERT_B64 =
            "MIIDUzCCAjugAwIBAgIEctnsBjANBgkqhkiG9w0BAQsFADBaMQ0wCwYDVQQGEwRkZW1vMQ0wCwYD"
                    + "VQQIEwRkZW1vMQ0wCwYDVQQHEwRkZW1vMQ0wCwYDVQQKEwRkZW1vMQ0wCwYDVQQLEwRkZW1vMQ0w"
                    + "CwYDVQQDEwRkZW1vMB4XDTIyMTAzMDA0MTkzMloXDTQ3MTAyNDA0MTkzMlowWjENMAsGA1UEBhME"
                    + "ZGVtbzENMAsGA1UECBMEZGVtbzENMAsGA1UEBxMEZGVtbzENMAsGA1UEChMEZGVtbzENMAsGA1UE"
                    + "CxMEZGVtbzENMAsGA1UEAxMEZGVtbzCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAK1F"
                    + "oOo9xk/ODgEvg8HvMOr1yweMz2ORK6q8KKGyvG2AHQqqT07U0BM+TMobBz6ozdoVVeTwfI3+J18S"
                    + "JSWfVQrVRxFUe7ByCLAnaAQsBG04RXtA5X49FPKQESwFfe56lxW9JGcHY28De3yHnNB3nz0hUzhR"
                    + "P89QGZg2/0oqqKQeMOtIdNiPT0VmKDFev0fNixIA23TxSKyMxImvuAXuUi4rj+S32ESLyDJiQN20"
                    + "Ms14GpSBoXX1nWCUKEblFg2otRzXoaj8U1aQ6gmMzAli6bVr1DCr4HoxwrTagvFOQJdaWvQrt9Jn"
                    + "ZTjdiG+UzzSteH1hN+nguUlAlHySAKgXOIkCAwEAAaMhMB8wHQYDVR0OBBYEFA/xBG0z57PxLJ+V"
                    + "yE7Nn4xDgxPLMA0GCSqGSIb3DQEBCwUAA4IBAQBmDoeaycURdJ2BslCt4fUPpg7UAY3UuHj/jj+L"
                    + "kd91TTps9SgDOIw1Y6MulxxMufhNmx6Ua/tB1I2PUFThO/68QWfDoIcBac7ef3CfrMbHyons6HiD"
                    + "4Kty3YYu24S/VjjedveiIPtRKs3EtvFaa3KLZkF7qakfdJjgBisnMEW3utpcDQRCtGs2I2ngdSrI"
                    + "0RDKvwaCxqlAqBhlp3ry50uZP1+znfezJxC8SjrqHtFF7d2aJwWR/Jofrey5Kkeg7cFDclojBGDJ"
                    + "aOXctA8+fFx9hw4Bk4KRFF5nOL3Cdtf4aSgpnXlxRFQ4LhMogmjly4wJiSNajEce15pdF4B9dZR2";

    /** 偶像梦幻祭2 TapTap 官方（com.happyelements.es2，CN=boy2，SHA-256=60fd8332…）。
     *  2026-09-15 从官方 4.5.8861 的 KEY0.RSA / v2 签名块导出，供 TapSDK 签名读取回灌。 */
    private static final String ES2_PKG = "com.happyelements.es2";
    private static final String ES2_CERT_B64 =
            "MIIDgTCCAmmgAwIBAgIEAxZhTzANBgkqhkiG9w0BAQsFADBxMQswCQYDVQQGEwJDTjEPMA0GA1UECAwG5YyX5LqsMQ8wDQYDVQQH"
                    + "DAbljJfkuqwxFzAVBgNVBAoTDkhhcHB5IEVsZW1lbnRzMRcwFQYDVQQLEw5IYXBweSBFbGVtZW50czEOMAwGA1UEAxMFYm95czIw"
                    + "HhcNMjAwMTEzMTEwMjE4WhcNNDUwMTA2MTEwMjE4WjBxMQswCQYDVQQGEwJDTjEPMA0GA1UECAwG5YyX5LqsMQ8wDQYDVQQHDAbl"
                    + "jJfkuqwxFzAVBgNVBAoTDkhhcHB5IEVsZW1lbnRzMRcwFQYDVQQLEw5IYXBweSBFbGVtZW50czEOMAwGA1UEAxMFYm95czIwggEi"
                    + "MA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCPO3NQj9evCPYR3NoGkz2H/7wUxUIunnyUzWZ5FOh8e3izdM+YKf02UDECS7BL"
                    + "NFJTcEQDN2nHTxg+yAQ+DXuqMpLIv5y8lJtjLAb+HT/ZvVV89jMpu54wpV4fWeHZ+s3HDLzC2oLt/OjlIu38BulsJBJHqy/H8UOk"
                    + "hBex119GSC2EAKGuXjZjWlTSbZgZDbZZBTKA+FlniE8YYzwnK+op7IsZ8laGJ3dcR/dTPutrpRpEtDGu7QQy/lM63VlV7EuqWCEw"
                    + "oJnxH2n63i+8oR5KNlcGTdaeYy7sW0MK5DiFL2VEQ17CzRr3jBnjdaCbcQ81PAQ/Q9FU+/vtvvBntsFTAgMBAAGjITAfMB0GA1Ud"
                    + "DgQWBBRVoQcgLv2VlvK/hUSPE1n4fawTnTANBgkqhkiG9w0BAQsFAAOCAQEASE1qLRN74kWPEJwaISTIAklHHEalRWudqEBFQOJk"
                    + "sKuLSEd3HY08+g7d9+tLJv1vqziAKeerg+CWlyOwxmyQrPLeI2jWRXdfHvVBlxcbuipsR+UwSFNvz5oSPkDFPoYQLaVUg6HKAi4d"
                    + "9J07ukU+S2ImCOgfHjtt5a2QWSCrvMwEfiJ7yRvZgfgcUiKamLMTVO8aVwWHSb8TE9z/86shUjLmQNOFHoxgE03vJzNoVTFb4vwd"
                    + "WEPAKBaHCKD1fOSt85lkbCDZv61W/8JNC4LMXBx0a/nU1SHKMu7SQwtNCvgKYThl6Me4HwtVnx+xavlany5xkdox5C9ETclTOaE59A==";
    /** 动物大联盟 TapTap 官方（com.fightingstars.games.animal，CN=shan，SHA-256=de8a75d3…）。
     *  APKSecure 在 Application.attachBaseContext 中校验签名；需在 super 前回灌官方证书。 */
    private static final String DWDLM_PKG = "com.fightingstars.games.animal";
    private static final String DWDLM_CERT_B64 =
            "MIIDaTCCAlGgAwIBAgIEaqhNXzANBgkqhkiG9w0BAQsFADBkMQswCQYDVQQGEwJDTjESMBAGA1UE"
                    + "CBMJZ3Vhbmdkb25nMRIwEAYDVQQHEwlndWFuZ3pob3UxDjAMBgNVBAoTBWJpbmdvMQ4wDAYDVQQL"
                    + "EwViaW5nbzENMAsGA1UEAxMEc2hhbjAgFw0yMzA5MDkwNTA3NDFaGA8yMTIzMDgxNjA1MDc0MVow"
                    + "ZDELMAkGA1UEBhMCQ04xEjAQBgNVBAgTCWd1YW5nZG9uZzESMBAGA1UEBxMJZ3Vhbmd6aG91MQ4w"
                    + "DAYDVQQKEwViaW5nbzEOMAwGA1UECxMFYmluZ28xDTALBgNVBAMTBHNoYW4wggEiMA0GCSqGSIb3"
                    + "DQEBAQUAA4IBDwAwggEKAoIBAQCfR0huISUVuONz3O/C0HR93zyxx1y23fbLfAZDmm99Ye3MKsSt"
                    + "G0dV+VfopLfnRv3Zg3BtSj2c7lpIdROdxEsT0EWuQ4o6JNKgCTParqNouB57xMWhqN+E8iaF1lP9"
                    + "fYT/DQRaGLWoY1lfcu3JXy7kJiDsXXGgQUYiMJjgDbFHKPP+2qMfKKbdCyqHOWM4R1vjdkKifsd3"
                    + "YTw8+FOrMsQ6XFYLPgDZAdmA4yWFsLJXVZaTbcRRcYl/9b1U1EXGsdARF8X0zWNW+lokddhKGwn"
                    + "05tEnbpke18cclOKSstIiZGEab4aoifo6lWUbEM4sgILSJIozane5GbuGdqfz9huRAgMBAAGjITAf"
                    + "MB0GA1UdDgQWBBTvLE8/I1WCrRjo25ZLgRZtFP1x7jANBgkqhkiG9w0BAQsFAAOCAQEAJmar+Mxz"
                    + "Z+Oz4E+R2gT/L9h2sfjkv8U7I60k6wCkMIl6nvEgPoVBHFvJ+CgiXlWvWGJT/StLP1A0TbS4Ic+f"
                    + "MrCB1a7tf3qopaJ8a/pbH/el4r6HQgI0rrqBXD8gtBcCT0q5NrcGaD3CNtz0MgwJB7Q7NF/bdtoX"
                    + "6+B2JhhYsnNDyGe3CuK/XAEOo242gqe/uOAAh5jMdtzX3SibHsEEjOFMuh0W/1djI9lpKUtoQ9jj"
                    + "Z3wLOd6bPDpgQeeul8GVY4Vo5J/X33ooWzAYukJlL/Ur3+10JcsPFDBLVwj5gG+eBR9GtsJoRfUF"
                    + "3qcqVP1rpIumaNfiwEMOfzzZmbNv+w==";
    /** 宝石战争 TapTap 官方（com.acing.gow.adv，MD5=d7dec28ad1785e76c9b912b19ad62d4c）。
     *  2026-09-16 从官方 6.0.012 的 v2 签名证书导出，供 TapSDK 签名读取回灌。 */
    private static final String BSZZ_PKG = "com.acing.gow.adv";
    private static final String BSZZ_CERT_B64 =
            "MIIDeTCCAmGgAwIBAgIEQUNGWDANBgkqhkiG9w0BAQsFADBsMRAwDgYDVQQGEwdVbmtub3duMRAwDgYDVQQIEwdVbmtub3duMRAwDgYDVQQHEwdVbmtub3duMRAwDgYDVQQKEwdVbmtub3duMRAwDgYDVQQLEwdVbmtub3duMRAwDgYDVQQDEwdVbmtub3duMCAXDTE2MDQyNzAzMzYzNloYDzIwNzEwMTI5MDMzNjM2WjBsMRAwDgYDVQQGEwdVbmtub3duMRAwDgYDVQQIEwdVbmtub3duMRAwDgYDVQQHEwdVbmtub3duMRAwDgYDVQQKEwdVbmtub3duMRAwDgYDVQQLEwdVbmtub3duMRAwDgYDVQQDEwdVbmtub3duMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAu4tOXCl9qs+ofsxCC0S2FreAZSBABo6CfLam/oHCrPNP/gobaxi6w8/QcCN1mAvElyV+L1TISXo8Fm13y3zBuQvYiH3W5EYXvVZPKlyX4tb3FObJu7YPh/aNpLgs3xbJ/CVVdvjfNubhAWIYZCuoM3Bc08/lEyon8JqgoiT6KRvc6QjMlJPjro+DgEw6ohYSiXxdnDag0MSrdwJIXtUdQmyxz6Yn/6RZ+EcnjcKoEVcvW8SENoeKpdt/O7re/qECrKCpjktpAcB0jwD3LbfrSWhmUF7YIn7DT/bLapZzzSRdesc/iMH0YGU5RBpCIIUJisULlKneoMPRovJ3HY712wIDAQABoyEwHzAdBgNVHQ4EFgQU5pAJYGyev2009ECHKZWNhLX1mPwwDQYJKoZIhvcNAQELBQADggEBADPsSQ1Bcsxh5jFsouOv6+jP0AI54o56s6Dv4zLPXLgbK7aZp9RAwsyEIJddC/f++TlGFll3hF//M4wr/ALpKhFec/t+/zjR0/FXo7foKBFkdsgZDYrekL1L+035nJ7cJnAuKqLgDKJV8oryGh+kC225u0o3eB3qIqbwf6Hx7Avw6losxbrAbT2orIcNExOf1GNPcqa58Z49vBsML/lRhIr65FSYSQK5Ej3EvGMfLZpDBxwAryaPo2ka/BuNB1aop0Hhusfj8Z/kUr9y1HH1TqhqGwpIY3IG4XuCbHl7HGdeLZWYv6gUIeMEy8w2ooUhuY/KG//1QpeZ8DiGNhhJJkA=";
    private static volatile boolean sInstalled;

    private SignSpoof() {}

    public static void install() {
        if (sInstalled) return;
        synchronized (SignSpoof.class) {
            if (sInstalled) return;
            try {
                exemptHiddenApis();
                Map<String, Signature> fakes = new HashMap<>();
                fakes.put(YWGN_PKG, new Signature(Base64.decode(YWGN_CERT_B64, Base64.DEFAULT)));
                fakes.put(XXH2_PKG, new Signature(Base64.decode(XXH2_CERT_B64, Base64.DEFAULT)));
                fakes.put(SHC_PKG, new Signature(Base64.decode(SHC_CERT_B64, Base64.DEFAULT)));
                fakes.put(ZMXY_PKG, new Signature(Base64.decode(ZMXY_CERT_B64, Base64.DEFAULT)));
                fakes.put(ZMWS_PKG, new Signature(Base64.decode(ZMWS_CERT_B64, Base64.DEFAULT)));
                fakes.put(LTZJ_PKG, new Signature(Base64.decode(LTZJ_CERT_B64, Base64.DEFAULT)));
                fakes.put(NEON_JY_PKG, new Signature(Base64.decode(NEON_JY_CERT_B64, Base64.DEFAULT)));
                fakes.put(NEON_TT_PKG, new Signature(Base64.decode(NEON_TT_CERT_B64, Base64.DEFAULT)));
                fakes.put(ZJMC_PKG, new Signature(Base64.decode(ZJMC_CERT_B64, Base64.DEFAULT)));
                fakes.put(HZSJ_PKG, new Signature(Base64.decode(HZSJ_CERT_B64, Base64.DEFAULT)));
                fakes.put(CSTJJS_PKG, new Signature(Base64.decode(CSTJJS_CERT_B64, Base64.DEFAULT)));
                fakes.put(PCR_PKG, new Signature(Base64.decode(PCR_CERT_B64, Base64.DEFAULT)));
                fakes.put(GYWS_PKG, new Signature(Base64.decode(GYWS_CERT_B64, Base64.DEFAULT)));
                fakes.put(TLGD_PKG, new Signature(Base64.decode(TLGD_CERT_B64, Base64.DEFAULT)));
                fakes.put(TLGD_TT_PKG, new Signature(Base64.decode(TLGD_TT_CERT_B64, Base64.DEFAULT)));
                fakes.put(DZQS_PKG, new Signature(Base64.decode(DZQS_CERT_B64, Base64.DEFAULT)));
                fakes.put(SHHJ_PKG, new Signature(Base64.decode(SHHJ_CERT_B64, Base64.DEFAULT)));
                fakes.put(GJC2INTL_PKG, new Signature(Base64.decode(GJC2INTL_CERT_B64, Base64.DEFAULT)));
                fakes.put(ZMXS_PKG, new Signature(Base64.decode(ZMXS_CERT_B64, Base64.DEFAULT)));
                fakes.put(WGGLS_PKG, new Signature(Base64.decode(WGGLS_CERT_B64, Base64.DEFAULT)));
                fakes.put(WGDZZ_PKG, new Signature(Base64.decode(WGDZZ_CERT_B64, Base64.DEFAULT)));
                fakes.put(BJDW_PKG, new Signature(Base64.decode(BJDW_CERT_B64, Base64.DEFAULT)));
                fakes.put(WHRHXKB_PKG, new Signature(Base64.decode(WHRHXKB_CERT_B64, Base64.DEFAULT)));
                fakes.put(WHRHX4399_PKG, new Signature(Base64.decode(WHRHXKB_CERT_B64, Base64.DEFAULT)));
                fakes.put(CWJH_PKG, new Signature(Base64.decode(CWJH_CERT_B64, Base64.DEFAULT)));
                fakes.put(ENDFIELD_PKG, new Signature(Base64.decode(ENDFIELD_CERT_B64, Base64.DEFAULT)));
                fakes.put(WDYZ_PKG, new Signature(Base64.decode(WDYZ_CERT_B64, Base64.DEFAULT)));
                fakes.put(YQZD_PKG, new Signature(Base64.decode(YQZD_CERT_B64, Base64.DEFAULT)));
                fakes.put(MWDL_PKG, new Signature(Base64.decode(MWDL_CERT_B64, Base64.DEFAULT)));
                fakes.put(WJTX_PKG, new Signature(Base64.decode(WJTX_CERT_B64, Base64.DEFAULT)));
                fakes.put(YSJYZ_PKG, new Signature(Base64.decode(YSJYZ_CERT_B64, Base64.DEFAULT)));
                fakes.put(FCXYXZJ_PKG, new Signature(Base64.decode(FCXYXZJ_CERT_B64, Base64.DEFAULT)));
                fakes.put(WDXSJ_PKG, new Signature(Base64.decode(WDXSJ_CERT_B64, Base64.DEFAULT)));
                fakes.put(DHSS_PKG, new Signature(Base64.decode(DHSS_CERT_B64, Base64.DEFAULT)));
                fakes.put(ZZBX2_PKG, new Signature(Base64.decode(ZZBX2_CERT_B64, Base64.DEFAULT)));
                fakes.put(YNTT_PKG, new Signature(Base64.decode(YNTT_CERT_B64, Base64.DEFAULT)));
                fakes.put(ES2_PKG, new Signature(Base64.decode(ES2_CERT_B64, Base64.DEFAULT)));
                fakes.put(DWDLM_PKG, new Signature(Base64.decode(DWDLM_CERT_B64, Base64.DEFAULT)));
                fakes.put(BSZZ_PKG, new Signature(Base64.decode(BSZZ_CERT_B64, Base64.DEFAULT)));
                killPm(fakes);
                sInstalled = true;
                Log.i(TAG, "installed pkgs=" + fakes.keySet());
            } catch (Throwable t) {
                Log.e(TAG, "install failed", t);
            }
        }
    }

    private static void exemptHiddenApis() {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.P) return;
        // 多路回退：元反射 → 直接 getDeclaredMethod → 直接 getMethod。
        // 部分 ROM（如 MuMu 硬化）会黑名单隐藏 setHiddenApiExemptions 致元反射
        // NoSuchMethodException，逐路降级尝试。
        Throwable last = null;
        try {
            Method meta = Class.class.getDeclaredMethod("getDeclaredMethod", String.class, Class[].class);
            Class<?> vm = Class.forName("dalvik.system.VMRuntime");
            Method getRuntime = (Method) meta.invoke(vm, "getRuntime", (Object[]) null);
            Method setExemptions = (Method) meta.invoke(vm, "setHiddenApiExemptions", new Class[]{String[].class});
            Object runtime = getRuntime.invoke(null);
            setExemptions.invoke(runtime, (Object) new String[]{"L"});
            Log.i(TAG, "exemptHiddenApis(meta) OK");
            return;
        } catch (Throwable t) {
            last = t;
            Log.e(TAG, "exemptHiddenApis(meta) failed", t);
        }
        try {
            Class<?> vm = Class.forName("dalvik.system.VMRuntime");
            Method getRuntime = vm.getDeclaredMethod("getRuntime");
            getRuntime.setAccessible(true);
            Object runtime = getRuntime.invoke(null);
            Method setExemptions = vm.getDeclaredMethod("setHiddenApiExemptions", String[].class);
            setExemptions.setAccessible(true);
            setExemptions.invoke(runtime, (Object) new String[]{"L"});
            Log.i(TAG, "exemptHiddenApis(declared) OK");
            return;
        } catch (Throwable t) {
            last = t;
            Log.e(TAG, "exemptHiddenApis(declared) failed", t);
        }
        try {
            Class<?> vm = Class.forName("dalvik.system.VMRuntime");
            Method setExemptions = vm.getMethod("setHiddenApiExemptions", String[].class);
            Object runtime = vm.getMethod("getRuntime").invoke(null);
            setExemptions.invoke(runtime, (Object) new String[]{"L"});
            Log.i(TAG, "exemptHiddenApis(public) OK");
            return;
        } catch (Throwable t) {
            last = t;
            Log.e(TAG, "exemptHiddenApis(public) failed", t);
        }
        Log.w(TAG, "exemptHiddenApis all failed: " + last);
    }

    private static void killPm(final Map<String, Signature> fakeByPkg) throws Exception {
        final Parcelable.Creator<PackageInfo> originalCreator = PackageInfo.CREATOR;
        Parcelable.Creator<PackageInfo> creator = new Parcelable.Creator<PackageInfo>() {
            @Override
            public PackageInfo createFromParcel(Parcel source) {
                PackageInfo packageInfo = originalCreator.createFromParcel(source);
                if (packageInfo != null && packageInfo.packageName != null) {
                    Signature fake = fakeByPkg.get(packageInfo.packageName);
                    if (fake != null) {
                        applyFake(packageInfo, fake);
                    }
                }
                return packageInfo;
            }

            @Override
            public PackageInfo[] newArray(int size) {
                return originalCreator.newArray(size);
            }
        };
        findField(PackageInfo.class, "CREATOR").set(null, creator);

        try {
            Object cache = findField(PackageManager.class, "sPackageInfoCache").get(null);
            if (cache != null) {
                cache.getClass().getMethod("clear").invoke(cache);
            }
        } catch (Throwable ignored) {
        }
        // 禁止清 Parcel.mCreators / sPairedCreators：ColorOS 上 attach 阶段
        // FileProvider→getExternalFilesDirs→StorageVolume 反序列化会 NPE
        // （HashMap.put on null），弹力果冻官方带 FileProvider 必现。
    }

    private static void applyFake(PackageInfo packageInfo, Signature fakeSignature) {
        if (packageInfo.signatures == null || packageInfo.signatures.length == 0) {
            packageInfo.signatures = new Signature[]{fakeSignature};
        } else {
            packageInfo.signatures[0] = fakeSignature;
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) {
            SigningInfo signingInfo = packageInfo.signingInfo;
            if (signingInfo != null) {
                try {
                    Signature[] arr = signingInfo.getApkContentsSigners();
                    if (arr != null && arr.length > 0) {
                        arr[0] = fakeSignature;
                    }
                } catch (Throwable ignored) {
                }
                try {
                    Signature[] hist = signingInfo.getSigningCertificateHistory();
                    if (hist != null && hist.length > 0) {
                        hist[0] = fakeSignature;
                    }
                } catch (Throwable ignored) {
                }
            }
        }
    }

    private static Field findField(Class<?> clazz, String fieldName) throws NoSuchFieldException {
        Class<?> c = clazz;
        while (c != null && c != Object.class) {
            try {
                Field field = c.getDeclaredField(fieldName);
                field.setAccessible(true);
                return field;
            } catch (NoSuchFieldException ignored) {
                c = c.getSuperclass();
            }
        }
        throw new NoSuchFieldException(fieldName);
    }
}
