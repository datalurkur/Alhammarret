#include "stdafx.h"

using namespace System;
using namespace Recognizer;

bool Test_OCRFromTransformedCard(CardRecognizer^ recognizer, System::String^ path, System::String^ expectedText)
{
    recognizer->SetIntermediateData(CardRecognizer::IntermediateMat::Transformed, path);
    String^ result = recognizer->RecognizeText();
    return (result == expectedText);
}

int main(array<System::String ^> ^args)
{
    String^ cwd = System::IO::Directory::GetCurrentDirectory();
    CardRecognizer^ recognizer = gcnew CardRecognizer();
    if (!Test_OCRFromTransformedCard(recognizer, "TestAssets\\TirelessTracker_Cropped.jpg", "Tireless Tracker"))
    {
        Console::WriteLine("Failed");
    }
    return 0;
}
