using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class DisableTeleportation : MonoBehaviour
{
    public TeleportationProvider teleportationProvider;

    void Start()
    {
        if (teleportationProvider != null)
        {
            teleportationProvider.enabled = false;
        }
    }
}
