using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity.InputModule;

/// <summary>
/// Specifies that the attached <see cref="GameObject"/> is a target for focus tracking analytics.
/// </summary>
/// <remarks>
/// The <see cref="GameObject"/> must have a <see cref="Collider"/> so that it can receive pointer events. <see cref="AnalyticsFocusTarget"/> 
/// monitors pointer entry and exit events through the <see cref="IPointerSpecificFocusable"/> interface. It then tracks pointer movement 
/// across the surface of the <see cref="Collider"/> while the object has focus. 
/// </remarks>
[RequireComponent(typeof(Collider))]
public class AnalyticsFocusTarget : MonoBehaviour, IPointerSpecificFocusable
{
	#region Nested Types
	private enum TriggerType
	{
		Enter,
		Exit,
		Stay
	};
	#endregion // Nested Types

	#region Member Variables
	private Color focusedColor = Color.red;
	private Vector3 lastWorldPosition;
	private Vector3 lastLocalPosition;
	private Color originalColor;
	private Material material;
	private IEnumerator pointerStayRoutine;
	private List<IPointingSource> trackedPointers = new List<IPointingSource>();
	#endregion // Member Variables

	#region Inspector Variables
	public bool VisualizeGaze = false;

	[Tooltip("The name of this entity when captured in analytics. If not specified it will default to the same name as the GameObject.")]
	public string EntityName;

	[Tooltip("Optional custom renderer to be used for the records collected by this entity.")]
	public AnalyticsFocusRenderer RenderOverride;

	[EnumFlag]
	[Tooltip("Specifies which types of events will be reported by this entity.")]
	public ReportEventTypes ReportEvents = (ReportEventTypes)(-1);

	[Tooltip("The interval, in seconds, that 'Stay' records like PointerStay will be generated.")]
	[Range(0.1f, 10f)]
	public float StayInterval = 1.0f;

	#endregion // Inspector Variables

	/// <summary>
	/// Gets a value that indicates if recording is enabled for the specified event type.
	/// </summary>
	/// <param name="eventType">
	/// The event type to check.
	/// </param>
	/// <returns>
	/// <c>true</c> if enabled; otherwise <c>false</c>.
	/// </returns>
	private bool IsEnabled(ReportEventTypes eventType)
	{
		return ((eventType & ReportEvents) == eventType);
	}

