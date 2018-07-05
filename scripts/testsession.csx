#! "netcoreapp2.0"
#r "nuget: YAXLib, 2.15.0"
#r "nuget: System.Text.Encoding.CodePages, 4.5.0"
#r "nuget: SharpZipLib, 1.0.0-alpha2"
#r "../src/YHSBJ.SBGLPT/bin/Debug/netstandard2.0/YHSBJ.SBGLPT.dll"
#r "../../yhcjb/src/YHCJB.Util/bin/Debug/netstandard2.0/NPOI.dll"
#r "../../yhcjb/src/YHCJB.Util/bin/Debug/netstandard2.0/NPOI.OOXML.dll"
#r "../../yhcjb/src/YHCJB.Util/bin/Debug/netstandard2.0/YHCJB.Util.dll"

using YHSBJ.SBGLPT;
using YHCJB.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

void sbqkcx(string idcard)
{
    Session.Using(session =>
    {
        var cx = new Sncbrycx(session);
        //Console.WriteLine(cx.MetaData.ToDictString());

        var sbbm = new Sbjgbm(session);
        //Console.WriteLine(sbbm.ToDictString());
    
        var ltx = new Ltxrycxtj(session);
        //Console.WriteLine(ltx.MetaData.ToDictString());
    
        Action<string, string> print = (pid, msg) =>
        {
            var list = cx.Search(pid);
            foreach (var dict in list)
            {
                Console.WriteLine("=======================================================");
                var items = new List<string>();
                items.Add(msg);
                items.Add(dict["aac003"]);
                var id = dict["aac002"];
                items.Add(id);
                var st = dict["aac008"];
                items.Add(st);
                items.Add(dict["aab300"]);
                var bm = sbbm.GetBm(dict["aab300"]);
                items.Add(bm);
                Console.WriteLine(string.Join("|", items));

                if (st == "2")
                {
                    foreach (var iltx in ltx.Search(id, bm))
                    {
                        var dwmc = iltx["aab004"];
                        var dyffzt = iltx["aae116"];
                        var ltxrq = iltx["aic162"];
                        var dykssj = iltx["aic160"];
                
                        Console.WriteLine($"{dwmc}|{dyffzt}|{ltxrq}|{dykssj}");
                    }
                }
                Console.WriteLine("=======================================================");
            }
        };

        //print("43030219270804004X", "职保正常待遇");
        //print("43032119421116052X", "职保待遇暂停");
        //print("430302193111210020", "职保待遇终止");
        //print("430302193607020028", "职保正常参保");
        //print("430302199008105027", "职保暂停参保");
        //print("430302193904050020", "职保正常待遇");
        print(idcard, "");
    });
}

// aac031:参保状态:     3-终止缴费, 1-参保缴费, 2-暂停缴费
// aac008:社会保险状态: 2-退休,     1-在职
// sac007:缴费人员类别: 102-个体缴费, 101-单位在业人员
// aab300:社保机构名称
// aac003:姓名
// aac002:身份证号码

// 1. aac008 2
// 2. aab300 -> 获取地区编码
// 3. 按地区编码 身份证号码 -> 信息
// aic162:离退休日期       2011-12-01
// aic160:待遇开始享受日期 201201
// aae116:待遇发放状态     1-可发放[正常], 0-不可发放[暂停], 3-待遇终止[终止]
// aab004:单位名称
void updateSbzt(string xls = @"D:\数据核查\雨湖区2012到2016年历年暂停停人员名册表\雨湖区2012到2016年历年暂停停人员名册表（职保比对）.xlsx")
{
    IWorkbook workbook = ExcelExtension.LoadExcel(xls);
    var sheet = workbook.GetSheetAt(0);
    Session.Using(session =>
    {
        var cx = new Sncbrycx(session);
        var sbbm = new Sbjgbm(session);
        var ltx = new Ltxrycxtj(session);
        
        for (var i = 151; i <= 500/*sheet.LastRowNum*/; i++)
        {
            var name = "";
            var id = "";
            var cbzt = "";
            var shbxzt = "";
            var sbjgmc = "";
            var sbjgbm = "";

            var dwmc = "";
            var dyffzt = "";
            var ltxrq = "";
            var dykssj = "";

            var memo = "";

            var idcard = sheet.Cell(i, 4).StringCellValue;
            var list = cx.Search(idcard);

            if (list.Count > 0)
            {
                var dict = list[0];
                name = dict["aac003"];
                id = dict["aac002"];
                cbzt = dict["aac031"];
                shbxzt = dict["aac008"];
                sbjgmc = dict["aab300"];
                sbjgbm = sbbm.GetBm(sbjgmc);

                if (shbxzt == "2")
                {
                    var iltxs = ltx.Search(id, sbjgbm);
                    if (iltxs.Count > 0)
                    {
                        var iltx = iltxs[0];
                        dwmc = iltx["aab004"];
                        dyffzt = iltx["aae116"];
                        ltxrq = iltx["aic162"];
                        dykssj = iltx["aic160"];
                    }
                    if (iltxs.Count > 1)
                        memo = "有多条待遇记录";   
                }
            }
            if (list.Count > 1)
            {
                var bz = "有多条参保记录";
                memo = memo != "" ? bz + "|" + memo : bz;
            }
            var msg = $"{i}:{idcard}";
            if (list.Count == 0)
                msg += "|未参保";
            else
                msg += $"|{name}|{sbjgmc}|{cbzt}|{shbxzt}|{dwmc}|{ltxrq}|{dykssj}|{dyffzt}|{memo}";
            Console.WriteLine(msg);

            sheet.Row(i).CreateCell(11).SetValue(name);
            sheet.Row(i).CreateCell(12).SetValue(sbjgmc);
            sheet.Row(i).CreateCell(13).SetValue(cbzt);
            sheet.Row(i).CreateCell(14).SetValue(shbxzt);
            sheet.Row(i).CreateCell(15).SetValue(dwmc);
            sheet.Row(i).CreateCell(16).SetValue(ltxrq);
            sheet.Row(i).CreateCell(17).SetValue(dykssj);
            sheet.Row(i).CreateCell(18).SetValue(dyffzt);
            sheet.Row(i).CreateCell(19).SetValue(memo);
        }
    });

    workbook.Save(Utils.FileNameAppend(xls, ".new"));
    workbook.Close();
}

updateSbzt();
