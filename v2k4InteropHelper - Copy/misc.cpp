#pragma once
#include "misc.h"
#include "logger.h"


void createDir(std::string path) {
    if (!fs::is_directory(path) || !fs::exists(path)) {
        logger.Write(LOG_INFO, "create_directory: %s", path.c_str());
        if (!fs::create_directory(path)) {
            logger.Write(LOG_ERROR, "create_directory failed");
        }
    }
}

std::string ReplaceAll(std::string str, const std::string& from, const std::string& to) {
    size_t start_pos = 0;
    while ((start_pos = str.find(from, start_pos)) != std::string::npos) {
        str.replace(start_pos, from.length(), to);
        start_pos += to.length(); // Handles case where 'to' is a substring of 'from'
    }
    return str;
}

std::vector<std::string> splitStr(std::string str, std::string delim) {
    std::vector<std::string> result;

    std::size_t current, previous = 0;
    current = str.find_first_of(delim);
    if (current == std::string::npos) {
        result.push_back(str);
        return result;
    }
    std::string to_add = "";
    while (current != std::string::npos) {
        to_add = str.substr(previous, current - previous);
        if (!to_add.empty())
            result.push_back(to_add);
        previous = current + 1;
        current = str.find_first_of(delim, previous);
    }
    result.push_back(str.substr(previous, current - previous));
    return result;
}


namespace JsonHelper {
    void createKey(json& j, std::vector<std::string>& path, std::string defaultval) {
        if (path.size() == 0) {
            // End
            return;
        }
        std::string el = path[0];
        if (el.empty()) {
            path.erase(path.begin());
            createKey(j, path, defaultval);
        }

        if (path.size() == 1) {
            // Assign value
            j[el] = defaultval;
        }
        else {
           
            try {
                j.at(json::json_pointer("/" + el));
            }
            catch (nlohmann::json::exception) {
                // create empty object
                j[el] = json::object();
            }
        }
        path.erase(path.begin());
        createKey(j[el], path, defaultval);
    }
}