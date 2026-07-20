using UnityEngine;

public enum SocialAnchorType
{
    CentralSocialCircle,
    DiningShared,
    KitchenShared,
    GameActivity,
    LoungeConversation,
    TerracePrivatePair,
    WithdrawalSolo
}

/// <summary>
/// Scene-authored destination data for future contestants. This component does
/// not own AI or story logic; it only describes capacity and safe approach slots.
/// </summary>
public sealed class SocialInteractionAnchor : MonoBehaviour
{
    [SerializeField] private SocialAnchorType anchorType;
    [SerializeField, Min(1)] private int capacity = 2;
    [SerializeField] private Transform facingPoint;
    [SerializeField] private Transform[] slots;
    [SerializeField] private bool availableDuringDay = true;
    [SerializeField] private bool availableDuringNight = true;

    public SocialAnchorType AnchorType => anchorType;
    public int Capacity => capacity;
    public Transform FacingPoint => facingPoint;
    public Transform[] Slots => slots;
    public bool AvailableDuringDay => availableDuringDay;
    public bool AvailableDuringNight => availableDuringNight;

    public void Configure(SocialAnchorType type, int requestedCapacity, Transform lookAt, Transform[] contestantSlots)
    {
        anchorType = type;
        capacity = Mathf.Max(1, requestedCapacity);
        facingPoint = lookAt;
        slots = contestantSlots;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = anchorType == SocialAnchorType.WithdrawalSolo
            ? new Color(0.25f, 0.75f, 1f, 0.9f)
            : new Color(1f, 0.55f, 0.2f, 0.9f);

        if (slots == null) return;
        foreach (Transform slot in slots)
        {
            if (slot == null) continue;
            Gizmos.DrawWireSphere(slot.position, 0.4f);
            if (facingPoint != null) Gizmos.DrawLine(slot.position, facingPoint.position);
        }
    }
}
