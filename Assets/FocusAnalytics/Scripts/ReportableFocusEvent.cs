using System;
using Newtonsoft.Json;
using UnityEngine;
using System.ComponentModel;

/// <summary>
/// Describes the types of report events that can occur.
/// </summary>
[Flags]
public enum ReportEventTypes
{
	/// <summary>
	/// The user began gazing at the entity.
	/// </summary>
	GazeEnter = (1 << 0),

	/// <summary>
	/// The user stopped gazing at the entity.
	/// </summary>
	GazeExit = (1 << 1),

	/// <summary>
	/// Users gaze remained on the entity for an update period.
	/// </summary>
	GazeStay = (1 << 2),

	/// <summary>
	/// A controller began pointing at the entity.
	/// </summary>
	PointerEnter = (1 << 3),

	/// <summary>
	/// A controller stopped pointing at the entity.
	/// </summary>
	PointerExit = (1 << 4),

	/// <summary>
	/// A controller remained pointing at the entity for an update period.
	/// </summary>
	PointerStay = (1 << 5),

	/// <summary>
	/// A controller has grabbed the entity.
	/// </summary>
	Grab = (1 << 6),

	/// <summary>
	/// A controller has released the entity.
	/// </summary>
	Release = (1 << 6),
}

/// <summary>
/// Represents a single report for a focus event.
/// </summary>
public class ReportableFocusEvent
{
	#region Member Variables
	private bool calcTime;          // Should Time be calculated the next time it's accessed?
	private Vector3 localPosition;	// Position of the event in the entities local coordinate space
	private DateTimeOffset time;	// Time of the event in DateTimeOffset format (.Net only)
	private DateTime timeUtc;		// Time of the event in UTC (server only)
	private int timeOffset;			// Offset in time of the event (server only)
	private Vector3 worldPosition;  // Position of the event in the scenes world space
	#endregion // Member Variables

	#region Constructors
	/// <summary>
	/// This constructor is to be used for serialization purposes only.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public ReportableFocusEvent()
	{
	}

	/// <summary>
	/// Initializes a new <see cref="ReportableFocusEvent"/>.
	/// </summary>
	/// <param name="packageToken">
	/// The package token that uniquely identifies the application.
	/// </param>
	/// <param name="entityName">
	/// The unique name of the entity where the event occurred.
	/// </param>
	/// <param name="sourceName">
	/// The name of the source that triggered the event.
	/// </param>
	/// <param name="eventType">
	/// The type of event that has occurred.
	/// </param>
	/// <param name="time">
	/// The time that the event occurred.
	/// </param>
	/// <param name="localPosition">
	/// The position that the event occurred in the entities local coordinate space.
	/// </param>
	/// <param name="worldPosition">
	/// The position that the event occurred in world space.
	/// </param>
	public ReportableFocusEvent(string packageToken, string entityName, string sourceName, ReportEventTypes eventType, DateTimeOffset time, Vector3 localPosition, Vector3 worldPosition)
	{
		this.PackageToken = packageToken;
		this.EntityName = entityName;
		this.SourceName = sourceName;
		this.EventType = eventType;
		this.Time = time;
		this.LocalPosition = localPosition;
		this.WorldPosition = worldPosition;
	}
	#endregion // Constructors


	#region Serialization Properties
	/**************************************************************************
	 * The properties in this region are here to avoid requiring custom Read, 
	 * Insert and Update scripts on the mobile service. This approach may 
	 * change in the final architecture but ideally the public signature of 
	 * this class shouldn't change.
	 **************************************************************************/

