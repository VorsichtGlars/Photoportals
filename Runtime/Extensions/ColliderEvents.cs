using UnityEngine;
using UnityEngine.Events;

/*
 * This script triggers UnityEvents when a collider enters or exits the trigger collider attached to this GameObject.
*/
namespace VRSYS.Photoportals.Extensions
{
public class ColliderEvents : MonoBehaviour {

    public Collider collider;
    public Collider other;
    public bool isColliding;
    public UnityEvent<Collider> OnEnter;
    public UnityEvent<Collider> OnExit;
    public UnityEvent<Collider> OnStay;

    void OnTriggerEnter(Collider other) {
        this.isColliding = true;
        this.other = other;
        this.OnEnter.Invoke(other);
    }

    void OnTriggerExit(Collider other) {
        this.isColliding = false;
        this.other = null;
        this.OnExit.Invoke(other);
    }

    void OnTriggerStay(Collider other) {
        this.OnStay.Invoke(other);
    }

}
}