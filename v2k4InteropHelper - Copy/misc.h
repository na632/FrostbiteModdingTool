#pragma once
#include <filesystem>
#include <string>
#include <vector>
#include "external/nlohmann/json.hpp"

namespace fs = std::filesystem;
using json = nlohmann::json;

void createDir(std::string path);
std::string ReplaceAll(std::string str, const std::string& from, const std::string& to);
std::vector<std::string> splitStr(std::string str, std::string delim);

namespace JsonHelper {
    void createKey(json& j, std::vector<std::string>& path, std::string defaultval);
}

