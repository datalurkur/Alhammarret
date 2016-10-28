#include "HunspellLib.h"

using namespace HunspellLib;
using namespace System::Runtime::InteropServices;

HunspellWrapper::HunspellWrapper(String^ dictionaryPath, String^ affixPath)
{
	/*
	char dBuff[512];
	dBuff[0] = '\\';
	dBuff[1] = '\\';
	dBuff[2] = '?';
	dBuff[3] = '\\';

	char aBuff[512];
	aBuff[0] = '\\';
	aBuff[1] = '\\';
	aBuff[2] = '?';
	aBuff[3] = '\\';
	*/

	System::IntPtr dPtr = Marshal::StringToHGlobalAnsi(dictionaryPath);
	char* dPathPtr = (char*)(void*)dPtr;
	System::IntPtr aPtr = Marshal::StringToHGlobalAnsi(affixPath);
	char* apathPtr = (char*)(void*)aPtr;

	//handle = Hunspell_create(apathPtr, dPathPtr);

	Marshal::FreeHGlobal(dPtr);
	Marshal::FreeHGlobal(aPtr);
}


HunspellWrapper::~HunspellWrapper()
{
	//Hunspell_destroy(handle);
}


bool HunspellWrapper::IsWordValid(String^ word)
{
	System::IntPtr ptr = Marshal::StringToHGlobalAnsi(word);
	char* cPtr = (char*)(void*)ptr;

	//bool ret = Hunspell_spell(handle, cPtr) != 0;

	Marshal::FreeHGlobal(ptr);

	return false;//ret;
}

List<String^>^ HunspellWrapper::GetSuggestions(String^ word)
{
	char** results = 0;

	System::IntPtr ptr = Marshal::StringToHGlobalAnsi(word);
	char* cPtr = (char*)(void*)ptr;

	//int suggestionCount = Hunspell_suggest(handle, &results, cPtr);
	int suggestionCount = 0;

	Marshal::FreeHGlobal(ptr);

	List<String^>^ ret = gcnew List<String^>();
	for (int i = 0; i < suggestionCount; ++i)
	{
		wchar_t wBuff[128];
		char* result = results[i];
		ret->Add(gcnew String(result));
	}
	return ret;
}