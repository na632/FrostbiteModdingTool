#pragma once
#include <string>

namespace AppInteractionNS {

    struct Transfer {
        int PlayerId;
        int TeamIdTo;
    };

    struct FileConstruct {
        std::string LUA;
        std::vector<Transfer> Transfers;
    };

    
}
