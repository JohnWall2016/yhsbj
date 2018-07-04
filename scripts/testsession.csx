#! "netcoreapp2.0"
#r "nuget: YAXLib, 2.15.0"
#r "nuget: System.Text.Encoding.CodePages, 4.5.0"
#r "../src/YHSBJ.SBGLPT/bin/Debug/netstandard2.0/YHSBJ.SBGLPT.dll"

using YHSBJ.SBGLPT;

Session.Using(session =>
{
    var cx = new Sncbrycx(session);
    foreach (var (k, v) in cx.MetaData)
        Console.Write("{0}:{1}|", k, v);
    Console.WriteLine();

    var list = cx.Search("430302195806251012");
    foreach (var dict in list)
    {
        foreach (var (k, v) in dict)
            Console.Write("{0}:{2}:{1}|", k, v, cx.MetaData.GetMetaData(k));
        Console.WriteLine();
    }
});
