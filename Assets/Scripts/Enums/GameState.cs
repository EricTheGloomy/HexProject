public enum GameState
{
    Initial,                // Before anything starts
    GameStart,              // Initializing the game
    MapGeneration,          // Generating the map data
    GridInitialization,     // Setting up the grid structure
    LocationsAssigning,     // Assigning special locations (starting point, etc.)
    MapRendering,           // Rendering the map
    FogOfWarInitialization, //Initialize fog of war
    Gameplay,               // Main gameplay loop
}
