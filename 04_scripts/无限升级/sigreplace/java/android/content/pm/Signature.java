package android.content.pm;
import android.os.Parcel;
import android.os.Parcelable;
public class Signature implements Parcelable {
    public Signature(byte[] signature) { }
    public byte[] toByteArray() { return null; }
    public int describeContents() { return 0; }
    public void writeToParcel(Parcel dest, int flags) { }
}