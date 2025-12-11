using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// Streamer.bot C# Action for triggering Chaos Mod random effects
// 
// Usage:
// 1. Copy this code into a Streamer.bot C# Action (Inline mode)
// 2. Assign it to chat commands, channel point redeems, cheer events, etc.
// 3. Configure the port if you changed it in the mod's config file
//
// Features:
// - Two-way communication with confirmation
// - Automatic retry on failure
// - Request ID correlation for concurrent requests
// - Configurable duration via global variable

public class CPHInline
{
    // WebSocket configuration
    private const int WEBSOCKET_PORT = 42071; // Default direct connection port
    private const string WEBSOCKET_URL = "ws://localhost";
    private const int RESPONSE_TIMEOUT_MS = 2000; // 2 seconds to wait for response
    private const int MAX_RETRIES = 3;
    
    // Thread-safe request ID counter
    private static int requestIdCounter = 0;

    // Simple helper to extract JSON string value
    private static string GetJsonValue(string json, string key)
    {
        string searchKey = "\"" + key + "\":";
        int keyIndex = json.IndexOf(searchKey);
        if (keyIndex < 0) return null;
        
        int valueStart = keyIndex + searchKey.Length;
        while (valueStart < json.Length && (json[valueStart] == ' ' || json[valueStart] == '\t'))
            valueStart++;
        
        if (valueStart >= json.Length) return null;
        
        // String value (quoted)
        if (json[valueStart] == '"')
        {
            int start = valueStart + 1;
            int end = json.IndexOf('"', start);
            if (end < 0) return null;
            return json.Substring(start, end - start);
        }
        // Boolean or number value
        else
        {
            int end = valueStart;
            while (end < json.Length && json[end] != ',' && json[end] != '}' && json[end] != ' ')
                end++;
            return json.Substring(valueStart, end - valueStart).Trim();
        }
    }

    /// <summary>
    /// Triggers a random Chaos Mod effect
    /// </summary>
    /// <param name="durationMs">Duration in milliseconds (default: 30000 = 30 seconds)</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool TriggerRandomEffect(int durationMs = 30000)
    {
        // Generate unique request ID for correlation
        int requestId = Interlocked.Increment(ref requestIdCounter);

        for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
        {
            try
            {
                // Build the JSON message
                string jsonMessage = $"{{\"type\":\"randomEffect\",\"id\":{requestId},\"duration\":{durationMs}}}";
                
                if (attempt > 1)
                {
                    CPH.LogInfo($"Chaos Mod: Retry attempt {attempt}/{MAX_RETRIES} (Request ID: {requestId})...");
                }
                else
                {
                    CPH.LogInfo($"Chaos Mod: Sending random effect trigger (Request ID: {requestId})...");
                }

                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                Task<(bool success, string message)> task = Task.Run(async () =>
                {
                    using (var client = new ClientWebSocket())
                    {
                        var uri = new Uri($"{WEBSOCKET_URL}:{WEBSOCKET_PORT}");
                        await client.ConnectAsync(uri, cts.Token);

                        if (client.State == WebSocketState.Open)
                        {
                            // Send message
                            byte[] sendBytes = Encoding.UTF8.GetBytes(jsonMessage);
                            await client.SendAsync(new ArraySegment<byte>(sendBytes), WebSocketMessageType.Text, true, cts.Token);
                            CPH.LogDebug($"Chaos Mod: Message sent (ID: {requestId}), waiting for response...");

                            // Wait for response with timeout
                            byte[] responseBuffer = new byte[4096];
                            var responseCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(RESPONSE_TIMEOUT_MS));
                            
                            try
                            {
                                var result = await client.ReceiveAsync(new ArraySegment<byte>(responseBuffer), responseCts.Token);
                                
                                if (result.MessageType == WebSocketMessageType.Text)
                                {
                                    string responseStr = Encoding.UTF8.GetString(responseBuffer, 0, result.Count);
                                    CPH.LogDebug($"Chaos Mod: Received response: {responseStr}");

                                    // Simple JSON parsing - extract values we need
                                    string responseType = GetJsonValue(responseStr, "type");
                                    
                                    if (responseType == "randomEffectResponse")
                                    {
                                        // Verify request ID matches
                                        string idStr = GetJsonValue(responseStr, "id");
                                        if (idStr != null)
                                        {
                                            int responseId = int.Parse(idStr);
                                            if (responseId != requestId)
                                            {
                                                CPH.LogWarn($"Chaos Mod: Response ID mismatch! Expected {requestId}, got {responseId}");
                                                return (false, $"Response ID mismatch (expected {requestId}, got {responseId})");
                                            }
                                        }

                                        string successStr = GetJsonValue(responseStr, "success");
                                        bool success = successStr == "true";
                                        
                                        if (success)
                                        {
                                            string effectID = GetJsonValue(responseStr, "effectID") ?? "unknown";
                                            string effectName = GetJsonValue(responseStr, "effectName") ?? effectID;
                                            
                                            // Set effect name as a variable for Streamer.bot automation
                                            CPH.SetGlobalVar("ChaosModLastEffectName", effectName, false);
                                            CPH.SetGlobalVar("ChaosModLastEffectID", effectID, false);
                                            
                                            CPH.LogInfo($"Chaos Mod: Success! Effect '{effectName}' (ID: {effectID}) triggered (Request ID: {requestId}).");
                                            return (true, effectName);
                                        }
                                        else
                                        {
                                            string reason = GetJsonValue(responseStr, "reason") ?? "Unknown error";
                                            return (false, reason);
                                        }
                                    }
                                    else
                                    {
                                        return (false, "Invalid response format");
                                    }
                                }
                            }
                            catch (TaskCanceledException)
                            {
                                return (false, "Response timeout - no answer from mod");
                            }

                            // Close connection cleanly
                            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", cts.Token);
                            return (false, "No response received");
                        }
                        return (false, $"Connection state: {client.State}");
                    }
                }, cts.Token);

                (bool success, string message) result = task.Result;

                if (result.success)
                {
                    return true;
                }
                else
                {
                    CPH.LogWarn($"Chaos Mod: Attempt {attempt} failed (ID: {requestId}) - {result.message}");
                    
                    // Wait before retry (except on last attempt)
                    if (attempt < MAX_RETRIES)
                    {
                        Thread.Sleep(500); // Wait 500ms before retry
                    }
                }
            }
            catch (TaskCanceledException)
            {
                CPH.LogWarn($"Chaos Mod: Attempt {attempt} (ID: {requestId}) - Connection timeout");
                if (attempt < MAX_RETRIES) Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                CPH.LogError($"Chaos Mod Error (attempt {attempt}, ID: {requestId}): {ex.GetType().Name} - {ex.Message}");
                if (attempt < MAX_RETRIES) Thread.Sleep(500);
            }
        }

        CPH.LogError($"Chaos Mod: All retry attempts failed (Request ID: {requestId}). Make sure game is running and mod is loaded.");
        return false;
    }

    /// <summary>
    /// Streamer.bot execute method - called when the action runs
    /// </summary>
    public bool Execute()
    {
        // Get duration from global variable if set, otherwise use default 30 seconds
        int duration = CPH.GetGlobalVar<int>("ChaosModDuration", true);
        if (duration == 0) duration = 30000; // Default 30 seconds
        
        return TriggerRandomEffect(duration);
    }
}

