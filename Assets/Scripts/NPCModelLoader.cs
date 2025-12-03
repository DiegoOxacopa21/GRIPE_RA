using UnityEngine;

public class NPCModelLoader : MonoBehaviour
{
    public Transform modelParent;

    GameObject currentModel;
    Animator modelAnimator;
    Renderer[] modelRenderers;

    public void ApplyModel(GameObject modelPrefab)
    {
        if (modelParent == null)
        {
            UIManager.Instance?.Log("ERROR NPCModelLoader: Falta modelParent");
            return;
        }

        // borrar modelo anterior
        for (int i = modelParent.childCount - 1; i >= 0; i--)
            DestroyImmediate(modelParent.GetChild(i).gameObject);

        currentModel = Instantiate(modelPrefab, modelParent);
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;
        currentModel.transform.localScale = Vector3.one;

        modelAnimator = currentModel.GetComponentInChildren<Animator>();
        modelRenderers = currentModel.GetComponentsInChildren<Renderer>();
    }

    public Animator GetModelAnimator() => modelAnimator;
    public Renderer[] GetModelRenderers() => modelRenderers;
}
