#pragma once
#include <string>
#include <vector>
//#include "imgui.h"
//#include "imgui_impl_win32.h"

enum LogLevel {
    LOG_DEBUG,
    LOG_INFO,
    LOG_WARN,
    LOG_ERROR,
    LOG_FATAL,
};

inline struct GuiCons {
    char                  InputBuf[256] = { "\0"};
    /*ImVector<char*>       Items;
    ImVector<const char*> Commands;
    ImVector<char*>       History;*/
    int                   HistoryPos = -1;    // -1: new line, 0..History.Size-1 browsing history.
    //ImGuiTextFilter       Filter;
    bool                  AutoScroll = true;
    bool                  ScrollToBottom = false;
} ImGuiConsoleLog;

class Logger {
public:
    const std::vector<std::string> levelStrings{
        "DEBUG",
        "INFO",
        "WARN",
        "ERROR",
        "FATAL",
    };

    // What2Log
    bool logGetFileInfo = false;
    bool logLoadFile = false;
    bool logIniRead = false;
    bool logGameLogs = false;
    bool logluaL_tolstring = false;

    Logger();
    void SetFile(const std::string& fileName);
    void SetHookedFile(const std::string& fileName);
    void SetMinLevel(LogLevel level);
    void Clear() const;
    void WriteHooked(LogLevel level, const std::string& text) const;
    void Write(LogLevel level, const std::string& text) const;
    void Write(LogLevel level, const char* fmt, ...) const;

private:
    
    // Portable helpers
    static int   Stricmp(const char* str1, const char* str2) { int d; while ((d = toupper(*str2) - toupper(*str1)) == 0 && *str1) { str1++; str2++; } return d; }
    static int   Strnicmp(const char* str1, const char* str2, int n) { int d = 0; while (n > 0 && (d = toupper(*str2) - toupper(*str1)) == 0 && *str1) { str1++; str2++; n--; } return d; }
    //static char* Strdup(const char* str) { size_t len = strlen(str) + 1; void* buf = malloc(len); IM_ASSERT(buf); return (char*)memcpy(buf, (const void*)str, len); }
    static void  Strtrim(char* str) { char* str_end = str + strlen(str); while (str_end > str&& str_end[-1] == ' ') str_end--; *str_end = 0; }

    std::string file = "";
    std::string hooked_file = "";
    std::string levelText(LogLevel level) const;
    LogLevel minLevel = LOG_INFO;


    const std::vector<std::string> ignoreGameWords{
        "_level",
        "LocalPlanarReflection"
    };

};

extern Logger logger;