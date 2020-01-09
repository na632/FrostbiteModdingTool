#ifndef DRAWINGS_H
#define DRAWINGS_H


#include "DirectX.h"

#pragma warning( disable : 4244 )
#pragma warning( disable : 4996 )


#include <d3d9.h>
#include <d3dx9.h>
#pragma comment(lib, "d3d9.lib")
#pragma comment(lib, "d3dx9.lib")


void DrawString(char* String, int x, int y, int a, int r, int g, int b, ID3DXFont* font);
void DrawFilledRectangle(float x, float y, float w, float h, int a, int r, int g, int b);
void DrawBorderBox(int x, int y, int w, int h, int thickness, int a, int r, int g, int b);

#endif