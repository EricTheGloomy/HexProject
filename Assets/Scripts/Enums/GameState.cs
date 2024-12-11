public enum GameState
{
    Initial,            // Before anything starts
    GameStart,          // Initializing the game
    MapGeneration,      // Generating the map data
    GridInitialization, // Setting up the grid structure
    LocationsAssigning, // Assigning special locations (starting point, etc.)
    MapRendering,       // Rendering the map
    Gameplay,           // Main gameplay loop
}
