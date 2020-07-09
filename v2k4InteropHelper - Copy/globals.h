#pragma once
#include <windows.h>
#include "context.h"
#include "external/PolyHook2/PolyHook_2_0/polyhook2/CapstoneDisassembler.hpp"
#include "external/PolyHook2/PolyHook_2_0/polyhook2/Detour/x64Detour.hpp"

inline HWND g_main_window = nullptr;

inline extern int dx_max_attempts = 2;
inline int dx11_attempts = 0;
inline int dx12_attempts = 0;

inline bool g_bPresent11Initialised = false;
inline bool g_bPresent11Hooked = false;

inline bool g_bPresent12Initialised = false;

//extern "C" bool g_bShowMainMenu = false;

inline core::Context g_ctx_proc;
inline core::Context g_ctx_dll;
inline core::Context g_ctx_ucrtbase_dll;

inline PLH::CapstoneDisassembler dis(PLH::Mode::x64);
inline std::string hooked_dx_ver = "DX00";

#pragma region Tramps
inline uint64_t hookResizeBuffersD3D11Tramp = NULL;
inline uint64_t hookResizeTargetD3D11Tramp = NULL;

inline uint64_t hookResizeBuffersD3D12Tramp = NULL;
inline uint64_t hookResizeTargetD3D12Tramp = NULL;
inline uint64_t hookExecuteCommandListsD3D12Tramp = NULL;
inline uint64_t hookSignalD3D12Tramp = NULL;
inline uint64_t hookIGOWngProcTramp = NULL;
inline uint64_t hkStdioPrinfTramp = NULL;
#pragma endregion

#pragma region Detours
inline std::unique_ptr<PLH::x64Detour> g_dtr_DirectX11Present = NULL;
inline std::unique_ptr<PLH::x64Detour> g_dtr_DirectX11ResizeTarget = NULL;
inline std::unique_ptr<PLH::x64Detour> g_dtr_DirectX11ResizeBuffers = NULL;
inline std::unique_ptr<PLH::x64Detour> g_dtr_DirectX12Present = NULL;
inline std::unique_ptr<PLH::x64Detour> g_dtr_DirectX12ResizeTarget = NULL;
inline std::unique_ptr<PLH::x64Detour> g_dtr_DirectX12ResizeBuffers = NULL;
inline std::unique_ptr<PLH::x64Detour> g_dtr_ExecuteCommandListsD3D12 = NULL;
inline std::unique_ptr<PLH::x64Detour> g_dtr_SignalD3D12 = NULL;
inline std::unique_ptr<PLH::x64Detour> g_dtr_IGOWngProc = NULL;
inline std::unique_ptr<PLH::x64Detour> g_dtr_stdio_vprintf = NULL;
#pragma endregion

