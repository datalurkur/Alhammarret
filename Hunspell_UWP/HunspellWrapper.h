#pragma once

#include <string>
#include "hunspell/hunspell.h"

namespace Hunspell_UWP
{
	public ref class HunspellSuggestions sealed
	{
	public:
		HunspellSuggestions(int count);

		Platform::String^ Get(int index);
		int Count();

		void SetSuggestion(int index, Platform::String^ str);

	private:
		std::vector<Platform::String^> suggestions;
	};

    public ref class HunspellWrapper sealed
    {
    public:
        HunspellWrapper(Platform::String^ dictionaryPath, Platform::String^ affixPath);
		virtual ~HunspellWrapper();

		bool IsWordValid(Platform::String^ word);
		HunspellSuggestions^ GetSuggestions(Platform::String^ word);

	private:
		bool StringToChar(Platform::String^ word, char* buffer, int bufferSize);

	private:
		Hunhandle* handle;
    };
}
