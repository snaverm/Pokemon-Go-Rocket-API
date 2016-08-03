using Windows.Storage;

namespace PokemonGo_UWP.Utils
{
	public static class LocalStorage
	{
		public static T GetStorageValue<T>(string key)
		{
			// Get the local storage container
			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
			// Get the value of the property
			if (localSettings.Values.ContainsKey(key))
				return (T)localSettings.Values[key];
			else
				return default(T);
		}

		public static T SetStorageValue<T>(string key, T value)
		{
			// Get the local storage container
			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
			// Set the value to the property
			localSettings.Values[key] = value;
			return value;
		}
	}
}
