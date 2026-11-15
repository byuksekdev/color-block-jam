using ColorBlockJam.Core.Boards;
using UnityEngine;

namespace ColorBlockJam.Game
{
    /// <summary>How forgiving a drag feels. The game scene hands these values to the rules of every level.</summary>
    public sealed class DragTuning : ScriptableObject
    {
        [Tooltip("How far, in cells, the dragged block may sink into another block. 0 stops it exactly at the cell line; a larger value lets it pass between blocks without being lined up exactly.")]
        [Range(0f, 0.45f)]
        [SerializeField] private float _blockOverlap = 0.15f;

        [Tooltip("How far, in cells, a block may still be from the door's side of the board when its door takes it. 0.5 takes it as soon as it stands on the cell at the door.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float _doorReach = 0.5f;

        public DragRules Rules => new DragRules(_blockOverlap, _doorReach);
    }
}
