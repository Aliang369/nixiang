package android.os;
public interface Parcelable {
    interface Creator<T> { T createFromParcel(Parcel p); T[] newArray(int n); }
    int describeContents();
    void writeToParcel(Parcel dest, int flags);
}
