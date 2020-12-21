// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <windows.h>
#include "string.h"
#include<iostream>
#include<set>
#include<string>
#include <ostream>
#include <fstream>
#include <vector>

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

static std::string base64_encode(const std::string& in) {

    std::string out;

    int val = 0, valb = -6;
    for (int jj = 0; jj < in.size(); jj++) {
        char c = in[jj];
        val = (val << 8) + c;
        valb += 8;
        while (valb >= 0) {
            out.push_back("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"[(val >> valb) & 0x3F]);
            valb -= 6;
        }
    }
    if (valb > -6) out.push_back("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"[((val << 8) >> (valb + 8)) & 0x3F]);
    while (out.size() % 4) out.push_back('=');
    return out;
}

static std::string base64_decode(const std::string& in) {

    std::string out;

    std::vector<int> T(256, -1);
    for (int i = 0; i < 64; i++) T["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"[i]] = i;

    int val = 0, valb = -8;
    for (int jj = 0; jj < in.size(); jj++) {
        char c = in[jj];
        if (T[c] == -1) break;
        val = (val << 6) + T[c];
        valb += 6;
        if (valb >= 0) {
            out.push_back(char((val >> valb) & 0xFF));
            valb -= 8;
        }
    }
    return out;
}

std::string AVAILABLE_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";

int index(char c) {
    for (int ii = 0; ii < AVAILABLE_CHARS.size(); ii++) {
        if (AVAILABLE_CHARS[ii] == c) {
            // std::cout << ii << " " << c << std::endl;
            return ii;
        }
    }
    return -1;
}


std::string extend_key(std::string& msg, std::string& key) {
    //generating new key
    int msgLen = msg.size();
    std::string newKey(msgLen, 'x');
    int keyLen = key.size(), i, j;
    for (i = 0, j = 0; i < msgLen; ++i, ++j) {
        if (j == keyLen)
            j = 0;

        newKey[i] = key[j];
    }
    newKey[i] = '\0';
    return newKey;
}


std::string encrypt_vigenere(std::string& msg, std::string& key) {
    int msgLen = msg.size(), keyLen = key.size(), i, j;
    std::string encryptedMsg(msgLen, 'x');
    // char newKey[msgLen], encryptedMsg[msgLen], decryptedMsg[msgLen];

    std::string newKey = extend_key(msg, key);

    //encryption
    for (i = 0; i < msgLen; ++i) {
        // std::cout << msg[i] << " " << isalnum(msg[i]) << std::endl;
        if (isalnum(msg[i]) or msg[i] == ' ') {
            encryptedMsg[i] = AVAILABLE_CHARS[((index(msg[i]) + index(newKey[i])) % AVAILABLE_CHARS.size())];
        }
        else {
            encryptedMsg[i] = msg[i];
        }
    }

    encryptedMsg[i] = '\0';
    return encryptedMsg;
}

std::string decrypt_vigenere(std::string& encryptedMsg, std::string& newKey) {
    // decryption
    int msgLen = encryptedMsg.size();
    std::string decryptedMsg(msgLen, 'x');
    int i;
    for (i = 0; i < msgLen; ++i) {
        if (isalnum(encryptedMsg[i]) or encryptedMsg[i] == ' ') {
            decryptedMsg[i] = AVAILABLE_CHARS[(((index(encryptedMsg[i]) - index(newKey[i])) + AVAILABLE_CHARS.size()) % AVAILABLE_CHARS.size())];
        }
        else {
            decryptedMsg[i] = encryptedMsg[i];
        }
    }
    decryptedMsg[i] = '\0';
    return decryptedMsg;
}

string encryptDecrypt2(string toEncrypt) {
    //char key[3] = { 'K' }; //Any chars will work
    //string output = toEncrypt;

    //for (int i = 0; i < toEncrypt.size(); i++)
    //    output[i] = toEncrypt[i] ^ key[i % (sizeof(key) / sizeof(char))];

    //return output;

    char key = 'x';

    string output;
    for (int temp = 0; temp < toEncrypt.size(); temp++) {
        output += toEncrypt[temp] ^ (int(key) + temp) % 255;
    }
    return output;
}
//
//string encrypt(string toEncrypt) {
//    string output = toEncrypt;
//
//    for (int i = 0; i < toEncrypt.size(); i++) {
//        switch (toEncrypt[i]) {
//            case '0':
//                output[i] = '¶';
//                break;
//            /*case '1':
//                break;
//            case '2':
//                break;
//            case '3':
//                break;
//            case '4':
//                break;
//            case '5':
//                break;
//            case '6':
//                break;
//            case '7':
//                break;
//            case '9':
//                break;*/
//            default:
//                output[i] = toEncrypt[i];
//                break;
//        }
//    }
//
//    return output;
//}
//
//string decrypt(string toEncrypt) {
//    string output = toEncrypt;
//
//    for (int i = 0; i < toEncrypt.size(); i++) {
//        switch (toEncrypt[i]) {
//        case '¶':
//            output[i] = '0';
//            break;
//            /*case '1':
//                break;
//            case '2':
//                break;
//            case '3':
//                break;
//            case '4':
//                break;
//            case '5':
//                break;
//            case '6':
//                break;
//            case '7':
//                break;
//            case '9':
//                break;*/
//        default:
//            output[i] = toEncrypt[i];
//            break;
//        }
//    }
//
//    return output;
//}

