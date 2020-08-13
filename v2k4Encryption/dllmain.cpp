// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <windows.h>
#include "string.h"
#include<iostream>
#include<set>
#include<string>
#include <ostream>
#include <fstream>

using namespace std;

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

string encryptDecrypt2(string toEncrypt) {
    char key[3] = { 'K', 'C', 'Q' }; //Any chars will work
    string output = toEncrypt;

    for (int i = 0; i < toEncrypt.size(); i++)
        output[i] = toEncrypt[i] ^ key[i % (sizeof(key) / sizeof(char))];

    return output;
}

void encrypt_decrypt_file(const char* filepath, const char* to_path) {

    string line;
    string entirefile;
    ifstream myfile(filepath);
    if (myfile.is_open())
    {
        myfile.seekg(0, myfile.end);
        int length = myfile.tellg();
        myfile.seekg(0, myfile.beg);

        char* buffer = new char[length];

        std::cout << "Reading " << length << " characters... ";
        // read data as a block:
        myfile.read(buffer, length);

        
        if (myfile)
            OutputDebugStringA("all characters read successfully.");
        else {
            std::string err = "error: only read ";
            err = err.append(std::to_string(myfile.gcount()));
            OutputDebugStringA(err.c_str());

        }

        //while (getline(myfile, line))
        //{
        //    OutputDebugStringA(line.c_str());
        //    //cout << line << '\n';
        //    entirefile = entirefile.append(line);
        //    OutputDebugStringA(entirefile.c_str());
        //}
        myfile.close();

        auto magic = encryptDecrypt2(std::string(buffer));
        ofstream ofile(to_path);
        if (ofile.is_open())
        {
            ofile << magic;
            ofile.close();
        }
        else cout << "Unable to open file";

    }
    else cout << "Unable to open file";
}


extern "C" {

    __declspec(dllexport) void encryptFile(const char* filepath, const char* to_path) 
    {
        encrypt_decrypt_file(filepath, to_path);
    }
        
}

