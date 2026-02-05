using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[DisallowMultipleComponent]
public class BlockIfAlreadySelected : MonoBehaviour, IXRSelectFilter {
    public bool canProcess => isActiveAndEnabled;

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable) {
        bool canSelect = false;
        if (interactable.firstInteractorSelecting == null)
            canSelect = true;
        if (interactable?.firstInteractorSelecting == interactor)
            canSelect = true;
        return canSelect;
    }
}
