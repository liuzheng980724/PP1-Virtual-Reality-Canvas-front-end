using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Snobal.Library
{
    [Serializable]
    public class SnobalException : Exception
    {
        // Common error messages
        public const string MSG_UnexpectedError = "An unexpected error occured and the application could not continue.";
        public const string MSG_NoSuchVendor = "Unhandled vendor SDK";

        public string userExplanation;
        public SnobalException()
        {
        }
        public SnobalException(string message) : base(message)
        {
            this.userExplanation = MSG_UnexpectedError;
        }

        public SnobalException(string message, string userExplanation) : base(message)
        {
            this.userExplanation = userExplanation;
        }
        public SnobalException(string message, Exception innerException) : base(message, innerException)
        {
            this.userExplanation = MSG_UnexpectedError;
        }

        public SnobalException(string message, string userExplanation, Exception innerException) : base(message, innerException)
        {
            this.userExplanation = userExplanation;
        }

        protected SnobalException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        
        public string ExceptionType()
        {
            return this.GetType().Name;
        }
    }

    [Serializable]
    public class SnobalNoSessionException : SnobalException
    {
        public SnobalNoSessionException()
        {
        }

        public SnobalNoSessionException(string message, string userExplanation) : base(message, userExplanation)
        {
        }

        public SnobalNoSessionException(string message, string userExplanation, Exception innerException) : base(message, userExplanation, innerException)
        {
        }

        protected SnobalNoSessionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class SnobalBadRequestException : SnobalException
    {
        public SnobalBadRequestException()
        {
        }

        public SnobalBadRequestException(string message, string userExplanation) : base(message, userExplanation)
        {
        }
    }

    [Serializable]
    public class SnobalHttpException : SnobalException
    {
        public SnobalHttpException()
        {
        }

        public SnobalHttpException(string message, string userExplanation) : base(message, userExplanation)
        {
        }

        public SnobalHttpException(string message, string userExplanation, Exception innerException) : base(message,
            userExplanation, innerException)
        {
        }

        protected SnobalHttpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
    
    [Serializable]
    public class SnobalLoginFailedException : SnobalException
    {
    }
}
