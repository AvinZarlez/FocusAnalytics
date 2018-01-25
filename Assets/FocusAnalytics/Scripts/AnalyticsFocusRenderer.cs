using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Microsoft.WindowsAzure.MobileServices;


public class AnalyticsFocusRenderer : MonoBehaviour
{
	#region Inspector Variables
	[Tooltip("The prefab to instantiate for an 'enter' record")]
	public GameObject enterPrefab;

	[Tooltip("The prefab to instantiate for an 'exit' record")]
	public GameObject exitPrefab;

	[Tooltip("The prefab to instantiate for a 'stay' record")]
	public GameObject stayPrefab;
	#endregion // Inspector Variables

	#region Internal Methods
	/// <summary>
	/// Creates a default prefab for a record with the specified color.
	/// </summary>
	/// <param name="color">
	/// The color to use for the prefab.
	/// </param>
	/// <returns>
	/// The prefab instance.
	/// </returns>
	private GameObject CreateDefaultPrefab(Color color)
	{
		// Create a sphere
		var prim = GameObject.CreatePrimitive(PrimitiveType.Sphere);

		// Make it small
		prim.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

		// Change the color
		var rend = prim.GetComponent<Renderer>();
		rend.material.color = color;

		// Remove the collider
		var collider = prim.GetComponent<Collider>();
		if (collider != null)
		{
			Destroy(collider);
		}

		return prim;
	}

	/// <summary>
	/// Initializes any prefabs that weren't specified.
	/// </summary>
	private void InitPrefabs()
	{
		if (enterPrefab == null) { enterPrefab = CreateDefaultPrefab(Color.green); }
		if (exitPrefab == null) { exitPrefab = CreateDefaultPrefab(Color.red); }
		if (stayPrefab == null) { stayPrefab = CreateDefaultPrefab(Color.cyan); }
	}

	/// <summary>
	/// Creates a prefab instance that matches the record type.
	/// </summary>
	/// <param name="type">
	/// The type of record
	/// </param>
	/// <returns>
	/// The prefab instance.
	/// </returns>
	private GameObject CreatePrefab(ReportEventTypes type)
	{
		switch (type)
		{
			case ReportEventTypes.GazeEnter:
			case ReportEventTypes.PointerEnter:
				return GameObject.Instantiate(enterPrefab);
			case ReportEventTypes.GazeExit:
			case ReportEventTypes.PointerExit:
				return GameObject.Instantiate(exitPrefab);
			default:
				return GameObject.Instantiate(stayPrefab);

		}
	}
	#endregion // Internal Methods


	#region Unity Overrides
	protected virtual void Awake()
	{
		InitPrefabs();
	}
	#endregion // Unity Overrides

	#region Public Methods
	/// <summary>
	/// Renders the specified event.
	/// </summary>
	/// <param name="evt">
	/// The event to render
	/// </param>
	/// <param name="entity">
	/// The GameObject that represents the entity.
	/// </param>
	/// <returns>
	/// The <see cref="GameObject"/> that was created to represent the event.
	/// </returns>
	public GameObject RenderEvent(ReportableFocusEvent evt, GameObject entity)
	{
		// Validate parameters
		if (evt == null) throw new ArgumentNullException(nameof(evt));
		if (entity == null) throw new ArgumentNullException(nameof(entity));

		// Create a prefab instance that matches the record type
		var record = CreatePrefab(evt.EventType);

		// Make it a child of the entity so it moves with the entity
		// We use worldPositionStays: true even though we're going to change its 
		// position because this makes sure the original prefabs scale is maintained
		record.transform.SetParent(entity.transform, worldPositionStays: true);

		// Position it correctly
		record.transform.localPosition = evt.LocalPosition;

		// Return it
		return record;
	}
	#endregion // Public Methods
}
