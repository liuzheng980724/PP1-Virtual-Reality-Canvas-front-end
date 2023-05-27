using System;
using System.Collections.Generic;
using Snobal.Utilities;

namespace Snobal.Library
{
	public class Features
	{
		[Serializable]
		public abstract class FeatureData
		{
			public bool enabled;
			public long expires;

			public abstract U GetCapabilities<U>();
		}

		[Serializable]
		public class FeatureData<T> : FeatureData
		{
			public T capabilities;
			public override U GetCapabilities<U>()
            {
				if (typeof(U) == typeof(T))
                {
					return (U)Convert.ChangeType(capabilities, typeof(T));
				}

				return default;
			}
		}

		[Serializable]
		public class MultiuserCapability
		{
			public int maxUsers;
		}

		private abstract class FeatureBase
		{
			public FeatureBase()
			{
			}

			public abstract void Deserialise(string featureBlob);
			public abstract FeatureData GetFeatureData();
		}

		private class Feature<T> : FeatureBase
		{
			public FeatureData<T> featureData;

			public Feature()
			{
			}

			public override void Deserialise(string featureBlob)
			{
				featureData = Serialization.DeserializeToType<FeatureData<T>>(featureBlob);
			}

			public override FeatureData GetFeatureData()
            {
				return featureData;
			}

		}

		private static Dictionary<string, FeatureBase> features = new Dictionary<string, FeatureBase>()
			{ {"multi-user", new Feature<MultiuserCapability>()} };

		public static bool IsInitialised { get; private set; }

		public static void GetFeatures(Tenant tenant, IRestClient restClient)
		{
			foreach (var f in features)
            {
				var featureBlob = restClient.Get(tenant.BuildTenantAPIURL(API.licenseExtension+(f.Key))).Response;
				f.Value.Deserialise(featureBlob);
			}

			IsInitialised = true;
		}

		public static FeatureData GetFeatureData(string featureName)
		{
			if (features.ContainsKey(featureName))
            {
				return features[featureName].GetFeatureData();
			}

			return null;
		}
	}

}
