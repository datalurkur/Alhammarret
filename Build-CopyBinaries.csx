using System;

bool isDebugBuild = false;
string platform = "Win32";
string destination = "";

string[] args = Environment.GetCommandLineArgs();
for (int i = 0; i < args.Length; ++i)
{
    if (args[i].Length == 0) { continue; }
    if (args[i][0] == '-')
    {
        int equalIndex = args[i].IndexOf("=");
        if (equalIndex != -1)
        {
            string argValue = args[i].Substring(equalIndex + 1);
            if (args[i].StartsWith("-Config"))
            {
                isDebugBuild = (argValue == "Debug");
            }
            else if (args[i].StartsWith("-Platform"))
            {
                platform = argValue;
            }
        }
        else
        {
            // Must be a flag with no value
        }
    }
    else
    {
        // This must be the destination path
        destination = args[i];
    }
}

string binSource = $"..\\opencv\\{platform}\\bin\\";
string[] binFiles = new string[] {
    "opencv_core310",
    "opencv_imgproc310",
    "opencv_imgcodecs310",
    "opencv_ml310",
    "opencv_text310",
    "opencv_videoio310",
    "libtesseract302",
    "liblept168"
};
bool[] binHasDebug = new bool[] {
    true,
    true,
    true,
    true,
    true,
    true,
    false,
    false
};
string assetsSource = $"..\\Assets";
string testAssetsSource = $"..\\TestAssets";

// TODO - validate that this is actually a legit path

Console.WriteLine("Beginning file copy...");
Console.WriteLine("Copying binaries...");
for (int i = 0; i < binFiles.Length; ++i)
{
    string fileName = binFiles[i];
    if (binHasDebug[i] && isDebugBuild)
    {
        fileName += "d.dll";
    }
    else
    {
        fileName += ".dll";
    }
    string fullSourceName = binSource + fileName;
    string fullDestName = destination + fileName;
    Console.WriteLine($"  -Copying {fullSourceName} to {fullDestName}");
    File.Copy(fullSourceName, fullDestName, true);
}

Console.WriteLine($"Copying assets...");
string assetDir = destination + "Assets";
if (!Directory.Exists(assetDir))
{
    Directory.CreateDirectory(assetDir);
}
foreach (string asset in Directory.GetFiles(assetsSource))
{
    string destAsset = Path.Combine(assetDir, Path.GetFileName(asset));
    Console.WriteLine($"  -Copying {asset} to {destAsset}");
    File.Copy(asset, destAsset, true);
}

Console.WriteLine($"Copying test assets...");
assetDir = destination + "TestAssets";
if (!Directory.Exists(assetDir))
{
    Directory.CreateDirectory(assetDir);
}
foreach (string testAsset in Directory.GetFiles(testAssetsSource))
{
    string destAsset = Path.Combine(assetDir, Path.GetFileName(testAsset));
    Console.WriteLine($"  -Copying {testAsset} to {destAsset}");
    File.Copy(testAsset, destAsset, true);
}

Console.WriteLine("Copy done!");
