using UnityEditor;

// Keeps the Unity MCP local HTTP bridge enabled for the Codex MCP server.
[InitializeOnLoad]
internal static class McpCodexAutoSetup
{
    static McpCodexAutoSetup()
    {
        EditorPrefs.SetBool("MCPForUnity.UseHttpTransport", true);
        EditorPrefs.SetString("MCPForUnity.HttpTransportScope", "local");
        EditorPrefs.SetString("MCPForUnity.HttpUrl", "http://127.0.0.1:8080/mcp");
        EditorPrefs.SetBool("MCPForUnity.AutoStartOnLoad", true);
        EditorPrefs.SetBool("MCPForUnity.HttpServerLaunchConfirmed", true);
    }
}
