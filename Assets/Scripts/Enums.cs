public enum Inhabitance { Uninhabited, Primitive, Industrial, SpaceAge }
public enum Resource { NoResource, Forest, Ore, Oil, Asterminium, Stations }
public enum ResourceGatherType 
{ 
    None = 0, 
    Research = 1 << 0, 
    Soldiers = 1 << 1, 
    Natural = 1 << 2 
}
public enum TileSize { Small, Large }
public enum Team { Uninhabited, Union, Kharkyr, Plinthen, Indigenous }
public enum GameEventStage { Begin, End, Continue }
public enum ShipProperties 
{
    None = 0, 
    Untransferable = 1 << 0,
    Structure = (1 << 1) | (1 << 2),
    GroundStructure = 1 << 1, 
    SpaceStructure = 1 << 2, 
    ResourceTransport = 1 << 3 
}
public enum BattleType { Space = 1, Invasion = 2 }
public enum ListingType { Info, Build }
