import com.android.apksig.ApkVerifier;

import java.io.File;
import java.security.cert.X509Certificate;
import java.util.Base64;
import java.util.List;

/** Print the DER base64 of every signer certificate from an APK (v1/v2/v3). */
public final class ExtractApkCert {
    public static void main(String[] args) throws Exception {
        if (args.length != 1) {
            System.err.println("usage: ExtractApkCert <apk>");
            System.exit(2);
        }
        ApkVerifier.Result result = new ApkVerifier.Builder(new File(args[0])).build().verify();
        List<X509Certificate> certs = result.getSignerCertificates();
        if (certs == null || certs.isEmpty()) {
            System.err.println("no signer certificates");
            System.exit(1);
        }
        for (X509Certificate cert : certs) {
            System.out.println("subject=" + cert.getSubjectX500Principal());
            System.out.println("sha256=" + Base64.getEncoder().encodeToString(
                    java.security.MessageDigest.getInstance("SHA-256").digest(cert.getEncoded())));
            System.out.println("der_b64=" + Base64.getEncoder().encodeToString(cert.getEncoded()));
        }
    }
}
