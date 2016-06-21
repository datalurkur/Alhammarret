#include "pch.h"
#include "HunspellWrapper.h"

using namespace Hunspell_UWP;
using namespace Platform;

HunspellSuggestions::HunspellSuggestions(int count)
{
	suggestions.resize(count);
}

Platform::String^ HunspellSuggestions::Get(int index)
{
	if (index >= (int)suggestions.size() || index < 0) { return nullptr; }
	return suggestions[index];
}

int HunspellSuggestions::Count()
{
	return (int)suggestions.size();
}

void HunspellSuggestions::SetSuggestion(int index, Platform::String^ str)
{
	suggestions[index] = str;
}

HunspellWrapper::HunspellWrapper(Platform::String^ dictionaryPath, Platform::String^ affixPath)
{
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

	if (StringToChar(dictionaryPath, &(dBuff[4]), 508) && StringToChar(affixPath, &(aBuff[4]), 508))
	{
		handle = Hunspell_create(aBuff, dBuff);
	}
}


HunspellWrapper::~HunspellWrapper()
{
	Hunspell_destroy(handle);
}


bool HunspellWrapper::IsWordValid(Platform::String^ word)
{
	char dBuff[128];
	StringToChar(word, dBuff, 128);
	return Hunspell_spell(handle, dBuff) != 0;
}

HunspellSuggestions^ HunspellWrapper::GetSuggestions(Platform::String^ word)
{
	char** results = 0;
	char dBuff[128];
	StringToChar(word, dBuff, 128);
	int suggestionCount = Hunspell_suggest(handle, &results, dBuff);
	HunspellSuggestions^ ret = ref new HunspellSuggestions(suggestionCount);
	for (int i = 0; i < suggestionCount; ++i)
	{
		wchar_t wBuff[128];
		char* result = results[i];
		int len = MultiByteToWideChar(CP_UTF8, 0, result, -1, NULL, 0);
		if (len >= 128) { continue; }
		MultiByteToWideChar(CP_UTF8, 0, result, -1, wBuff, len);
		wBuff[len] = 0;
		ret->SetSuggestion(i, ref new Platform::String(wBuff));
	}
	return ret;
}

bool HunspellWrapper::StringToChar(Platform::String^ word, char* buffer, int bufferSize)
{
	int len = WideCharToMultiByte(CP_UTF8, 0, word->Data(), -1, NULL, 0, NULL, NULL);
	if (len >= bufferSize) { return false; }
	WideCharToMultiByte(CP_UTF8, 0, word->Data(), -1, buffer, len, NULL, NULL);
	buffer[len] = 0;
	return true;
}