std::string encrypt(std::string& msg, std::string& key) {
    std::string b64_str = base64_encode(msg);
    std::string vigenere_msg = encrypt_vigenere(b64_str, key);
    // std::cout << vigenere_msg << std::endl;
    return vigenere_msg;
}


std::string decrypt(std::string& encrypted_msg, std::string& key) {
    std::string newKey = extend_key(encrypted_msg, key);
    std::string b64_encoded_str = decrypt_vigenere(encrypted_msg, newKey);
    std::string b64_decode_str = base64_decode(b64_encoded_str);
    return b64_decode_str;
}

bool is_file_encrypted(const char* filepath) {

    if (std::string(filepath).ends_with(".mod"))
        return true;

    //ifstream myfile(filepath, ios::in | ios::binary);
    ////myfile.ignore(99999, '\n');
    //if (myfile.is_open())
    //{
    //    char* encrypted_check_buffer = new char[10];
    //    myfile.get(encrypted_check_buffer, 10);
    //    if (std::string(encrypted_check_buffer).find("ENCRYPTED") != -1)
    //    {
    //        OutputDebugStringA("ENCRYPTED FILE FOUND");
    //        myfile.close();
    //        return true;
    //    }
    //    myfile.close();
    //}
    return false;
}

std::string read_mod_file_from_file_system(const char* filepath) {

    bool isEncrypted = false;
    if (is_file_encrypted(filepath))
    {
        isEncrypted = true;
    }

    ifstream myfile(filepath, ios::in | ios::binary);
    if (myfile.is_open())
    {
        myfile.seekg(0, myfile.end);
        int length = myfile.tellg();
        myfile.seekg(0, myfile.beg);

        std::string ent;
        char my_character;
        if (myfile) {
            while (!myfile.eof()) {
                myfile.get(my_character);
                if (!myfile)
                    break;
                ent += my_character;
            }

        }
        else {
            std::cout << "file does not exist!\n";
        }

        if (myfile)
            OutputDebugStringA("All characters read successfully. \n");
        else {
            OutputDebugStringA("Could not read file. \n");
        }

        myfile.close();

        if (isEncrypted) {
            std::string magic = "";
            std::string key = "FIFERISACUNT";
            magic = decrypt(ent, key);
            ent = magic;
        }

        return ent;
       /* char* c = const_cast<char*>(ent.c_str());
        return c;*/
    }

    return NULL;
}

void encrypt_decrypt_file(const char* filepath, const char* to_path, bool to_decrypt) {

    /*if (!is_file_encrypted(filepath) && to_decrypt)
        return;*/

    string line;
    string entirefile;
    ifstream myfile(filepath, ios::in | ios::binary);
    //myfile.ignore(99999, '\n');
    if (myfile.is_open())
    {
        myfile.seekg(0, myfile.end);
        int length = myfile.tellg();
        myfile.seekg(0, myfile.beg);

        char* buffer = new char[length];

        std::cout << "Reading " << length << " characters... ";
        std::string ent;

        char my_character;
        if (myfile) {
            while (!myfile.eof()) {
                myfile.get(my_character);
                if (!myfile)
                    break;
                ent += my_character;
            }

        }
        else {
            std::cout << "file does not exist!\n";
        }

        if (myfile)
            OutputDebugStringA("all characters read successfully. \n");
        else {
        }

        myfile.close();

        std::string magic = "";
        std::string key = "FIFERISACUNT";
        if (to_decrypt)
            magic = decrypt(ent, key);
        else
            magic = encrypt(ent, key);

        ofstream ofile(to_path);
        if (ofile.is_open())
        {
            ofile << magic;
            ofile.close();
        }
        else cout << "Unable to open file";

    }
    else cout << "Unable to open file";


    //auto rTest = std::string(read_mod_file_from_file_system(filepath));
    //if (rTest.length() > 0) {
    //    // https://www.techiedelight.com/convert-std-string-char-cpp/ There are many other options for this
    //    char* c = const_cast<char*>(rTest.c_str());
    //    if (c) {
    //        OutputDebugStringA("");
    //    }
    //}

}


extern "C" {

    __declspec(dllexport) void encryptFile(const char* filepath, const char* to_path) 
    {
        encrypt_decrypt_file(filepath, to_path, false);
    }

    __declspec(dllexport) void decryptFile(const char* filepath, const char* to_path)
    {
        encrypt_decrypt_file(filepath, to_path, true);
    }

    __declspec(dllexport) void test_load_mod_file(const char* filepath)
    {
        std::string result = std::string(read_mod_file_from_file_system(filepath));
        if (result.length() > 0) {
            //    // https://www.techiedelight.com/convert-std-string-char-cpp/ There are many other options for this
           char* c = const_cast<char*>(result.c_str());
           if (c) {
               OutputDebugStringA("");
           }
        }
    }
        
}

