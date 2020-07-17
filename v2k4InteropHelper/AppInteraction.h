#pragma once
#include <windows.h>

#include <string>
#include <cstring>
#include "logger.h"
#include "hook_manager.h"
#include <iostream>
#include <map>
#include <filesystem>
#include <TlHelp32.h>
#include "engine.h"
#include "StringBuilder.h"
#include "v2k4helpers.h"
#include <thread>
#include "AppInteractionTransfer.h"
#include "json\json.h"


class AppInteraction
{
private:
	inline static std::thread c;
	inline static bool StopThread = false;

	static bool CheckAmIAllowedToInteract(std::string &file)
	{
		// --------------------------------------------------------
		// FILE SYSTEM IS RELATIVE TO FIFA EXE!!!!

		bool allowed = false;
		if (std::filesystem::exists("InjOp.Open.txt"))
		{
			//logger.Write(LOG_DEBUG, "AppInteraction :: File System is Open");
			file = "InjOp.Open.txt";
			allowed = true;
		}
		else if (std::filesystem::exists("InjOp.Server.txt"))
		{
			//logger.Write(LOG_DEBUG, "AppInteraction :: File System is OWNED (Server)");
			file = "InjOp.Server.txt";
		}
		else if (std::filesystem::exists("InjOp.Client.txt"))
		{
			//logger.Write(LOG_DEBUG, "AppInteraction :: File System is Client");
			file = "InjOp.Client.txt";
		}
		else
		{
			logger.Write(LOG_DEBUG, "AppInteraction :: File System doesn't exist");
		}
		return allowed;
	}
public:
	inline static bool Working = false;
	AppInteraction() {
		c = std::thread([]() { 
			while (!StopThread) {
				Sleep(2000);

				std::string indicatorFile;
				bool allowedToInteract = CheckAmIAllowedToInteract(indicatorFile);
				if (!Working && allowedToInteract && !indicatorFile.empty())
				{
					Working = true;
					//logger.Write(LOG_DEBUG, indicatorFile);
					auto fp = indicatorFile.find("Open");
					if (fp != std::string::npos) {
						// If a data file exists read out its contents
						if (std::filesystem::exists("InjOp.Data.json"))
						{
							try {

								std::array<std::string,10> responsedata;
								responsedata.at(0) = "TEST";

								// Change Indicator file to SERVER
								if(std::filesystem::exists("InjOp.Open.txt"))
									std::filesystem::rename("InjOp.Open.txt", "InjOp.Server.txt");

								// load the file contents into json
								//std::ifstream i("InjOp.Data.json");

								Json::Value root;   // starts as "null"; will contain the root value after parsing
								std::ifstream config_doc("InjOp.Data.json", std::ifstream::binary);
								config_doc >> root;

								// Get the LUA
								// Flat LUA String to Call
								std::string LUA = root.get("LUA", "").asString();
								if (!LUA.empty()) {
									logger.Write(LOG_DEBUG, "Running Script from Interaction:: " + LUA);
									OutputDebugString("Running Script from Interaction");
									if (g_engine.isInCM()) {
										g_engine.RunFIFAScript(LUA);
									}
									else {
										logger.Write(LOG_ERROR, "Cant run Script. Not in CM /n");
										OutputDebugString("Cant run Script. Not in CM ");
									}
								}

								// Get the Transfers
								// Transfers would be an Array of Transfer objects/structs
								const Json::Value Transfers = root["Transfers"];
								if (!Transfers.isNull()) {
									for (int index = 0; index < Transfers.size(); ++index)
									{
										auto playerid = Transfers[index]["PlayerId"].asInt();
										auto newteamid = Transfers[index]["TeamIdTo"].asInt();

									}
								}

								// EditedPlayers
								// EditedPlayers would be an Array of FIFAPlayer objects/structs
								const Json::Value EditedPlayers = root["EditedPlayers"];
								if (!EditedPlayers.isNull())
								{
									for (int index = 0; index < EditedPlayers.size(); ++index)
									{

									}
								}
								// Run Other functions
								/*HMODULE mod = GetModuleHandleW(NULL);
								Engine* fptr = (Engine*)GetProcAddress(mod, "doSomething");
								fptr();*/


								// TODO: GIVE DATA BACK
								//root["RESPONSE_DATA"] = array
								root["LUA"] = "";
								for (int index = 0; index < responsedata.size(); ++index)
								{
									root["RESPONSE_DATA"] = responsedata[index];
								}
								std::ofstream out("InjOp.Data.json");
								std::streambuf* coutbuf = std::cout.rdbuf(); //save old buf
								std::cout.rdbuf(out.rdbuf()); //redirect std::cout to out.txt!

								// Make a new JSON document with the new configuration. Preserve original comments.
								std::cout << root << "\n";
								std::cout << std::endl;


								// Change Indicator file to SERVER
								if (std::filesystem::exists("InjOp.Server.txt")) {
									std::filesystem::rename("InjOp.Server.txt", "InjOp.Open.txt");
									Sleep(10000);
								}
							}
							catch (std::exception e) {
								logger.Write(LOG_ERROR, e.what());
							}
						}

						// Change Indicator file to OPEN

					}
					Working = false;

				}


			}
		});
	}

	void close() {
		StopThread = true;
		c.join();
	}

	~AppInteraction() {
		StopThread = true;

		/*if(c.joinable())
			c.join();*/

	}

	
};



