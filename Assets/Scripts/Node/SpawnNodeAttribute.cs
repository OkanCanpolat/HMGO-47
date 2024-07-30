using UnityEngine;

public class SpawnNodeAttribute : NodeAttribute
{
    public GameObject pawnPrefab;

    public bool isPassiveAtStart;

    protected virtual void Awake()
    {
        base.Start();

        MeshRenderer component = GetComponent<MeshRenderer>();

        if ((bool)component) Destroy(component);

        GameObject gameObject = Instantiate(pawnPrefab, transform.position, Quaternion.identity);
        Pawn pawn = gameObject.GetComponent<Pawn>();

        if ((bool)pawn)
        {
            OnPawnSpawned(pawn);
            pawn.OnSpawned();
        }
    }

    protected virtual void OnPawnSpawned(Pawn pawn)
    {
        pawn.IsPassive = isPassiveAtStart;
    }
}
