using ColorBlockJam.Core.Editing;

namespace ColorBlockJam.LevelEditor
{
    internal sealed class EraseTool : IBoardTool
    {
        private readonly IEditingContext _context;

        public EraseTool(IEditingContext context)
        {
            _context = context;
        }

        public string Label => "Erase";
        public string Hint => "Click a block or a door to remove it. Right click does the same with any tool.";

        public void PointerDown(BoardHit hit)
        {
            if (hit.Kind == BoardHitKind.Cell)
            {
                int block = LevelEditing.FindBlockAt(_context.Level, hit.Cell);
                if (block != LevelEditing.NotFound)
                {
                    _context.Edit("Remove Block", level => level.Blocks.RemoveAt(block));
                }
            }
            else if (hit.Kind == BoardHitKind.DoorSlot)
            {
                int door = LevelEditing.FindDoorAt(_context.Level, hit.Side, hit.Position);
                if (door != LevelEditing.NotFound)
                {
                    _context.Edit("Remove Door", level => level.Doors.RemoveAt(door));
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
