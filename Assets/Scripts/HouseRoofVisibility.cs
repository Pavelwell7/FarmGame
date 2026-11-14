using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class HouseRoofVisibility : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer roof;

    private BoxCollider2D interiorZone;

    private void Awake()
    {
        interiorZone = GetComponent<BoxCollider2D>();
    }

    private void LateUpdate()
    {
        if (player == null || roof == null)
            return;

        bool playerInside = interiorZone.OverlapPoint(
            (Vector2)player.position
        );

        roof.enabled = !playerInside;
    }

    private void OnDisable()
    {
        if (roof != null)
            roof.enabled = true;
    }
}