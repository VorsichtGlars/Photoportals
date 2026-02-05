using System.Collections.Generic;
using UnityEngine;

public class TestPortalSystemControl : MonoBehaviour {

    [SerializeField]
    List<RenderStateController> widgetsAndGizmosRenderStateControllers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    [ContextMenu("RetrieveAllWidgetsAndGizmos()")]
    private void RetrieveAllWidgetsAndGizmos() {
        foreach (var item in GameObject.FindGameObjectsWithTag("3DUI")) {
            var controller = item.GetComponent<RenderStateController>();
            if (controller == null)
                return;
            this.widgetsAndGizmosRenderStateControllers.Add(controller);
        }
    }

    [ContextMenu("EnableAllWidgetsAndGizmos()")]
    private void EnableAllWidgetsAndGizmos() {
        foreach (var controller in this.widgetsAndGizmosRenderStateControllers) {
            controller.Activate();
        }
    }

    [ContextMenu("DisableAllWidgetsAndGizmos()")]
    private void DisableAllWidgetsAndGizmos() {
        foreach (var controller in this.widgetsAndGizmosRenderStateControllers) {
            controller.Deactivate();
        }
    }
}