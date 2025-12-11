# Streamer.bot Integration for Trilogy Chaos Mod

This folder contains example code for integrating Trilogy Chaos Mod with Streamer.bot to trigger random effects via WebSocket.

## Features

- ✅ **Two-way communication** - Confirms effect was triggered
- ✅ **Automatic retry** - Retries up to 3 times if no response
- ✅ **Request ID correlation** - Handles concurrent requests correctly
- ✅ **Configurable duration** - Set via Streamer.bot global variable
- ✅ **Error handling** - Comprehensive error logging

## Setup Instructions

### 1. Prerequisites

- Streamer.bot installed and running
- Trilogy Chaos Mod loaded in your game
- Game must be running for the connection to work

### 2. Configure the Mod

Make sure the mod's config file has the direct WebSocket port set (default is 42071):

```toml
[Chaos]
DirectWebsocketPort = 42071
```

### 3. Add the Action to Streamer.bot

1. Open Streamer.bot
2. Go to **Actions** → **Add Action**
3. Choose **C# Code** sub-action
4. Select **Inline** mode
5. Copy the contents of `ChaosModRandomEffect.cs` into the code editor
6. Save the action

### 4. Assign to Events

You can assign this action to:
- **Chat Commands** (e.g., `!chaos`, `!randomeffect`)
- **Channel Point Redemptions**
- **Twitch Cheer/Bits Events**
- **Subscriber Events**
- **Any other Streamer.bot trigger**

### 5. (Optional) Configure Duration

To set a custom duration for effects:

1. In Streamer.bot, go to **Settings** → **Global Variables**
2. Add a new variable:
   - **Name**: `ChaosModDuration`
   - **Type**: `Integer`
   - **Value**: Duration in milliseconds (e.g., `60000` for 60 seconds)
3. If not set, defaults to 30 seconds (30000 ms)

## Usage Examples

### Basic Chat Command

1. Create a new **Chat Command** action
2. Set command: `!chaos` (or any command you want)
3. Add **C# Code** sub-action
4. Use the code from `ChaosModRandomEffect.cs`

### Channel Point Redemption

1. Create a new **Channel Point Redemption** action
2. Set the redemption name (e.g., "Trigger Random Effect")
3. Add **C# Code** sub-action
4. Use the code from `ChaosModRandomEffect.cs`

### With Custom Duration

1. Set global variable `ChaosModDuration` to desired milliseconds
2. The action will automatically use this duration

## Message Format

### Request (Client → Mod)

```json
{
  "type": "randomEffect",
  "id": 123,
  "duration": 30000
}
```

### Response (Mod → Client)

**Success:**
```json
{
  "type": "randomEffectResponse",
  "id": 123,
  "success": true,
  "effectID": "effect_weather",
  "duration": 30000
}
```

**Failure:**
```json
{
  "type": "randomEffectResponse",
  "id": 123,
  "success": false,
  "reason": "No effects available"
}
```

## Troubleshooting

### "Connection timeout" Error

- **Check**: Is the game running?
- **Check**: Is the mod loaded? (Check game console/logs)
- **Check**: Is the port correct? (Default: 42071)

### "Response timeout - no answer from mod" Error

- The mod received the message but didn't respond
- Check mod logs for errors
- Try restarting the game

### "Response ID mismatch" Warning

- Multiple requests sent simultaneously
- This is handled automatically, but indicates high concurrency
- Consider rate limiting in Streamer.bot

### Effects Not Triggering

1. Verify game is running and mod is loaded
2. Check Streamer.bot logs for error messages
3. Verify WebSocket port matches mod config
4. Try restarting both the game and Streamer.bot

## Advanced Configuration

### Change WebSocket Port

If you changed the port in the mod's config, update the constant in the C# code:

```csharp
private const int WEBSOCKET_PORT = 42071; // Change this to match your config
```

### Adjust Retry Settings

Modify these constants in the code:

```csharp
private const int RESPONSE_TIMEOUT_MS = 2000; // Time to wait for response
private const int MAX_RETRIES = 3; // Number of retry attempts
```

## Technical Details

- **Protocol**: WebSocket (ws://)
- **Port**: 42071 (default, configurable)
- **Host**: localhost (127.0.0.1)
- **Message Format**: JSON
- **Response Timeout**: 2 seconds
- **Max Retries**: 3 attempts

## Support

For issues with:
- **The mod**: Check the mod's repository/issues
- **Streamer.bot**: Check Streamer.bot documentation
- **This integration**: Verify port configuration and game/mod status

