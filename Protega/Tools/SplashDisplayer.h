#pragma once
#include "windows.h"
class SplashDisplayer
{
public:
	SplashDisplayer(LPCTSTR lpszFileName, COLORREF colTrans);
	~SplashDisplayer();
	void ShowSplash();
	int CloseSplash();
	DWORD SetBitmap(LPCTSTR lpszFileName);
	DWORD SetBitmap(HBITMAP hBitmap);
	bool SetTransparentColor(COLORREF col);
	LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
	HWND m_hwnd;
private:
	void  OnPaint(HWND hwnd);
	bool MakeTransparent();
	HWND RegAndCreateWindow();
	COLORREF m_colTrans;
	DWORD m_dwWidth;
	DWORD m_dwHeight;
	void FreeResources();
	HBITMAP m_hBitmap;
	LPCTSTR m_lpszClassName;
};

