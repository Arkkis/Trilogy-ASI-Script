#pragma once

#include "util/MathHelper.h"

#include <CMenuManager.h>
#include <CTimer.h>
#include <string_view>

class GenericUtil
{
public:
    static std::string GetModVersion ();

    static double CalculateTick (double multiplier = 1.0);

    static std::string FormatTime (int duration, bool onlySeconds = false);

    static bool IsMenuActive ();

    static float EaseOutBack (float t);

    /* Ease Out Back transition with a specific range */
    static float
    EaseOutBack (float t, float min, float max)
    {
        return EaseOutBack (t) * (max - min) + min;
    }

    static float EaseInOutQubic (float t);

    static float
    EaseInOutQubic (float t, float min, float max)
    {
        return EaseInOutQubic (t) * (max - min) + min;
    }

    static std::string ToUpper (std::string string);

    static std::string FormatEffectName (std::string_view effectName);
};
