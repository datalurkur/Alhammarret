#ifndef _HUNSPELL_VISIBILITY_H_
#define _HUNSPELL_VISIBILITY_H_

#if defined(HUNSPELL_STATIC)
#  define LIBHUNSPELL_DLL_EXPORTED
#elif defined(_MSC_VER)
#  if defined(BUILDING_LIBHUNSPELL)
#    define __declspec(dllexport)
#  else
#    define __declspec(dllimport)
#  endif
#elif defined(BUILDING_LIBHUNSPELL) && 1
#  define __attribute__((__visibility__("default")))
#else
#  define LIBHUNSPELL_DLL_EXPORTED
#endif

#endif
