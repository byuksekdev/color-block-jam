using ColorBlockJam.Core.Editing;
using UnityEngine.UIElements;

namespace ColorBlockJam.LevelEditor
{
    /// <summary>Puts ice on a block or a door, or takes it off: the chosen number is how many blocks have to leave before it melts.</summary>
    internal sealed class IceTool : IBoardTool
    {
        private const int FirstCount = 1;

        private readonly IEditingContext _context;
        private readonly Stepper _count = new Stepper("Blocks that must leave first", 0, LevelEditing.MaxIceCount);

        public IceTool(IEditingContext context)
        {
            _context = context;
            _count.AddToClassList("wide-stepper");
            _count.SetValueWithoutNotify(FirstCount);
        }

        public string Label => "Ice";
        public string Hint => "Click a block or a door to freeze it. It melts when the chosen number of blocks has left the level. Choose 0 to take the ice off.";
        public VisualElement Options => _count;

        public void PointerDown(BoardHit hit)
        {
            int count = _count.Value;
            if (hit.Kind == BoardHitKind.Cell)
            {
                int block = LevelEditing.FindBlockAt(_context.Level, hit.Cell);
                if (block != LevelEditing.NotFound)
                {
                    _context.Edit("Set Block Ice", level => LevelEditing.SetBlockIce(level, block, count));
                }
            }
            else if (hit.Kind == BoardHitKind.DoorSlot)
            {
                int door = LevelEditing.FindDoorAt(_context.Level, hit.Side, hit.Position);
                if (door != LevelEditing.NotFound)
                {
                    _context.Edit("Set Door Ice", level => LevelEditing.SetDoorIce(level, door, count));
                }
            }
        }

        public void PointerMove(BoardHit hit)
        {
        }

        public void PointerUp(BoardHit hit)
        {
        }

        public void Cancel()
        {
        }
    }
}
