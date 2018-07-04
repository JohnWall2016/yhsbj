#! "netcoreapp2.0"
#r "nuget: YAXLib, 2.15.0"
#r "nuget: System.Text.Encoding.CodePages, 4.5.0"
#r "../src/YHSBJ.SBGLPT/bin/Debug/netstandard2.0/YHSBJ.SBGLPT.dll"

using YHSBJ.SBGLPT;

Session.Using(session =>
{
    /*session.SendInput(inEnv =>
    {
        inEnv.Header.Params.Add("funid", "F00.01.03");
        inEnv.Body.Params.Add("startrow", "1");
        inEnv.Body.Params.Add("row_count", "-1");
        inEnv.Body.Params.Add("pagesize", "500");
        inEnv.Body.Params.Add("clientsql", "( aac002 = &apos;430302195806251012&apos;)");
        inEnv.Body.Params.Add("functionid", "F27.06");
    });
    session.GetOutput();*/
    var cx = new Sncbrycx(session);
    foreach (var (k, v) in cx.MetaData)
        Console.Write("{0}:{1}|", k, v);
    Console.WriteLine();

    var list = cx.Search("430302195806251012");
    foreach (var dict in list)
    {
        foreach (var (k, v) in dict)
            Console.Write("{0}:{2}:{1}|", k, v, dict.GetMetaData(k, cx.MetaData));
        Console.WriteLine();
    }

});
