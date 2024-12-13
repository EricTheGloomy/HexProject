public enum GameState
{
    Initial,                // Before anything starts
    GameStart,              // Initializing the game
    GridInitialization,     // Setting up the grid structure
    MapGeneration,          // Generating the map data
    LocationsAssigning,     // Assigning special locations (starting point, etc.)
    MapRendering,           // Rendering the map
    FogOfWarInitialization, //Initialize fog of war
    CameraInitialization,   //Initialize the camera
    Gameplay,               // Main gameplay loop
}
