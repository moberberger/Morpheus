#include <intrin.h>

extern "C" __declspec(dllexport)
unsigned long long __stdcall RDTSC_Wrapper()
{
    return __rdtsc();
}

extern "C" __declspec(dllexport)
unsigned int __stdcall RDRAND32_Wrapper()
{
    unsigned int val;
    while (_rdrand32_step( &val ) == 0);
    return val;
}

extern "C" __declspec(dllexport)
unsigned long long __stdcall RDRAND64_Wrapper()
{
    unsigned long long val;
    while (_rdrand64_step( &val ) == 0);
    return val;
}

extern "C" __declspec(dllexport)
unsigned int __stdcall RDSEED32_Wrapper()
{
    unsigned int val;
    while (_rdseed32_step( &val ) == 0);
    return val;
}

extern "C" __declspec(dllexport)
unsigned long long __stdcall RDSEED64_Wrapper()
{
    unsigned long long val;
    while (_rdseed64_step( &val ) == 0);
    return val;
}
