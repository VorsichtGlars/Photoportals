using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RenderStateController : MonoBehaviour {
    [SerializeField]
    private bool render = true;
    [SerializeField]
    private List<Renderer> renderers;

    #region States
    public void Start() {
        this.GetRenderersInChildren();
        this.ApplyRenderState();
    }


    public void SetRenderState(bool value) {
        this.render = value;
        this.ApplyRenderState();
    }
    #endregion

    [ContextMenu("Activate()")]
    public void Activate() {
        this.SetRenderState(true);
    }

    [ContextMenu("Deactivate()")]
    public void Deactivate() {
        this.SetRenderState(false);
    }

    [ContextMenu("GetRenderersInChildren()")]
    public void GetRenderersInChildren() {
        this.renderers = this.GetComponentsInChildren<Renderer>().ToList<Renderer>();
    }

    private void ApplyRenderState() {
        foreach (var renderer in this.renderers) {
            renderer.enabled = this.render;
        }
    }
}