	[EditorBrowsable(EditorBrowsableState.Never)]
	[JsonProperty(PropertyName = "localX")]
	private float LocalX
	{
		get
		{
			return localPosition.x;
		}
		set
		{
			localPosition.Set(value, localPosition.y, localPosition.z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[JsonProperty(PropertyName = "localY")]
	private float LocalY
	{
		get
		{
			return localPosition.y;
		}
		set
		{
			localPosition.Set(localPosition.x, value, localPosition.z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[JsonProperty(PropertyName = "localZ")]
	private float LocalZ
	{
		get
		{
			return localPosition.z;
		}
		set
		{
			localPosition.Set(localPosition.x, localPosition.y, value);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[JsonProperty(PropertyName = "time")]
	private DateTime TimeUtc
	{
		get
		{
			return timeUtc;
		}
		set
		{
			if (timeUtc != value)
			{
				timeUtc = value;
				calcTime = true;
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[JsonProperty(PropertyName = "timeOffset")]
	private int TimeOffset
	{
		get
		{
			return timeOffset;
		}
		set
		{
			if (timeOffset != value)
			{
				timeOffset = value;
				calcTime = true;
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[JsonProperty(PropertyName = "worldX")]
	private float WorldX
	{
		get
		{
			return worldPosition.x;
		}
		set
		{
			worldPosition.Set(value, worldPosition.y, worldPosition.z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[JsonProperty(PropertyName = "worldY")]
	private float WorldY
	{
		get
		{
			return worldPosition.y;
		}
		set
		{
			worldPosition.Set(worldPosition.x, value, worldPosition.z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[JsonProperty(PropertyName = "worldZ")]
	private float WorldZ
	{
		get
		{
			return worldPosition.z;
		}
		set
		{
			worldPosition.Set(worldPosition.x, worldPosition.y, value);
		}
	}
	#endregion // Serialization Properties


	#region Public Properties
	/// <summary>
	/// Gets or sets the unique ID of the event record.
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Gets or sets the package token that uniquely identifies the application.
	/// </summary>
	[JsonProperty(PropertyName = "packageToken")]
	public string PackageToken { get; set; }

	/// <summary>
	/// Gets or sets the unique name of the entity where the event occurred.
	/// </summary>
	[JsonProperty(PropertyName = "entityName")]
	public string EntityName { get; set; }

	/// <summary>
	/// Gets or sets the type of event that has occurred.
	/// </summary>
	/// <remarks>
	/// The value is accessed locally as a <see cref="ReportEventTypes"/> enum but on the server it is 
	/// stored as an <see cref="int"/> value.
	/// </remarks>
	[JsonProperty(PropertyName = "eventType")]
	public ReportEventTypes EventType { get; set; }

	/// <summary>
	/// Gets or sets the position that the event occurred in the entities local coordinate space.
	/// </summary>
	/// <remarks>
	/// The value is accessed locally as a <see cref="Vector3"/> but on the server it is stored as three 
	/// separate <see cref="float"/> values.
	/// </remarks>
	[JsonIgnore]
	public Vector3 LocalPosition
	{
		get
		{
			return localPosition;
		}
		set
		{
			localPosition = value;
		}
	}

	/// <summary>
	/// Gets or sets the name of the source that triggered the event.
	/// </summary>
	[JsonProperty(PropertyName = "sourceName")]
	public string SourceName { get; set; }


	/// <summary>
	/// Gets or sets the time that the event occurred.
	/// </summary>
	/// <remarks>
	/// The value is accessed locally as a <see cref="DateTimeOffset"/> but on the server it is stored as UTC time and an offset in minutes.
	/// </remarks>
	[JsonIgnore]
	public DateTimeOffset Time
	{
		get
		{
			if (calcTime)
			{
				calcTime = false;
				time = new DateTimeOffset(DateTime.SpecifyKind(TimeUtc.ToUniversalTime(), DateTimeKind.Unspecified), TimeSpan.FromMinutes(TimeOffset));
			}
			return time;
		}
		set
		{
			if (time != value)
			{
				time = value;
				timeUtc = value.DateTime.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(value.DateTime, DateTimeKind.Utc) : value.DateTime.ToUniversalTime();
				timeOffset = (int)value.Offset.TotalMinutes;
			}
		}
	}

	/// <summary>
	/// Gets or sets the position that the event occurred in world space.
	/// </summary>
	/// <remarks>
	/// The value is accessed worldly as a <see cref="Vector3"/> but on the server it is stored as three 
	/// separate <see cref="float"/> values.
	/// </remarks>
	[JsonIgnore]
	public Vector3 WorldPosition
	{
		get
		{
			return worldPosition;
		}
		set
		{
			worldPosition = value;
		}
	}
	#endregion // Public Properties
}
