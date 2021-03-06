package ucl.group18.bodyscanner.model;

import android.os.Parcel;
import android.os.Parcelable;
import android.util.Log;

import java.util.Calendar;

/**
 * Class for a measurement request to the server
 * Has parity with a measurement request record in the MeasurementRequests Database.
 * Holds information on:
 * - Database Unique ID
 * - server request ID
 * - if scan processed by server
 * - time of creation of the request
 * - time of last request to the server
 * - Gender
 * - No of times this request has been made to the server
 * - Measurements (if scan is processed)
 *
 * Created by Shubham on 12/04/2015.
 */
public class MeasurementRequest implements Parcelable {

    public enum Gender {MALE,FEMALE};

    private static final String LOG_TAG = "MeasurementRequest";
    long id = -1;
    String requestID;
    boolean processed = false;
    Calendar lastRequest;
    Calendar firstCreated;
    Measurement measurement = null;
    Gender gender;
    int noOfRequests;

    public MeasurementRequest(String requestID, Gender gender){
        this.requestID = requestID;
        this.gender = gender;
    }

    public Measurement getMeasurements (){ return measurement;}

    public long getId() {
        return id;
    }

    public String getRequestID() {
        return requestID;
    }

    public void setId(long id) { this.id=id; }

    public boolean isProcessed() {
        return processed;
    }

    public void setProcessed(boolean processed) {
        this.processed = processed;
    }

    public Calendar getLastRequest() {
        return lastRequest;
    }

    public Calendar getFirstCreated() {
        return firstCreated;
    }

    public Gender getGender() {
        return gender;
    }

    public void setLastRequest(Calendar lastRequest) {
        this.lastRequest = lastRequest;
    }

    public void setFirstCreated(Calendar firstCreated) {
        this.firstCreated = firstCreated;
    }

    public void setMeasurement(Measurement measurement) {this.measurement = measurement;}

    public void setNoOfRequests(int noOfRequests) {this.noOfRequests = noOfRequests;}

    public int getNoOfRequests() {return noOfRequests;}

    public MeasurementRequest shallowCopy(){

        MeasurementRequest measurementRequestCopy = new MeasurementRequest(requestID,gender);
        measurementRequestCopy.setLastRequest(getLastRequest());
        measurementRequestCopy.setNoOfRequests(getNoOfRequests());
        measurementRequestCopy.setProcessed(isProcessed());
        measurementRequestCopy.setId(getId());
        measurementRequestCopy.setMeasurement(getMeasurements());

        return measurementRequestCopy;
    }

    /*
    Methods and Constructor for Parcelable interface
    */
    protected MeasurementRequest(Parcel in) {
        id = in.readLong();
        requestID = in.readString();
        processed = in.readByte() != 0x00;
        lastRequest = (Calendar) in.readValue(Calendar.class.getClassLoader());
        measurement = (Measurement) in.readValue(Measurement.class.getClassLoader());
        gender = (Gender) in.readValue(Gender.class.getClassLoader());
    }

    @Override
    public int describeContents() {
        return 0;
    }

    @Override
    public void writeToParcel(Parcel dest, int flags) {
        dest.writeLong(id);
        dest.writeString(requestID);
        dest.writeByte((byte) (processed ? 0x01 : 0x00));
        dest.writeValue(lastRequest);
        dest.writeValue(measurement);
        dest.writeValue(gender);
    }

    @SuppressWarnings("unused")
    public static final Parcelable.Creator<MeasurementRequest> CREATOR = new Parcelable.Creator<MeasurementRequest>() {
        @Override
        public MeasurementRequest createFromParcel(Parcel in) {
            return new MeasurementRequest(in);
        }

        @Override
        public MeasurementRequest[] newArray(int size) {
            return new MeasurementRequest[size];
        }
    };
}
