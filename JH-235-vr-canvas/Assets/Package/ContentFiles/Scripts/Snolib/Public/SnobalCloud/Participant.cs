using System;
using Snobal.Utilities;

namespace Snobal.Library
{ 
    [Serializable]
    public class Participant
    {
        public string participantId = null;
        public string email = null;
        public string name = null;
        public string avatarUrl = null;
        public string firstName { get { return name.Split(' ')[0]; } }
        public string fullName { get { return name; } }

        public static bool ValidatePasscode(IRestClient _restClient, Tenant _tenant, string _passcode, out Participant _participant)
        {
            var body = Serialization.SerializeObject(new { passcode = _passcode });
            var response = _restClient.Post(_tenant.BuildTenantAPIURL(API.validatePasscode), body);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                _participant = Serialization.DeserializeToType<Participant>(response.Response);
                return true;
            }
            if (response.HttpStatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                _participant = null;
                return false;
            }
            throw new SnobalException("Unhandled status code coming from Post(ValidatePasscode): " + response.HttpStatusCode.ToString(), "There was an error attempting to validate your passcode");
        }
      
        public  static Participant Deserialize(string jsonString)
        {
            Logger.Log("Configuring Participant.");

            if (string.IsNullOrEmpty(jsonString))
            {
                Logger.Log("No participant found, user will need to be validated");
                return null;
            }
            return Serialization.DeserializeToType<Participant>(jsonString);
        }
    }
}
       