	/// <summary>
	/// Gets the type of event represented by the pointer source and trigger.
	/// </summary>
	/// <param name="source">
	/// The source to test.
	/// </param>
	/// <param name="triggerType">
	/// The trigger type to check.
	/// </param>
	/// <returns>
	/// The event type
	/// </returns>
	static private ReportEventTypes GetEventType(IPointingSource source, TriggerType triggerType)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		bool isGaze = (source is GazeManager);
		// Figure out what type of event this is
		ReportEventTypes eventType;
		switch (triggerType)
		{
			case TriggerType.Enter:
				eventType = (isGaze ? ReportEventTypes.GazeEnter : ReportEventTypes.PointerEnter);
				break;
			case TriggerType.Exit:
				eventType = (isGaze ? ReportEventTypes.GazeExit : ReportEventTypes.PointerExit);
				break;
			default:
				eventType = (isGaze ? ReportEventTypes.GazeStay : ReportEventTypes.PointerStay);
				break;
		}
		return eventType;
	}

	/// <summary>
	/// Coroutine for tracking pointers.
	/// </summary>
	/// <returns>
	/// The <see cref="IEnumerator"/> that represents the coroutine.
	/// </returns>
	private IEnumerator OnPointerStay()
	{
		while (pointerStayRoutine != null)
		{
			lock (trackedPointers)
			{
				foreach (var source in trackedPointers)
				{
					// Make sure source is still pointing at us
					var res = source.Result;
					if ((res != null) && (res.End.Object == gameObject))
					{
						// It is, generate stay record
						RecordEvent(source, TriggerType.Stay);
					}
				}
			}

			// Wait before looping again
			yield return new WaitForSeconds(StayInterval);
		}
	}

	private void RecordEvent(IPointingSource source, TriggerType triggerType)
	{
		// Get the event type
		var eventType = GetEventType(source, triggerType);

		// Is this gaze or controller?
		bool isGaze = (source is GazeManager);

		// Placeholders
		Vector3 worldPosition, localPosition;

		// Exit events need to be handled differently because exit events do not occur "on object"
		if (triggerType != TriggerType.Exit)
		{
			// Use the position from the pointing source
			worldPosition = source.Result.End.Point;
			localPosition = transform.InverseTransformPoint(worldPosition);

			// Update last known positions for eventual exit event
			lastWorldPosition = worldPosition;
			lastLocalPosition = localPosition;
		}
		else
		{
			// It is an exit event, use the last known position "on object"
			worldPosition = lastWorldPosition;
			localPosition = lastLocalPosition;
		}

		// If recording for this event is not enabled, skip the rest of this event
		if (!IsEnabled(eventType)) { return; }

		// Calculate remaining parameters
		string packageToken = HardwareIdentification.GetPackageSpecificToken();
		string sourceName = (isGaze ? "Gaze" : "Controller");

		// Create event
		ReportableFocusEvent evt = new ReportableFocusEvent(packageToken, EntityName, sourceName, eventType, DateTimeOffset.Now, localPosition, worldPosition);

		// Report event
		AnalyticsFocusReporter.Instance.InsertReportableFocusEvent(evt);
	}

	/// <summary>
	/// Starts tracking the specific pointer.
	/// </summary>
	/// <param name="source">
	/// The pointer to start tracking
	/// </param>
	private void StartTracking(IPointingSource source)
	{
		// Validate
		if (source == null) throw new ArgumentNullException(nameof(source));

		lock (trackedPointers)
		{
			if (!trackedPointers.Contains(source))
			{
				trackedPointers.Add(source);
			}
		}

		// If this is the first pointer, start tracking
		if ((trackedPointers.Count > 0) && (pointerStayRoutine == null))
		{
			pointerStayRoutine = OnPointerStay();
			StartCoroutine(pointerStayRoutine);
		}
	}

	/// <summary>
	/// Stops tracking the specific pointer.
	/// </summary>
	/// <param name="source">
	/// The pointer to stop tracking
	/// </param>
	private void StopTracking(IPointingSource source)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		lock (trackedPointers)
		{
			if (trackedPointers.Contains(source))
			{
				trackedPointers.Remove(source);
			}
		}

		// If no more pointers, stop all tracking
		if (trackedPointers.Count < 1) { StopTracking(); }
	}

	/// <summary>
	/// Stops tracking all pointers.
	/// </summary>
	private void StopTracking()
	{
		// If the coroutine is running, stop it
		if (pointerStayRoutine != null)
		{
			StopCoroutine(pointerStayRoutine);
			pointerStayRoutine = null;
		}

		// Clear all tracked pointers
		lock (trackedPointers)
		{
			trackedPointers.Clear();
		}
	}

	#region Unity Overrides
	protected virtual void Awake()
	{
		// Store the material for later updates
		material = GetComponent<Renderer>().material;
		if (material != null)
		{
			originalColor = material.GetColor("_Color");
		}

		// If we haven't been given a specific entity name, default to the game object name
		if (string.IsNullOrEmpty(EntityName))
		{
			EntityName = gameObject.name;
		}
	}

	protected virtual void OnDisable()
	{
		if (AnalyticsFocusReporter.Instance != null)
		{
			AnalyticsFocusReporter.Instance.RemoveNamedEntity(EntityName);
		}

		// Stop tracking
		StopTracking();
	}

	protected virtual void OnEnable()
	{
		if (AnalyticsFocusReporter.Instance != null)
		{
			AnalyticsFocusReporter.Instance.RegisterNamedEntity(EntityName, this.gameObject);
		}
	}
	#endregion // Unity Overrides

	#region IPointerSpecificFocusable Members
	void IPointerSpecificFocusable.OnFocusEnter(PointerSpecificEventData eventData)
	{
		// If visualizing, change the color
		if (this.VisualizeGaze && material != null)
		{
			material.SetColor("_Color", focusedColor);
		}

		// Record the Enter event
		RecordEvent(eventData.Pointer, TriggerType.Enter);

		// If stay tracking is enabled for this type of pointer, start tracking it
		if (IsEnabled(GetEventType(eventData.Pointer, TriggerType.Stay)))
		{
			StartTracking(eventData.Pointer);
		}
	}

	void IPointerSpecificFocusable.OnFocusExit(PointerSpecificEventData eventData)
	{
		// If visualizing, change the color back
		if (this.VisualizeGaze && material != null)
		{
			material.SetColor("_Color", originalColor);
		}

		// Make sure we're no longer tracking this pointer
		StopTracking(eventData.Pointer);

		// Record the Exit event
		RecordEvent(eventData.Pointer, TriggerType.Exit);
	}
	#endregion // IPointerSpecificFocusable Members
}
