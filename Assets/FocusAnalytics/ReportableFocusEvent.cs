using System;
using Newtonsoft.Json;
using UnityEngine;

public class ReportableFocusEvent
{
    public ReportableFocusEvent()
    {

    }

    public ReportableFocusEvent(string token, string label, DateTimeOffset start, DateTimeOffset end, Vector3 position)
    {
        this.PackageSpecificToken = token;

        this.Label = label;

        this.Start = JsonConvert.ToString(start);

        TimeSpan ts = end - start;

        this.Duration = JsonConvert.ToString(ts);

        this.Position = string.Format("{0},{1},{2}", position.x, position.y, position.z);
    }

	public string Id { get; set; }

	[JsonProperty(PropertyName = "token")]
	public string PackageSpecificToken { get; set; }

	[JsonProperty(PropertyName = "label")]
    public string Label { get; set; }

	[JsonProperty(PropertyName = "start")]
	public string Start { get; set; }

	[JsonProperty(PropertyName = "duration")]
    public string Duration { get; set; }

	[JsonProperty(PropertyName = "position")]
	public string Position { get; set; }
}
