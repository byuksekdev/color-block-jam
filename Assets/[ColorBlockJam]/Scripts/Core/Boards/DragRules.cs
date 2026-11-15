using System;

namespace ColorBlockJam.Core.Boards
{
    /// <summary>How forgiving a drag is. Both values are in cells.</summary>
    public readonly struct DragRules
    {
        /// <summary>At half a cell the dragged block would stand on the cell of its neighbour.</summary>
        private const float MaxBlockOverlap = 0.45f;

        /// <summary>Further away than half a cell the block is on another cell, which does not touch the door.</summary>
        private const float MaxDoorReach = 0.5f;

        public DragRules(float blockOverlap, float doorReach)
        {
            BlockOverlap = Math.Clamp(blockOverlap, 0f, MaxBlockOverlap);
            DoorReach = Math.Clamp(doorReach, 0f, MaxDoorReach);
        }

        /// <summary>How far the dragged block may sink into another block before it stops.</summary>
        public float BlockOverlap { get; }

        /// <summary>How far from the door's side of the board a block may still be when its door takes it.</summary>
        public float DoorReach { get; }
    }
}
