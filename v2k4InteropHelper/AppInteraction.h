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
#include "FileWatcher.h"


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
	inline static std::string ClientToServerFile = "InjOp.ClientToServerData.json";
	inline static std::filesystem::file_time_type TimeLastEdited;
	inline static std::string ServerToClientFile = "InjOp.ServerToClientData.json";

	inline static void DoWork() {
		Json::Value root;   // starts as "null"; will contain the root value after parsing
		std::ifstream config_doc(ClientToServerFile, std::ifstream::binary);
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

	}

	AppInteraction() {
		logger.Write(LOG_DEBUG, "Start AppInteraction");

		c = std::thread([]() { 
			while (!StopThread) {
				Sleep(2000);

					Working = true;
					
					// If a data file exists read out its contents
					if (std::filesystem::exists(ClientToServerFile))
					{
						TimeLastEdited = std::filesystem::last_write_time(ClientToServerFile);
							try {

								Json::Value rootSTC;

								std::vector<std::string> rd;
								/*rd.push_back("TEST");
								rd.push_back("TEST");
								rd.push_back("TEST");*/
								

								Json::Value root;   // starts as "null"; will contain the root value after parsing
								std::ifstream config_doc(ClientToServerFile, std::ifstream::binary);
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
									if (g_engine.isInCM())
									{
										for (int index = 0; index < Transfers.size(); ++index)
										{
											auto playerid = Transfers[index]["PlayerId"].asInt();
											auto newteamid = Transfers[index]["TeamIdTo"].asInt();

											g_engine.LoadDB();
											g_engine.ReloadDB();
											std::string shortname = g_engine.dbMgr.tables_ordered.at("teamplayerlinks");
											auto t = reinterpret_cast<SDKHelper_FIFADBTable*>(g_engine.dbMgr.tables.at(shortname));
											auto row = t->GetSingleRowByField("playerid", "190871");
											if (row) {
												auto current_team_id = row->row.at("teamid")->value;
												g_engine.EditDBTableField("teamplayerlinks", "teamid", row->row.at("teamid")->addr, row->row.at("teamid")->offset, "11");
												logger.Write(LOG_DEBUG, "Transfer " + std::to_string(playerid) + " to " + std::to_string(newteamid));
												rd.push_back("Transfer " + std::to_string(playerid) + " to " + std::to_string(newteamid) + " complete");
											}
										}
										// NULL the Transfers
										root["Transfers"] = NULL;

									}
									else {
										rd.push_back("Transfers FAILED - Not in CM");

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
								Json::Value responseData;

								root["LUA"] = "";
								for (int index = 0; index < rd.size(); ++index)
								{
									responseData[index] = rd[index];
								}
								rootSTC = responseData;


								// -------------------------------------------------------------------------------
								// Write out the Server To Client File changes
								std::ofstream out(ServerToClientFile);
								std::streambuf* coutbuf = std::cout.rdbuf(); //save old buf
								std::cout.rdbuf(out.rdbuf());
								std::cout << rootSTC << "\n";
								std::cout << std::endl;


								// -------------------------------------------------------------------------------
								// Write out the Client To Server File changes
								std::ofstream outClientToServerFile(ClientToServerFile);
								coutbuf = std::cout.rdbuf();
								std::cout.rdbuf(outClientToServerFile.rdbuf());

								std::cout << root << "\n";
								std::cout << std::endl;

							}
							catch (std::exception e) {
								logger.Write(LOG_ERROR, e.what());
							}
						}

						// Change Indicator file to OPEN

					}
					Working = false;

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



