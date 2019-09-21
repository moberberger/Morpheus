#include <intrin.h>

__declspec(dllexport)
unsigned long long RDTSC_Wrapper()
{
    return __rdtsc();
}
