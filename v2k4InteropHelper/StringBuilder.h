#pragma once
#include <string>
class StringBuilder {
private:
    std::string main;
    std::string scratch;

    const std::string::size_type ScratchSize = 2048;  // or some other arbitrary number

public:
    StringBuilder& append(const std::string& str) {
        scratch.append(str);
        if (scratch.size() > ScratchSize) {
            main.append(scratch);
            scratch.resize(0);
        }
        return *this;
    }

    const std::string& str() {
        if (scratch.size() > 0) {
            main.append(scratch);
            scratch.resize(0);
        }
        return main;
    }
};

