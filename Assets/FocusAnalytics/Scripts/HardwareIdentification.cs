using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HardwareIdentification
{
	#region Member Variables
	static private string cachedToken;
	#endregion // Member Variables

	/// <summary>
	/// Gets the package specific identification token for the application. 
	/// </summary>
	/// <remarks>
	/// When running under the Unity Editor this method returns the string "Unity Editor". When not running under UWP, this method 
	/// returns the string "Non-UWP Application".
	/// </remarks>
	/// <returns>
	/// The package specific token string for the application.
	/// </returns>
	public static string GetPackageSpecificToken()
	{
		if (cachedToken == null)
		{
			#if WINDOWS_UWP
				var token = Windows.System.Profile.HardwareIdentification.GetPackageSpecificToken(null);
				var hardwareId = token.Id;
				var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(hardwareId);

				byte[] bytes = new byte[hardwareId.Length];
				dataReader.ReadBytes(bytes);

				cachedToken = BitConverter.ToString(bytes);
			#elif UNITY_EDITOR
				cachedToken = "Unity Editor";
			#else
				cachedToken = "Non-UWP Application";
			#endif
		}

		return cachedToken;
	}
}
