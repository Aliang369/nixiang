import brut.androlib.smali.SmaliBuilder;
import com.android.tools.smali.dexlib2.Opcodes;
import com.android.tools.smali.dexlib2.writer.builder.DexBuilder;
import com.android.tools.smali.dexlib2.writer.io.FileDataStore;
import java.io.File;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * Minimal smali -> dex assembler built on apktool 3.x's shaded smali/dexlib2 library.
 *
 * Used by surgical_wxsjoffline_decard.sh because the standalone baksmali/smali 2.5.2 in
 * 01_tools/smali cannot read or write DEX format 040 (Android 15), while apktool 3.0.3
 * ships smali 3.0.9-dev which can.
 *
 * usage: java -cp <apktool.jar>:<classes> AsmDex <smaliDir> <outDex> <api>
 *   api 34 -> dex version 040 (matches the base package's classes3.dex header)
 */
public final class AsmDex {
    private static void collect(File f, List<File> out) {
        if (f.isDirectory()) {
            File[] kids = f.listFiles();
            if (kids != null) {
                for (int i = 0; i < kids.length; i++) {
                    collect(kids[i], out);
                }
            }
        } else if (f.getName().endsWith(".smali")) {
            out.add(f);
        }
    }

    public static void main(String[] args) throws Exception {
        if (args.length < 3) {
            System.err.println("usage: AsmDex <smaliDir> <outDex> <api>");
            System.exit(2);
        }
        int api = Integer.parseInt(args[2]);
        List<File> files = new ArrayList<File>();
        collect(new File(args[0]), files);
        Collections.sort(files);
        DexBuilder dex = new DexBuilder(new Opcodes(api));
        SmaliBuilder smali = new SmaliBuilder(api);
        for (int i = 0; i < files.size(); i++) {
            File f = files.get(i);
            System.out.println("  + " + f.getPath());
            if (!smali.buildFile(f, dex)) {
                System.err.println("ERROR: smali buildFile failed: " + f.getPath());
                System.exit(3);
            }
        }
        dex.writeTo(new FileDataStore(new File(args[1])));
        System.out.println("wrote " + args[1] + " api=" + api + " smali=" + files.size());
    }
}
