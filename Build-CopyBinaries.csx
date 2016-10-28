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

string openCVSourceFolder = $"..\\opencv\\{platform}\\bin\\";
string[] openCVFiles = new string[] {
    "opencv_core310",
    "opencv_imgproc310",
    "opencv_imgcodecs310",
    "opencv_videoio310"
};

// TODO - validate that this is actually a legit path

Console.WriteLine("Beginning file copy...");
for (int i = 0; i < openCVFiles.Length; ++i)
{
    string fileName = openCVFiles[i] + (isDebugBuild ? "d" : "") + ".dll";
    string fullSourceName = openCVSourceFolder + fileName;
    string fullDestName = destination + fileName;
    Console.WriteLine($"  -Copying {fullSourceName} to {fullDestName}");
    File.Copy(fullSourceName, fullDestName, true);
}
Console.WriteLine("Copy done!");
