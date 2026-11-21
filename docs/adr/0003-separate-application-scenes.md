# Separate application scenes and share gameplay prefabs

The foundation uses Title, InGame, Editor, and Sandbox scenes with ordinary scene replacement and scene-local gameplay sessions. This separates player navigation, gameplay, future authoring, and developer testing without an additional persistent bootstrap scene. InGame and Sandbox share gameplay prefabs and scripts so testing exercises the same mechanics as the player-facing runtime.

Editor is an inaccessible placeholder until authoring work begins. Sandbox is opened directly in Unity. Title offers five unavailable level slots and a temporary mechanics-demo entry, preserving the requirement to create no authored levels. Returning to Title discards the session, while Reset within gameplay retains the program.

The project-layout migration renamed the Editor scene to LevelEditor to distinguish it from Unity editor tooling. Its responsibilities and accessibility remain unchanged.
