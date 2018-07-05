#! "netcoreapp2.0"
#r "nuget: YAXLib, 2.15.0"
#r "nuget: System.Text.Encoding.CodePages, 4.5.0"
#r "../src/YHSBJ.SBGLPT/bin/Debug/netstandard2.0/YHSBJ.SBGLPT.dll"

using YHSBJ.SBGLPT;

var dict = new Dictionary<string, string>
{
    ["1"] = "A",
    ["2"] = "B",
};

Console.WriteLine(dict["1"]);
Console.WriteLine(dict.ToString());
Console.WriteLine(dict.ToDictString());
