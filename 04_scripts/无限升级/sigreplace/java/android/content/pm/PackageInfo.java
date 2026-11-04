package android.content.pm;
import android.os.Parcel;
import android.os.Parcelable;
public class PackageInfo implements Parcelable {
    public String packageName;
    public Signature[] signatures;
    public static final Parcelable.Creator<PackageInfo> CREATOR = null;
    public int describeContents() { return 0; }
    public void writeToParcel(Parcel dest, int flags) { }
}