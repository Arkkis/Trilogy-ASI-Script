#include "EffectDatabase.h"

#include "util/EffectBase.h"

#include <iterator>
#include <random>

void
EffectDatabase::RegisterEffect (EffectBase *base)
{
    auto &id         = base->GetMetadata ().id;
    auto &effectsMap = GetInstance ().effectsMap;
    if (effectsMap.contains (id))
    {
#ifdef _DEBUG
        MessageBox (NULL, id.c_str (), "Trying to register duplicate effect",
                    MB_ICONHAND);
#endif
        return;
    }
    GetInstance ().effectsMap[id] = base;
}

EffectBase *
EffectDatabase::FindEffectById (std::string id)
{
    auto &effectsMap = GetInstance ().effectsMap;
    return effectsMap.contains (id) ? effectsMap[id] : nullptr;
}

EffectBase *
EffectDatabase::GetRandomEffect ()
{
    auto &effectsMap = GetInstance ().effectsMap;
    if (effectsMap.empty ()) return nullptr;

    // Get a random effect from the map
    static std::random_device rd;
    static std::mt19937       gen (rd ());
    std::uniform_int_distribution<> dis (0, effectsMap.size () - 1);

    auto it = effectsMap.begin ();
    std::advance (it, dis (gen));
    return it->second;
}