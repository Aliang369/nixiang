import org.jf.dexlib2.DexFileFactory;
import org.jf.dexlib2.Opcode;
import org.jf.dexlib2.Opcodes;
import org.jf.dexlib2.builder.BuilderInstruction;
import org.jf.dexlib2.builder.MutableMethodImplementation;
import org.jf.dexlib2.builder.instruction.BuilderInstruction21c;
import org.jf.dexlib2.builder.instruction.BuilderInstruction35c;
import org.jf.dexlib2.iface.ClassDef;
import org.jf.dexlib2.iface.DexFile;
import org.jf.dexlib2.iface.Method;
import org.jf.dexlib2.iface.MethodImplementation;
import org.jf.dexlib2.iface.instruction.Instruction;
import org.jf.dexlib2.iface.instruction.formats.Instruction21c;
import org.jf.dexlib2.iface.instruction.formats.Instruction35c;
import org.jf.dexlib2.iface.reference.MethodReference;
import org.jf.dexlib2.iface.reference.TypeReference;
import org.jf.dexlib2.immutable.ImmutableClassDef;
import org.jf.dexlib2.immutable.ImmutableDexFile;
import org.jf.dexlib2.immutable.ImmutableMethod;
import org.jf.dexlib2.immutable.ImmutableMethodImplementation;
import org.jf.dexlib2.immutable.instruction.ImmutableInstruction10x;
import org.jf.dexlib2.immutable.instruction.ImmutableInstruction11n;
import org.jf.dexlib2.immutable.instruction.ImmutableInstruction11x;
import org.jf.dexlib2.immutable.instruction.ImmutableInstruction21c;
import org.jf.dexlib2.immutable.reference.ImmutableMethodReference;
import org.jf.dexlib2.immutable.reference.ImmutableStringReference;
import org.jf.dexlib2.immutable.reference.ImmutableTypeReference;

import java.io.File;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

/**
 * 无限升级 13.101 TapTap 四步补丁（dexlib2 级手术，2026-11-18）
 *
 * 为什么不用 smali 往返：smali 2.5.2 assemble 非字节确定（同一 smali 树两次产出 md5 不同，
 * 实测 /tmp/dt -> f200f5d7 / b3a8e90b），无法满足「同输入两遍 md5 一致」。
 * dexlib2 write 实测确定（同一 DexFile 两次写 md5 相同 d5508880）。
 *
 * 只改三个类的四个方法，其余类原样透传：
 *   com/taptap/sdk/initializer/repository/GatekeeperRepository
 *       isGatekeeperLoadedSuccessfully()Z  -> const/4 v0,0x1; return v0
 *       showErrorToast(TapNetworkException)V -> return-void
 *   com/taptap/sdk/login/internal/LoginActivityImpl
 *       <init> 里 ClientLoginHandler 的 new-instance / <init> 调用 -> WebHandler
 *   com/taptap/sdk/kit/internal/utils/TapSignKit
 *       getSign(Context)String -> const-string <certMd5>; return-object v0
 */
public final class PatchWxsjTap {

    private static final String C_REPO = "Lcom/taptap/sdk/initializer/repository/GatekeeperRepository;";
    private static final String C_LOGIN = "Lcom/taptap/sdk/login/internal/LoginActivityImpl;";
    private static final String C_SIGN = "Lcom/taptap/sdk/kit/internal/utils/TapSignKit;";
    private static final String CLIENT = "Lcom/taptap/sdk/login/internal/handlers/client/ClientLoginHandler;";
    private static final String WEB = "Lcom/taptap/sdk/login/internal/handlers/web/WebHandler;";

