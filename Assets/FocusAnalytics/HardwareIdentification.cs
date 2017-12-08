using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HardwareIdentification {
    
    public static string GetPackageSpecificToken()
    {
#if WINDOWS_UWP
        var token = Windows.System.Profile.HardwareIdentification.GetPackageSpecificToken(null);
        var hardwareId = token.Id;
        var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(hardwareId);

        byte[] bytes = new byte[hardwareId.Length];
        dataReader.ReadBytes(bytes);

        return BitConverter.ToString(bytes);
#elif UNITY_EDITOR
        return "Unity Editor";
#else
        throw new System.Exception("Package Specific Token unavailable when running outside of the UWP platform.");
#endif
    }
}
