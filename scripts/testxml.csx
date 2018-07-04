#! "netcoreapp2.0"
#r "nuget: YAXLib, 2.15.0"
#r "../src/YHSBJ.SBGLPT/bin/Debug/netstandard2.0/YHSBJ.SBGLPT.dll"

using YHSBJ.SBGLPT;

var env = new Envelope<Input>
{
    Header = new Input("system"),
    Body  = new Input("business")
};
env.Header.Params.Add("usr", "hqm");
env.Header.Params.Add("pwd", "YLZ_A2A5F63315129CB2998A0E0FCE31BA51");
env.Header.Params.Add("funid", "F00.00.00.00|192.168.1.110|PC-20170427DGON|00-05-0F-08-1A-34");

string xml = env.ToString();
Console.WriteLine(xml);

env = Envelope<Input>.Load(xml);
Console.WriteLine(env.encodingStyle);
foreach (var (k, v) in env.Header.Params)
{
    Console.WriteLine("{0}:{1}", k, v);
}

var outEnv = Envelope<Output>.Load(
@"<?xml version=""1.0"" encoding=""GBK""?><soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" soap:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/""><soap:Header><result sessionID=""hpyzb7GLwxhWmPrd9JMbD9K8rsPp2DTKV8TYnJ11FsQ5zTjxW1QP!1220643430!1530594344225""/><result message=""查询同步数据库信息成功""/></soap:Header><soap:Body><out:business xmlns:out=""http://www.molss.gov.cn/""><result versionid="""" /><result result=""查询同步数据库信息成功"" /><resultset name=""outtable""><row versionid=""45067"" enabled=""1"" controlname=""B06.04.02"" parentid=""M01.02"" functiondesc=""网上经办灵活就业人员在职停保"" imageid=""0"" sstype=""100"" flag=""1"" helpid=""0"" functionid=""B06.04.02"" bplname=""dagl.bpl"" menuindex=""102414"" comments="""" classid="""" /><row versionid=""45068"" enabled=""1"" controlname=""B06.04.03"" parentid=""M01.02"" functiondesc=""网上经办灵活就业人员在职续保"" imageid=""0"" sstype=""100"" flag=""1"" helpid=""0"" functionid=""B06.04.03"" bplname=""dagl.bpl"" menuindex=""102413"" comments="""" classid="""" /></resultset></out:business></soap:Body></soap:Envelope>"                               
                               );

void printDictList(List<Dictionary<string, object>> list)
{
    foreach (var e in list)
    {
        foreach (var (k, v) in e)
            Console.Write("{0}:{1}|", k, v);
        Console.WriteLine();
    }
}

Console.WriteLine("header:{0}", outEnv.Header.Name);
printDictList(outEnv.Header.Results);
Console.WriteLine("header.resultset:{0}", outEnv.Header.Resultset.Name);
printDictList(outEnv.Header.Resultset.Rows);

Console.WriteLine("body:{0}", outEnv.Body.Name);
printDictList(outEnv.Body.Results);
Console.WriteLine("header.resultset:{0}", outEnv.Body.Resultset.Name);
printDictList(outEnv.Body.Resultset.Rows);
