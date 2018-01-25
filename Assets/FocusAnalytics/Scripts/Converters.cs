using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// A json converter for <see cref="DateTimeOffset"/> that preserves offset when transferring to and from Mobile Services.
/// </summary>
/// <remarks>
/// The implementation of this converter is covered in this 
/// <see href="https://blogs.msdn.microsoft.com/carlosfigueira/2013/05/13/preserving-date-time-offsets-in-azure-mobile-services/">blog post</see>. 
/// <b>IMPORTANT:</b> This converter does require custom insert, update and read scripts in the mobile service. These scripts are documented in the 
/// above blog post.
/// </remarks>
public class DtoPreservingOffsetConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(DateTimeOffset);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Dictionary<string, object> temp = serializer.Deserialize<Dictionary<string, object>>(reader);
		DateTime dateTimeUTC = ((DateTime)temp["DateTimeUTC"]).ToUniversalTime();
		int offsetMinutes = Convert.ToInt32(temp["OffsetMinutes"]);
		return new DateTimeOffset(
			DateTime.SpecifyKind(dateTimeUTC, DateTimeKind.Unspecified),
			TimeSpan.FromMinutes(offsetMinutes));
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		DateTimeOffset dto = (DateTimeOffset)value;
		DateTime dt = dto.DateTime.Kind == DateTimeKind.Unspecified ?
			DateTime.SpecifyKind(dto.DateTime, DateTimeKind.Utc) :
			dto.DateTime.ToUniversalTime();

		Dictionary<string, object> temp = new Dictionary<string, object>
	{
		{ "DateTimeUTC", dt },
		{ "OffsetMinutes", dto.Offset.TotalMinutes }
	};

		serializer.Serialize(writer, temp);
	}
}