    public static void main(String[] a) throws Exception {
        String in = a[0], out = a[1], cert = a[2];
        DexFile df = DexFileFactory.loadDexFile(new File(in), Opcodes.getDefault());
        List<ClassDef> outClasses = new ArrayList<>();
        int n1 = 0, n2 = 0, n3 = 0, n4 = 0;
        for (ClassDef cd : df.getClasses()) {
            String t = cd.getType();
            if (C_REPO.equals(t)) {
                List<Method> dm = new ArrayList<>();
                List<Method> vm = new ArrayList<>();
                for (Method m : cd.getVirtualMethods()) {
                    if ("isGatekeeperLoadedSuccessfully".equals(m.getName()) && m.getParameters().isEmpty()
                            && "Z".equals(m.getReturnType())) {
                        ImmutableMethodImplementation impl = new ImmutableMethodImplementation(2, Arrays.<Instruction>asList(
                                new ImmutableInstruction11n(Opcode.CONST_4, 0, 1),
                                new ImmutableInstruction11x(Opcode.RETURN, 0)), null, null);
                        vm.add(copy(m, impl)); n1++;
                    } else {
                        vm.add(m);
                    }
                }
                for (Method m : cd.getDirectMethods()) {
                    if ("isGatekeeperLoadedSuccessfully".equals(m.getName()) && m.getParameters().isEmpty()
                            && "Z".equals(m.getReturnType())) {
                        ImmutableMethodImplementation impl = new ImmutableMethodImplementation(2, Arrays.<Instruction>asList(
                                new ImmutableInstruction11n(Opcode.CONST_4, 0, 1),
                                new ImmutableInstruction11x(Opcode.RETURN, 0)), null, null);
                        dm.add(copy(m, impl)); n2++;
                    } else if ("showErrorToast".equals(m.getName()) && m.getParameters().size() == 1
                            && "Lcom/taptap/sdk/common/network/throwable/TapNetworkException;"
                                    .equals(m.getParameters().get(0).getType())
                            && "V".equals(m.getReturnType())) {
                        ImmutableMethodImplementation impl = new ImmutableMethodImplementation(
                                m.getImplementation() == null ? 8 : m.getImplementation().getRegisterCount(),
                                Arrays.<Instruction>asList(new ImmutableInstruction10x(Opcode.RETURN_VOID)), null, null);
                        dm.add(copy(m, impl)); n2++;
                    } else {
                        dm.add(m);
                    }
                }
                outClasses.add(rebuild(cd, dm, vm));
            } else if (C_LOGIN.equals(t)) {
                List<Method> dm = new ArrayList<>();
                for (Method m : cd.getDirectMethods()) {
                    if ("<init>".equals(m.getName()) && m.getImplementation() != null) {
                        dm.add(copy(m, patchInit(m.getImplementation()))); n3++;
                    } else {
                        dm.add(m);
                    }
                }
                outClasses.add(rebuild(cd, dm, null));
            } else if (C_SIGN.equals(t)) {
                List<Method> vm = new ArrayList<>();
                for (Method m : cd.getVirtualMethods()) {
                    if ("getSign".equals(m.getName()) && m.getParameters().size() == 1
                            && "Landroid/content/Context;".equals(m.getParameters().get(0).getType())
                            && "Ljava/lang/String;".equals(m.getReturnType())) {
                        ImmutableMethodImplementation impl = new ImmutableMethodImplementation(4, Arrays.<Instruction>asList(
                                new ImmutableInstruction21c(Opcode.CONST_STRING, 0, new ImmutableStringReference(cert)),
                                new ImmutableInstruction11x(Opcode.RETURN_OBJECT, 0)), null, null);
                        vm.add(copy(m, impl)); n4++;
                    } else {
                        vm.add(m);
                    }
                }
                outClasses.add(rebuild(cd, null, vm));
            } else {
                outClasses.add(cd);
            }
        }
        DexFileFactory.writeDexFile(out, new ImmutableDexFile(Opcodes.getDefault(), outClasses));
        System.out.println("patched: gatekeeper=" + n1 + "/" + n2 + " loginInit=" + n3 + " getSign=" + n4
                + " classes=" + outClasses.size());
        if (n1 < 1 || n2 < 1 || n3 < 1 || n4 < 1) {
            throw new IllegalStateException("锚点缺失：gatekeeper=" + n1 + "/" + n2 + " loginInit=" + n3 + " getSign=" + n4);
        }
    }

    private static List<String> toStringList(List<? extends CharSequence> in) {
        List<String> out = new ArrayList<>();
        for (CharSequence cs : in) out.add(cs.toString());
        return out;
    }

    private static Method copy(Method m, MethodImplementation impl) {
        return new ImmutableMethod(m.getDefiningClass(), m.getName(), m.getParameters(), m.getReturnType(),
                m.getAccessFlags(), m.getAnnotations(), m.getHiddenApiRestrictions(), impl);
    }

    private static ClassDef rebuild(ClassDef cd, List<? extends Method> direct, List<? extends Method> virtual) {
        return new ImmutableClassDef(cd.getType(), cd.getAccessFlags(), cd.getSuperclass(), cd.getInterfaces(),
                cd.getSourceFile(), cd.getAnnotations(), cd.getStaticFields(), cd.getInstanceFields(),
                direct == null ? cd.getDirectMethods() : direct,
                virtual == null ? cd.getVirtualMethods() : virtual);
    }

    /** <init> 里把 ClientLoginHandler 的实例化与其 <init> 调用换成 WebHandler（数组形态/instance-of 不动）。 */
    private static ImmutableMethodImplementation patchInit(MethodImplementation impl) {
        MutableMethodImplementation mmi = new MutableMethodImplementation(impl);
        List<BuilderInstruction> list = mmi.getInstructions();
        for (int i = 0; i < list.size(); i++) {
            BuilderInstruction bi = list.get(i);
            if (bi.getOpcode() == Opcode.NEW_INSTANCE && bi instanceof Instruction21c) {
                Instruction21c i21 = (Instruction21c) bi;
                if (i21.getReference() instanceof TypeReference
                        && CLIENT.equals(((TypeReference) i21.getReference()).getType())) {
                    mmi.replaceInstruction(i, new BuilderInstruction21c(Opcode.NEW_INSTANCE, i21.getRegisterA(),
                            new ImmutableTypeReference(WEB)));
                }
            } else if (bi.getOpcode() == Opcode.INVOKE_DIRECT && bi instanceof Instruction35c) {
                Instruction35c i35 = (Instruction35c) bi;
                if (i35.getReference() instanceof MethodReference) {
                    MethodReference mr = (MethodReference) i35.getReference();
                    if (CLIENT.equals(mr.getDefiningClass()) && "<init>".equals(mr.getName())) {
                        ImmutableMethodReference rep = new ImmutableMethodReference(WEB, "<init>",
                                toStringList(mr.getParameterTypes()), mr.getReturnType());
                        mmi.replaceInstruction(i, new BuilderInstruction35c(Opcode.INVOKE_DIRECT,
                                i35.getRegisterCount(), i35.getRegisterC(), i35.getRegisterD(), i35.getRegisterE(),
                                i35.getRegisterF(), i35.getRegisterG(), rep));
                    }
                }
            }
        }
        return ImmutableMethodImplementation.of(mmi);
    }
}
