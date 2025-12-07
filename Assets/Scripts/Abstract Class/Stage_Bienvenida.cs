using UnityEngine;

public class Stage_Bienvenida : StageBase
{
    private bool etapaFinalizada = false;

    public override void InitStage()
    {
        base.InitStage(); // Maneja el audio y el Log base
        UIManager.Instance?.Log("Toca la pantalla para comenzar la experiencia.");
    }

    public override void UpdateStage()
    {
        base.UpdateStage();

        // Evitar múltiples llamadas si ya se dio la orden de finalizar
        if (etapaFinalizada) return;

        // DETECCIÓN DE INPUT (HÍBRIDO: ANDROID + EDITOR)
        bool toqueDetectado = false;

        // 1. Opción Android: Hay al menos un dedo tocando y acaba de tocar (Began)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            toqueDetectado = true;
        }
        // 2. Opción Editor PC: Click izquierdo
        else if (Input.GetMouseButtonDown(0))
        {
            toqueDetectado = true;
        }

        if (toqueDetectado)
        {
            UIManager.Instance?.Log("Toque detectado -> Avanzando etapa.");

            // Ejemplo de uso de interacción (Opcional, solo para probar el sonido)
            ReproducirSonidoInteraccion();

            etapaFinalizada = true;
            FinishStage();
        }
    }
}