// HunspellCLR.h

#include <string>
#include "hunspell/hunspell.h"

using namespace std;
using namespace System;
using namespace System::Collections::Generic;

namespace HunspellCLR
{
	public ref class HunspellWrapper
	{
	public:
		HunspellWrapper(String^ dictionaryPath, String^ affixPath);
		virtual ~HunspellWrapper();

		bool IsWordValid(String^ word);
		List<String^>^ GetSuggestions(String^ word);

	private:
		Hunhandle* handle;
	};
}
