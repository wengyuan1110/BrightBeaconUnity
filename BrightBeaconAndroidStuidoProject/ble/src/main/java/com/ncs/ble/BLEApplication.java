package com.ncs.ble;

import android.Manifest;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothManager;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.RequiresApi;
import androidx.core.app.ActivityCompat;

import com.brtbeacon.sdk.BRTBeacon;
import com.brtbeacon.sdk.BRTBeaconManager;
import com.brtbeacon.sdk.BRTThrowable;
import com.brtbeacon.sdk.IBle;
import com.brtbeacon.sdk.callback.BRTBeaconManagerListener;
import com.brtbeacon.sdk.utils.L;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;

import com.google.gson.Gson;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

public class BLEApplication extends UnityPlayerActivity {
    private static final int BRTMAP_PERMISSION_CODE = 100;
    public int REQUEST_ENABLE_BT = 1;
    public static BLEApplication mInstance = null;
    public BRTBeaconManager mBeaconManager = null;
    private BluetoothAdapter mBluetoothAdapter = null;
    private static Context mContext;

    public static BLEApplication getInstance() {
        if (mInstance == null) {
            mInstance = new BLEApplication();
        }
        /*
        if (!mContext.getPackageManager().hasSystemFeature("android.hardware.bluetooth_le")) {
            Toast.makeText(mContext, "BLE Not Supported",
                    Toast.LENGTH_SHORT).show();
        }
        */

        return mInstance;
    }

    public BLEApplication() {
        UnityPlayer.UnitySendMessage("Main Camera", "OnLogMessage", "Initialize Started");

        mContext = UnityPlayer.currentActivity.getApplicationContext();
        // 单例
        mBeaconManager = BRTBeaconManager.getInstance(mContext);
        // 注册应用 APPKEY申请地址 http://brtbeacon.com/main/index.shtml
        mBeaconManager.registerApp("3d76046a7c5b49dcaf0f4e193b7f844d");
        // 开启Beacon扫描服务
        //beaconManager.startService();
        mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter();
        mBluetoothAdapter.enable();
        UnityPlayer.UnitySendMessage("Main Camera", "OnLogMessage", "Initialize bluetooth: " + mBluetoothAdapter.isEnabled());

        checkPermission(UnityPlayer.currentActivity.getApplicationContext(), UnityPlayer.currentActivity);
        UnityPlayer.UnitySendMessage("Main Camera", "OnLogMessage", "Initialize Completed");
    }

    @Override
    protected void onResume() {
        super.onResume();
        UnityPlayer.UnitySendMessage("Main Camera", "OnLogMessage", "onResume");
        if (mBluetoothAdapter != null && mBluetoothAdapter.isEnabled()) {
            startScan();
        }
    }

    @Override
    protected void onPause() {
        super.onPause();
        UnityPlayer.UnitySendMessage("Main Camera", "OnLogMessage", "onPause");
        if (mBluetoothAdapter != null && mBluetoothAdapter.isEnabled()) {
            stopScan();
        }
    }

    public void startBTScan() {
        if (mBluetoothAdapter != null && mBluetoothAdapter.isEnabled()) {
            UnityPlayer.UnitySendMessage("Main Camera", "OnLogMessage", "start scan");
            startScan();
        }
    }

    private void startScan() {
        mBeaconManager.setBRTBeaconManagerListener(scanListener);
        mBeaconManager.startRanging();
    }

    private void stopScan() {
        mBeaconManager.stopRanging();
        mBeaconManager.setBRTBeaconManagerListener(null);
    }

    private BRTBeaconManagerListener scanListener = new BRTBeaconManagerListener() {

        @Override
        public void onUpdateBeacon(final ArrayList<BRTBeacon> arg0) {

            /*
            for(int i = 0; i < arg0.size(); ++i) {
                Gson gson = new Gson();
                String device_info = gson.toJson(arg0.get(i));
                if (arg0.get(i).uuid != "00000000-0000-0000-0000-000000000000" || !arg0.get(i).uuid.equals("00000000-0000-0000-0000-000000000000"))
                {
                    //UnityPlayer.UnitySendMessage("Main Camera", "OnLogMessage", "---------------------" + device_info);
                    UnityPlayer.UnitySendMessage("Main Camera", "onUpdateBeacon", device_info);
                }
            }

             */


            Gson gson = new Gson();
            String devices_info = gson.toJson((List)arg0);
            UnityPlayer.UnitySendMessage("Main Camera", "onUpdateBeacon", devices_info);

        }

        @Override
        public void onNewBeacon(BRTBeacon arg0) {
            Gson gson = new Gson();
            String device_info = gson.toJson(arg0);
            UnityPlayer.UnitySendMessage("Main Camera", "onNewBeacon", device_info);
        }

        @Override
        public void onGoneBeacon(BRTBeacon arg0) {
            Gson gson = new Gson();
            String device_info = gson.toJson(arg0);
            UnityPlayer.UnitySendMessage("Main Camera", "onGoneBeacon", device_info);
        }

        @Override
        public void onError(BRTThrowable arg0) {

        }
    };

    private void checkPermission(Context context, Activity activity) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {//判断当前系统的SDK版本是否大于23
            List<String> permissionNeedRequest = new LinkedList<>();

            List<String> permissions = new ArrayList<>();

            permissions.add(Manifest.permission.ACCESS_FINE_LOCATION);
            permissions.add(Manifest.permission.ACCESS_COARSE_LOCATION);

            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
                // 安卓12及以上版本需要申请的相关权限
                permissions.add(Manifest.permission.BLUETOOTH_CONNECT);
                permissions.add(Manifest.permission.BLUETOOTH_SCAN);
                permissions.add(Manifest.permission.BLUETOOTH_ADVERTISE);
            }

            for (String permssion: permissions) {
                if(context.checkSelfPermission(permssion) != PackageManager.PERMISSION_GRANTED) {
                    permissionNeedRequest.add(permssion);
                }
            }
            if (permissionNeedRequest.isEmpty()) {
                return;
            }
            UnityPlayer.UnitySendMessage("Main Camera", "OnLogMessage", "checkPermission");
            activity.requestPermissions(permissionNeedRequest.toArray(new String[0]), BRTMAP_PERMISSION_CODE);
        }
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        switch (requestCode) {
            // requestCode即所声明的权限获取码，在requestPermissions时传入
            case BRTMAP_PERMISSION_CODE:
                boolean isAllGrant = true;
                for (int grantResult: grantResults) {
                    if (grantResult != PackageManager.PERMISSION_GRANTED) {
                        isAllGrant = false;
                        break;
                    }
                }

                if (!isAllGrant) {
                    Toast.makeText(mContext, "获取位置权限失败，请手动前往设置开启", Toast.LENGTH_SHORT).show();
                }

                break;
            default:
                break;
        }
    }
}

