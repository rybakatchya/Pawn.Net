#define PLUGIN_API __declspec(dllexport)

#ifdef __cplusplus
#define EXTERN_C_BEGIN extern "C" {
#define EXTERN_C_END }
#else
#define EXTERN_C_BEGIN
#define EXTERN_C_END
#endif

#include "amx.h"
#include "amx_loader.h"
#include <stdio.h>

EXTERN_C_BEGIN

amx_loader* _loader;
amx* _amx;

inline int test_native(struct amx_loader* loader, struct amx* amx, void* userdata)
{
	printf("hello from native plugin!\n");
	return 1;
}

PLUGIN_API inline void plugin_init(struct amx_loader* loader, struct amx* amx)
{
	_loader = loader;
	_amx = amx;
	amx_loader_register_native(_loader, "testNative", test_native);
	printf("native plugin loaded\n");
};

EXTERN_C_END