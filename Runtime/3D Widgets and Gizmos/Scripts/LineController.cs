using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class LineController : MonoBehaviour {

    [SerializeField]
    private Transform start;
    [SerializeField]
    private Transform end;
    private LineRenderer lineRenderer;

    [SerializeField]
    private float width = 0.01f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        this.lineRenderer = this.GetComponent<LineRenderer>();
        this.lineRenderer.startWidth = this.lineRenderer.endWidth = this.width;
    }

    // Update is called once per frame
    void Update() {
        if (this.start != null && this.start.hasChanged) {
            this.transform.position = this.start.position;
            this.transform.rotation = this.start.rotation;
            this.lineRenderer.SetPosition(0, this.start.position);
        }
        if (this.end != null && this.end.hasChanged) {
            this.lineRenderer.SetPosition(1, this.end.position);
        }
    }
}