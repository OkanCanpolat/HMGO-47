using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class KeyNodeAttribute : NodeAttribute
{
    public List<Connection> Connections = new List<Connection>();

    public List<DoorAnimation> Doors = new List<DoorAnimation>();

    public KeyDoorSoundConfig SoundConfig;

    [Inject] private AudioManager audioManager;
    public override void OnPawnArrival(Pawn pawn, Barrier barrier)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        base.OnPawnArrival(pawn, barrier);

        if (!isActive || !(pawn as PlayerPawn != null))
        {
            return;
        }
        isActive = false;
        HideAllMeshes();
        foreach (Connection connection in Connections)
        {
            connection.FirstNode.AddConnection(connection.SecondNode);
            connection.SecondNode.AddConnection(connection.FirstNode);
        }
        foreach (DoorAnimation door in Doors)
        {
            door.OpenDoor();
        }
        audioManager.PlaySoundOnceAmong(SoundConfig.PickupSounds, SoundConfig.PickupSoundsVolume);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (Connection connection in Connections)
        {
            if (connection.FirstNode != null && connection.SecondNode != null)
            {
                Vector3 to = (connection.FirstNode.transform.position + connection.SecondNode.transform.position) / 2f;
                Gizmos.DrawLine(base.transform.position, to);
            }
        }
    }
}
