namespace Netherlands3D.Core.Tiles
{
    /// <summary>
    /// Tile prioritiser optimised for WebGL where we cant use threading.
    /// Modern browsers like Chrome limits parralel downloads from host to 6 per tab.
    /// Threading is not supported for WebGL, so this prioritiser spreads out actions to reduce framedrop spikes.
    /// </summary>
    public class WebTilePrioritiser : TilePrioritiser
    {
        public override void LoadTileContent(Tile tile)
        {
            //check the current active downloads
            //max one load per frame
            //some need to be interupted, cleared for this one?
            //center of screen priority
        }

        public override void RemoveTileContent(Tile tile)
        {
            //Clean up as fast as possible, directly dispose
            tile.Dispose();
        }
    }
}