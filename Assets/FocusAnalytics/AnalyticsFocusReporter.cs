using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;

public class AnalyticsFocusReporter : MonoBehaviour {

    [HideInInspector]
    static public AnalyticsFocusReporter instance = null;

    public string MobileAppUri = string.Empty;

    protected MobileServiceClient Client;

    [Range(15f, 600f)]
    public float pushInterval = 15.0f;

    // Use this for initialization
    void Awake() {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogError("ERROR: Another AnalyticsFocusReporter exists alraedy in the scene");
            Destroy(gameObject);
        }

        InitClient();

        CancelInvoke("PushChanges");
        Invoke("PushChanges", pushInterval);
    }

    void OnDestroy()
    {
        CancelInvoke("PushChanges");

        //Eventually, will need to add code to do Client.LogoutAsync()
    }

    private async void InitClient()
	{
        if (Client == null)
        {

#if UNITY_ANDROID
		    // Android builds fail at runtime due to missing GZip support, so build a handler that uses Deflate for Android
		    var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate };
		    Client = new MobileServiceClient(MobileAppUri, handler);
#else
            Client = new MobileServiceClient(MobileAppUri);
#endif

            var store = new MobileServiceSQLiteStore("AnalyticsFocus.db");
            store.DefineTable<ReportableFocusEvent>();
            await Client.SyncContext.InitializeAsync(store);
        }
        else
        {
            Debug.LogError("ERROR: Client already initialized");
        }

    }

    public static IMobileServiceTable<ReportableFocusEvent> GetTable()
    {
        return instance.Client.GetTable<ReportableFocusEvent>();
    }

    public async void PushChanges()
    {
        //Debug.Log("Pushing table with " + Client.SyncContext.PendingOperations + " items");

        try
        {
            await Client.SyncContext.PushAsync();

        }
        catch (MobileServicePushFailedException e)
        {
            Debug.Log(e.ToString() + " | "+ e.PushResult.Status.ToString());
            var errors = e.PushResult.Errors;
            for (int i = 0; i < errors.Count; i++)
            {
                Debug.Log(errors[i].RawResult.ToString());
            }
        }

        Assert.IsTrue(pushInterval >= 0.1f);

        Invoke("PushChanges", pushInterval);
    }

    public static void InsertReportableFocusEvent(ReportableFocusEvent rfe)
    {
        instance.LocalInsertReportableFocusEvent(rfe);
    }

    protected async void LocalInsertReportableFocusEvent( ReportableFocusEvent rfe )
    {
        Assert.IsNotNull<ReportableFocusEvent>(rfe);

        IMobileServiceSyncTable<ReportableFocusEvent> localTable = Client.GetSyncTable<ReportableFocusEvent>();

        try
        {
            await localTable.InsertAsync(rfe);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }
}
