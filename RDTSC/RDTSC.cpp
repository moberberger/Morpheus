#include <intrin.h>

__declspec(dllexport)
unsigned long long RDTSC_Wrapper()
{
    return __rdtsc();
}

__declspec(dllexport)
unsigned int RDRAND32_Wrapper()
{
    unsigned int val;
    while (_rdrand32_step( &val ) == 0);
    return val;
}

__declspec(dllexport)
unsigned long long RDRAND64_Wrapper()
{
    unsigned long long val;
    while (_rdrand64_step( &val ) == 0);
    return val;
}

__declspec(dllexport)
unsigned int RDSEED32_Wrapper()
{
    unsigned int val;
    while (_rdseed32_step( &val ) == 0);
    return val;
}

__declspec(dllexport)
unsigned long long RDSEED64_Wrapper()
{
    unsigned long long val;
    while (_rdseed64_step( &val ) == 0);
    return val;
}
