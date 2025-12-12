#include "GenericUtil.h"

#include "util/Config.h"
#include "util/Globals.h"
#include "util/Version.h"
#include "util/Websocket.h"

#include <algorithm>
#include <cctype>
#include <string_view>

std::string
GenericUtil::GetModVersion ()
{
    std::string version = "Chaos Mod v3.3.0";

#ifndef NDEBUG
    version.append ("-debug");
#endif

#ifdef VERSION_SUFFIX
    version.append ("-git.").append (VERSION_SUFFIX);
#endif

    if (CONFIG_CC_ENABLED)
        version.append ("~n~CC Connected: ");
    else
        version.append ("~n~GUI Connected: ");

    bool connected = Websocket::IsClientConnected ();
    version.append (connected ? "~g~true" : "~r~false");

    return version;
}

double
GenericUtil::CalculateTick (double multiplier)
{
    unsigned diff = CTimer::m_snTimeInMilliseconds
                    - CTimer::m_snPreviousTimeInMilliseconds;

    // If the jump is too big, e.g. replays or loading saves
    if (diff <= 0 || diff >= 1000) return 0;

    float timeScale = std::max (0.000001f, CTimer::ms_fTimeScale);

    return diff / timeScale * multiplier;
}

std::string
GenericUtil::FormatTime (int duration, bool onlySeconds)
{
    int seconds = std::max (0, duration) / 1000, minutes = 0;
    while (seconds >= 60)
    {
        minutes += 1;
        seconds -= 60;
    }

    if (minutes > 99)
    {
        minutes = 99;
        seconds = 59;
    }

    std::string time;
    if (!onlySeconds)
    {
        if (minutes < 10) time.append ("0");

        time.append (std::to_string (minutes));
        time.append (":");
    }
    if (seconds < 10) time.append ("0");

    time.append (std::to_string (seconds));
    return time;
}

bool
GenericUtil::IsMenuActive ()
{
    return FrontEndMenuManager.m_bMenuActive;
}

float
GenericUtil::EaseOutBack (float t)
{
    t -= 1;
    return 1 + t * t * (2.70158f * t + 1.70158f);
}

float
GenericUtil::EaseInOutQubic (float t)
{
    if (t < 0.5)
        return 4 * t * t * t;
    else
        return 1 - powf (-2 * t + 2, 3) / 2;
}

std::string
GenericUtil::ToUpper (std::string string)
{
    std::transform (string.begin (), string.end (), string.begin (),
                    [] (unsigned char c) { return std::toupper (c); });
    return string;
}

std::string
GenericUtil::FormatEffectName (std::string_view effectName)
{
    std::string name(effectName);

    // Remove "effect_" prefix if present (for IDs)
    if (name.length () >= 7 && name.substr (0, 7) == "effect_")
    {
        name = name.substr (7);
    }
    // Remove "effect " prefix if present (for display names with space)
    else if (name.length () >= 7 && name.substr (0, 7) == "effect ")
    {
        name = name.substr (7);
    }

    // Replace underscores with spaces
    std::replace (name.begin (), name.end (), '_', ' ');

    // Trim leading/trailing whitespace
    size_t start = name.find_first_not_of (" \t\n\r");
    if (start != std::string::npos)
    {
        name.erase (0, start);
        size_t end = name.find_last_not_of (" \t\n\r");
        if (end != std::string::npos)
        {
            name.erase (end + 1);
        }
    }
    else
    {
        name.clear ();
    }

    // Capitalize first letter of each word
    bool capitalizeNext = true;
    for (char &c : name)
    {
        if (capitalizeNext && std::islower (c))
        {
            c = std::toupper (c);
            capitalizeNext = false;
        }
        else if (c == ' ')
        {
            capitalizeNext = true;
        }
        else
        {
            capitalizeNext = false;
        }
    }

    return name;
}
