#pragma once

#include "util/Config.h"
#include "util/DrawHelper.h"
#include "util/DrawVoting.h"
#include "util/RandomHelper.h"

#include <iostream>
#include <thread>

#include <ixwebsocket/IXNetSystem.h>
#include <ixwebsocket/IXUserAgent.h>
#include <ixwebsocket/IXWebSocket.h>
#include <ixwebsocket/IXWebSocketServer.h>

#define GUI_WEBSOCKET_PORT                                                     \
    std::max (1025, std::min (65535, CONFIG ("Chaos.GUIWebsocketPort", 42069)))
#define DIRECT_WEBSOCKET_PORT                                                  \
    std::max (1025, std::min (65535, CONFIG ("Chaos.DirectWebsocketPort", 42071)))

class Websocket
{
    static inline std::string CC_WEBSOCKET_URL = "ws://localhost:42070";

    // Websocket Client (connects to GUI)
    static inline ix::WebSocket wsClient;

    // Websocket Server (for direct connections)
    static inline ix::WebSocketServer *wsServer = nullptr;

    static std::string GetWebsocketURL ();

public:
    static void Setup ();
    static void SetupServer ();

    static std::string GetGUIWebsocketURL ();

    static bool IsClientConnected ();
    static bool IsClientConnectingOrConnected ();
    static void ReconnectClient ();

    static void CallFunction (std::string text);

    static void SendWebsocketMessage (nlohmann::json data);
    static void SendCrowdControlResponse (int effectID, int status);
};
