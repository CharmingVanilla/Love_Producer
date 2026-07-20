# Love Producer - Outfit Design Set 01

Each concept sheet shows three full outfit swaps in this order:

1. Left: Cozy Indoor
2. Center: Refined Date
3. Right: Beach / Sport

`Concepts 1` remains the identity, body-proportion, hair, and arrival-look reference.

## Runtime wardrobe scope

- Swap complete outfits, not individual tops and bottoms.
- Keep body, head, hair, armature, and animations shared per contestant.
- Build every outfit as separate skinned meshes bound to the same Humanoid skeleton.
- Keep shoes and outfit-specific jewelry inside each outfit prefab.
- Hide covered body polygons or use body masks to prevent clipping.

## Recommended Unity names

```text
Contestant_<Name>
|- Body
|- Head
|- Hair
|- Armature
`- Outfits
   |- Arrival_01
   |- Cozy_01
   |- Date_01
   `- BeachSport_01
```

## Target presentation

- Semi-realistic 3D
- Windows and macOS
- Must hold up in top-down gameplay, medium dialogue shots, and profile close-ups
- Use one shared material convention and one Humanoid animation convention across the cast
