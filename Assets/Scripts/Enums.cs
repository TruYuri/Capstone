/// <summary>
/// Various enumerations used throughout the game.
/// </summary>

public enum Inhabitance { Uninhabited, Primitive, Industrial, SpaceAge } // Planet inhabitance types.
public enum Resource { NoResource, Forest, Ore, Oil, Asterminium, Stations } // Resource types.
public enum ResourceGatherType 
{ 
    None = 0, 
    Research = 1 << 0, 
    Soldiers = 1 << 1, 
    Natural = 1 << 2 
} // Structure resource gather types (bitfield)
public enum TileSize { Small, Large } // Size of a tile
public enum Team { Uninhabited, Union, Kharkyr, Plinthen, Indigenous } // List of teams
public enum GameEventStage { Begin, End, Continue } // Event stages
public enum ShipProperties 
{
    None = 0, 
    Untransferable = 1 << 0,
    Structure = (1 << 1) | (1 << 2),
    GroundStructure = 1 << 1, 
    SpaceStructure = 1 << 2, 
    ResourceTransport = 1 << 3 
} // Configurable ship properties (bitfield)
public enum BattleType { Space = 1, Invasion = 2 } // Battle types
public enum ListingType { Info, Build } // UI listing types
