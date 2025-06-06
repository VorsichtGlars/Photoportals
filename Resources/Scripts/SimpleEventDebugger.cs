using UnityEngine;

namespace VRVIS.Photoportals {
    public class SimpleEventDebugger {
        public void LogEventMessage(string message) {
            Debug.Log($"Event: {message}");
        }
    }

}