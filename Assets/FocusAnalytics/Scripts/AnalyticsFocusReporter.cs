using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using HoloToolkit.Unity;

public class AnalyticsFocusReporter : Singleton<AnalyticsFocusReporter>
{
	#region Member Variables
	private Dictionary<string, GameObject> entityTable = new Dictionary<string, GameObject>();
	private List<GameObject> recordObjects = new List<GameObject>();
	#endregion // Member Variables

	#region Inspector Variables
	public string MobileAppUri = string.Empty;
	protected MobileServiceClient Client;

	[Tooltip("The interval, in seconds, that records will be pushed to the server.")]
	[Range(15f, 600f)]
	public float pushInterval = 15.0f;

	[Tooltip("The default reporter that will be used if none is specified on the entity. If one is not supplied it will be created.")]
	public AnalyticsFocusRenderer DefaultRenderer;
	#endregion // Inspector Variables

	#region Unity Overrides
	// Use this for initialization
	protected override void Awake()
	{
		base.Awake();

		InitClient();
		InitRenderer();

		CancelInvoke("PushChanges");
		Invoke("PushChanges", pushInterval);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		CancelInvoke("PushChanges");

		//Eventually, will need to add code to do Client.LogoutAsync()
	}
	#endregion // Unity Overrides

	#region Internal Methods
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

	private void InitRenderer()
	{
		if (DefaultRenderer == null)
		{
			DefaultRenderer = gameObject.AddComponent<AnalyticsFocusRenderer>();
		}
	}

	protected async void LocalInsertReportableFocusEvent(ReportableFocusEvent rfe)
	{
		Assert.IsNotNull<ReportableFocusEvent>(rfe);

		IMobileServiceSyncTable<ReportableFocusEvent> localTable = Client.GetSyncTable<ReportableFocusEvent>();

		try
		{
			await localTable.InsertAsync(rfe);
			Debug.Log($"Recording: {rfe.EventType} on {rfe.EntityName} at {rfe.LocalPosition}");
		}
		catch (Exception e)
		{
			Debug.LogError(e.ToString());
		}
	}
	#endregion // Internal Methods

	#region Public Methods
	/// <summary>
	/// Removes all of the report objects currently in the scene.
	/// </summary>
	public void ClearReport()
	{
		for (int i = recordObjects.Count - 1; i >= 0; i--)
		{
			Destroy(recordObjects[i]);
			recordObjects.RemoveAt(i);
		}
	}

	/// <summary>
	/// Gets the specified named entity.
	/// </summary>
	/// <param name="name">
	/// The name to retrieve.
	/// </param>
	public GameObject GetNamedEntity(string name)
	{
		return entityTable[name];
	}

	public IMobileServiceTable<ReportableFocusEvent> GetTable()
	{
		return Client.GetTable<ReportableFocusEvent>();
	}

	public void InsertReportableFocusEvent(ReportableFocusEvent rfe)
	{
		LocalInsertReportableFocusEvent(rfe);
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
			Debug.Log(e.ToString() + " | " + e.PushResult.Status.ToString());
			var errors = e.PushResult.Errors;
			for (int i = 0; i < errors.Count; i++)
			{
				Debug.Log(errors[i].RawResult.ToString());
			}
		}

		Assert.IsTrue(pushInterval >= 0.1f);

		Invoke("PushChanges", pushInterval);
	}

	/// <summary>
	/// Registers the specified GameObject as the named entity.
	/// </summary>
	/// <param name="name">
	/// The name to register with.
	/// </param>
	/// <param name="entity">
	/// The entity to register.
	/// </param>
	public void RegisterNamedEntity(string name, GameObject entity)
	{
		// Test for valid name
		if (string.IsNullOrEmpty(name)) { throw new ArgumentException(nameof(name) + " must not be null or empty"); }

		// Make sure valid entity
		if (entity == null) throw new ArgumentNullException(nameof(entity) + " must not be null");

		// Test that entity is an actual focus target
		var target = entity.GetComponent<AnalyticsFocusTarget>();
		if (target == null) { throw new ArgumentException(nameof(entity) + " is not an analytic target"); }

		// Register the name
		entityTable.Add(name, entity);
	}

	/// <summary>
	/// Removes the specified named entity.
	/// </summary>
	/// <param name="name">
	/// The name to remove.
	/// </param>
	public void RemoveNamedEntity(string name)
	{
		entityTable.Remove(name);
	}

	/// <summary>
	/// Runs a report of all records in the table.
	/// </summary>
	/// <param name="pageSize">
	/// The number of records to return in each page.
	/// </param>
	/// <param name="maxPages">
	/// The maximum number of pages to render.
	/// </param>
	public async void RunReport(int pageSize = 25, int maxPages = 4)
	{
		// Validate parameters
		if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));
		if (maxPages < 1) throw new ArgumentOutOfRangeException(nameof(maxPages));

		try
		{
			// Get the table
			var tbl = GetTable();

			// Loop as long as we're getting records
			for (int iPage=0; iPage < maxPages; iPage++)
			{ 
				// Fetch next page of data
				var page = await tbl.OrderByDescending((e) => e.Time).Skip(iPage * pageSize).Take(pageSize).ToListAsync();

				Debug.Log($"Rendering {page.Count} records (page {iPage + 1}).");

				// Render all records in this page
				foreach (ReportableFocusEvent evt in page)
				{
					// Finding the entity with the specified EntityName
					var entity = GetNamedEntity(evt.EntityName);

					// If not found, log and bail
					if (entity == null)
					{
						Debug.LogWarning($"No Entity with the name '{evt.EntityName}' could be found in the name table.");
						continue;
					}

					// Get the target
					var target = entity.GetComponent<AnalyticsFocusTarget>();

					// If no longer a valid target, log and bail
					if (target == null)
					{
						Debug.LogWarning($"Entity '{evt.EntityName}' is no longer an analytic target.");
						continue;
					}

					// Which renderer do we use?
					var renderer = (target.RenderOverride != null ? target.RenderOverride : DefaultRenderer);

					// Ask the renderer to render the event record
					var record = renderer.RenderEvent(evt, entity);

					// Hold onto the record so we can clear it later
					recordObjects.Add(record);
				}

				// If there are fewer records than a full page we know there are no more pages
				if (page.Count < pageSize) { break; }
			}
		}
		catch (Exception e)
		{
			Debug.Log(e.ToString());
		}
	}
	#endregion // Public Methods
}
