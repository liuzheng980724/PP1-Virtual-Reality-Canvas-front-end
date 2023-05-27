namespace Snobal.Library
{
	public static class Unity
	{
		/// <summary> Returns the path where the AssetBundles are located </summary>
		public static string GetAssetFileLocation()
		{
			return FileIO.GetXRContentDataPath();
		}
	}
}
