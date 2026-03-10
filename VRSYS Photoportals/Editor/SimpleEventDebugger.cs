using System.Collections.Generic;
using UnityEngine;

namespace VRSYS.Photoportals {

    /*
     * This is a simple script to log events from the inspector. It can be used to debug events that are triggered from the inspector, such as button clicks or slider changes.
     * It can also be used to log events from other scripts by calling the LogEventMessage or LogEventFloat methods.
     */
    public class SimpleEventDebugger : MonoBehaviour {
        private List<string> history = new List<string>();
        public IReadOnlyList<string> History => history;

        public void LogEventMessage(string message) {
            string logEntry = $"[{System.DateTime.Now:HH:mm:ss}] Message: {message}";
            Debug.Log(logEntry);
            history.Add(logEntry);
        }

        public void LogEventFloat(float value) {
            string logEntry = $"[{System.DateTime.Now:HH:mm:ss}] Float: {value}";
            Debug.Log(logEntry);
            history.Add(logEntry);
        }

        public void ClearHistory() {
            history.Clear();
        }
    }
}