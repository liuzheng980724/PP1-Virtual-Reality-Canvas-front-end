using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Snobal.Library;

namespace Snobal.CloudSDK
{
    /// <summary>
    /// When another class throws an exception,
    /// 1) the exception is added to our StoredExceptions array
    /// 2) we set scheduledForTermination = true
    /// During the update loop, because this is true, it calls the Terminate method with each exception
    /// for each one, an event is thrown with that Exception
    ///
    /// The Cloud Deploy state machine is subscribed to this event. When it is thrown,
    /// it automatically will change states to the error state
    /// </summary>
    public class SnobalErrorHandler : MonoBehaviour
    {
        public static System.Action<SnobalException> OnExceptionTermination;

        private static List<SnobalException> storedExceptions = new List<SnobalException>();
        private static bool scheduledForTermination = false;

        private static bool isInitialised = false;

        private static SnobalErrorHandler errorHandler;

        public static void Init()
        {
            errorHandler = FindObjectOfType<SnobalErrorHandler>();
            if (errorHandler == null)
            {
                errorHandler = new GameObject("SnobalErrorHandler").AddComponent<SnobalErrorHandler>();
                if (Application.isPlaying)
                    GameObject.DontDestroyOnLoad(errorHandler.gameObject);
            }

            Application.logMessageReceived += HandleExceptionLog;

            isInitialised = true;
        }

        public static void Shutdown()
        {
            if (isInitialised)
            {
                storedExceptions.Clear();

                if (errorHandler != null)
                {
                    Destroy(errorHandler.gameObject);
                    errorHandler = null;
                }

                Application.logMessageReceived -= HandleExceptionLog;

                isInitialised = false;
            }
        }

        public void Update()
        {
            if (scheduledForTermination)
            {
                var exceptions = storedExceptions.ToArray();
                for (var i = 0; i < exceptions.Length; i++)
                {
                    Terminate(exceptions[i]);
                }
                storedExceptions.Clear();
            }
        }

        public static void Throw(SnobalException _snobalException)
        {
            scheduledForTermination = true;
            storedExceptions.Add(_snobalException);
        }

        public static void Throw(System.AggregateException _e)
        {
            UnityEngine.Debug.Log(_e.Message);
            SnobalException snobalException = new SnobalException(_e.InnerException.Message, "An error occured", _e.InnerException);
            Throw(snobalException);
        }

        private static void HandleExceptionLog(string _logString, string _stackTrace, LogType _type)
        {
            if (_type != LogType.Exception)
                return;

            Throw(new SnobalException(string.Format("Unity unhandled exception:\n{0}\n{1}", _logString, _stackTrace)));
        }

        private static void Terminate(SnobalException _snobalException)
        {
            OnExceptionTermination?.Invoke(_snobalException);
            //Logging.LogSnobalException(_snobalException);
            DisplayError(_snobalException.ToString());
        }

        private static void DisplayError(string _message)
        {
            // Compile all cascaded error messages together
            string formattedMessage = "<b>An exception has been logged to Snolib.txt, Description below</b>";
            formattedMessage += "\n\n" + _message;
            UnityEngine.Debug.LogError(formattedMessage); // just log to unity log for now
            //SnobalApplication.UIManager.ShowError(formattedMessage);
        }
    }

}