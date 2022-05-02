#include <intrin.h>

__declspec(dllexport)
unsigned long long RDTSC_Wrapper()
{
    return __rdtsc();
}

__declspec(dllexport)
unsigned int RDRAND_Wrapper()
{
    unsigned int val;
    while (_rdrand32_step(&val) == 0);
    return val;
}

__declspec(dllexport)
unsigned long long RDSEED_Wrapper()
{
    unsigned int val;
    while (_rdseed32_step(&val) == 0);
    return val;
}
