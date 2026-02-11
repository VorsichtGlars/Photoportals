using UnityEngine;

namespace VRSYS.Photoportals {

    /*
     * This is a simple script to log events from the inspector. It can be used to debug events that are triggered from the inspector, such as button clicks or slider changes.
     * It can also be used to log events from other scripts by calling the LogEventMessage or LogEventFloat methods.
     */
    public class SimpleEventDebugger : MonoBehaviour {
        public void LogEventMessage(string message) {
            Debug.Log($"LogEventMessage: {message}");
        }

        public void LogEventFloat(float value) {
            Debug.Log($"LogEventFloat: {value}");
        }
    }
}