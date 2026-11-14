namespace ColorBlockJam.Core.Players
{
    /// <summary>Where the player's data is kept between runs of the game.</summary>
    public interface IPlayerStorage
    {
        /// <summary>Returns false when nothing usable is stored, which is the case for a new player.</summary>
        bool TryLoad(out PlayerData data);

        void Save(PlayerData data);
    }
}
