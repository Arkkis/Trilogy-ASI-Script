# Streamer.bot Action Examples

This document provides step-by-step examples for setting up Chaos Mod triggers in Streamer.bot.

## Example 1: Chat Command `!chaos`

### Setup Steps:

1. **Create Action**
   - Open Streamer.bot
   - Go to **Actions** → **Add Action**
   - Name: `Chaos Mod - Random Effect`

2. **Add Chat Command Sub-Action**
   - Click **Add Sub-Action**
   - Select **Chat Command**
   - Command: `!chaos`
   - Cooldown: `10` seconds (optional, prevents spam)

3. **Add C# Code Sub-Action**
   - Click **Add Sub-Action**
   - Select **C# Code**
   - Mode: **Inline**
   - Copy code from `ChaosModRandomEffect.cs`

4. **Save**
   - Click **Save**

### Result:
Viewers can type `!chaos` in chat to trigger a random effect.

---

## Example 2: Channel Point Redemption

### Setup Steps:

1. **Create Action**
   - Name: `Chaos Mod - Channel Points`

2. **Add Channel Point Redemption Sub-Action**
   - Click **Add Sub-Action**
   - Select **Channel Point Redemption**
   - Redemption Name: `Trigger Random Effect`
   - Cost: `100` (or your preferred amount)
   - Cooldown: `30` seconds (optional)

3. **Add C# Code Sub-Action**
   - Same as Example 1

4. **Save**

### Result:
Viewers can redeem channel points to trigger random effects.

---

## Example 3: Twitch Cheer/Bits

### Setup Steps:

1. **Create Action**
   - Name: `Chaos Mod - Cheer Trigger`

2. **Add Bits Sub-Action**
   - Click **Add Sub-Action**
   - Select **Bits**
   - Minimum Bits: `100` (or your preferred amount)
   - Cooldown: `60` seconds (optional)

3. **Add C# Code Sub-Action**
   - Same as Example 1

4. **Save**

### Result:
Viewers can cheer bits to trigger random effects.

---

## Example 4: Custom Duration via Command Argument

### Modified Code:

Replace the `Execute()` method with:

```csharp
public bool Execute()
{
    // Try to get duration from command arguments first
    string durationArg = CPH.GetGlobalVar<string>("ChaosModDurationArg", true);
    int duration = 30000; // Default 30 seconds
    
    if (!string.IsNullOrEmpty(durationArg) && int.TryParse(durationArg, out int parsedDuration))
    {
        duration = parsedDuration * 1000; // Convert seconds to milliseconds
    }
    else
    {
        // Fall back to global variable
        duration = CPH.GetGlobalVar<int>("ChaosModDuration", true);
        if (duration == 0) duration = 30000;
    }
    
    return TriggerRandomEffect(duration);
}
```

### Setup:

1. Create chat command: `!chaos 60` (for 60 seconds)
2. Add sub-action to parse argument and set `ChaosModDurationArg` global variable
3. Add C# Code sub-action

---

## Example 5: Multiple Commands with Different Durations

### Setup:

1. **Short Effect** (`!chaosshort`)
   - Duration: 15 seconds (15000 ms)
   - Modify code: `return TriggerRandomEffect(15000);`

2. **Normal Effect** (`!chaos`)
   - Duration: 30 seconds (default)
   - Use standard code

3. **Long Effect** (`!chaoslong`)
   - Duration: 60 seconds (60000 ms)
   - Modify code: `return TriggerRandomEffect(60000);`

---

## Example 6: Rate Limited Command

### Setup:

1. Create action with chat command
2. Add **Cooldown** sub-action:
   - Type: **User Cooldown**
   - Duration: `300` seconds (5 minutes per user)
3. Add C# Code sub-action

### Result:
Each viewer can only trigger an effect once every 5 minutes.

---

## Tips

- **Cooldowns**: Use Streamer.bot's built-in cooldown system to prevent spam
- **Permissions**: Restrict commands to mods/vips if desired
- **Logging**: Check Streamer.bot logs if effects aren't triggering
- **Testing**: Test with game running before going live
- **Duration**: Longer durations = more impactful effects

