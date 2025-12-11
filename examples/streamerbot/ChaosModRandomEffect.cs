using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
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
                var message = new Dictionary<string, object>
                {
                    ["type"] = "randomEffect",
                    ["id"] = requestId,
                    ["duration"] = durationMs
                };

                string jsonMessage = JsonSerializer.Serialize(message);
                
                if (attempt > 1)
                {
                    CPH.LogInfo($"Chaos Mod: Retry attempt {attempt}/{MAX_RETRIES} (Request ID: {requestId})...");
                }
                else
                {
                    CPH.LogInfo($"Chaos Mod: Sending random effect trigger (Request ID: {requestId})...");
                }

                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var task = Task.Run(async () =>
                {
                    using (var client = new ClientWebSocket())
                    {
                        var uri = new Uri($"{WEBSOCKET_URL}:{WEBSOCKET_PORT}");
                        await client.ConnectAsync(uri, cts.Token);

                        if (client.State == WebSocketState.Open)
                        {
                            // Send message
                            var bytes = Encoding.UTF8.GetBytes(jsonMessage);
                            await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cts.Token);
                            CPH.LogDebug($"Chaos Mod: Message sent (ID: {requestId}), waiting for response...");

                            // Wait for response with timeout
                            var responseBuffer = new byte[4096];
                            var responseCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(RESPONSE_TIMEOUT_MS));
                            
                            try
                            {
                                var result = await client.ReceiveAsync(new ArraySegment<byte>(responseBuffer), responseCts.Token);
                                
                                if (result.MessageType == WebSocketMessageType.Text)
                                {
                                    string responseStr = Encoding.UTF8.GetString(responseBuffer, 0, result.Count);
                                    CPH.LogDebug($"Chaos Mod: Received response: {responseStr}");

                                    // Parse response
                                    var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseStr);
                                    
                                    if (response != null && response.ContainsKey("type") && 
                                        response["type"].GetString() == "randomEffectResponse")
                                    {
                                        // Verify request ID matches (important for concurrent requests)
                                        if (response.ContainsKey("id"))
                                        {
                                            int responseId = response["id"].GetInt32();
                                            if (responseId != requestId)
                                            {
                                                CPH.LogWarn($"Chaos Mod: Response ID mismatch! Expected {requestId}, got {responseId}");
                                                return (false, $"Response ID mismatch (expected {requestId}, got {responseId})");
                                            }
                                        }

                                        bool success = response.ContainsKey("success") && response["success"].GetBoolean();
                                        
                                        if (success)
                                        {
                                            string effectID = response.ContainsKey("effectID") 
                                                ? response["effectID"].GetString() 
                                                : "unknown";
                                            CPH.LogInfo($"Chaos Mod: Success! Effect '{effectID}' triggered (Request ID: {requestId}).");
                                            return (true, "");
                                        }
                                        else
                                        {
                                            string reason = response.ContainsKey("reason") 
                                                ? response["reason"].GetString() 
                                                : "Unknown error";
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

                var (success, error) = task.Result;

                if (success)
                {
                    return true;
                }
                else
                {
                    CPH.LogWarn($"Chaos Mod: Attempt {attempt} failed (ID: {requestId}) - {error}");
                    
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

