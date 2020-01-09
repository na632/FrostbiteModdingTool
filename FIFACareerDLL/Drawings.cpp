#include "pch.h"
#include "Drawings.h"


void DrawString(char* String, int x, int y, int a, int r, int g, int b, ID3DXFont* font)
{
	RECT FontPos;
	FontPos.left = x;
	FontPos.top = y;
	font->DrawTextA(0, String, strlen(String), &FontPos, DT_NOCLIP, D3DCOLOR_ARGB(a, r, g, b));
}

void DrawFilledRectangle(float x, float y, float w, float h, int a, int r, int g, int b)
{
	D3DCOLOR color = D3DCOLOR_ARGB(a, r, g, b);
	D3DRECT rect = { x, y, w, h };
    //p_Device->Clear(1, &rect, D3DCLEAR_TARGET | D3DCLEAR_TARGET, color, 0, 0);
} 


void DrawBorderBox(int x, int y, int w, int h, int thickness, int a, int r, int g, int b)
{
	DrawFilledRectangle(x, y, w, y + thickness, a, r, g, b); // x
	DrawFilledRectangle(x, y, x + thickness, h, a, r, g, b); // y
	DrawFilledRectangle(x, h, w, h + thickness, a, r, g, b); // w
	DrawFilledRectangle(w, y, w + thickness, h + thickness, a, r, g, b); // h
}