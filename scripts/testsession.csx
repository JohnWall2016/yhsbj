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

// aac031:参保状态:     1-参保缴费, 2-暂停缴费, 3-终止缴费
// aac008:社会保险状态: 1-在职,     2-退休,     4-终止 
// sac007:缴费人员类别: 102-个体缴费, 101-单位在业人员
// aab300:社保机构名称
// aac003:姓名
// aac002:身份证号码

// 1. aac008 2
// 2. aab300 -> 获取地区编码
// 3. 按地区编码 身份证号码 -> 信息
// aic162:离退休日期       2011-12-01
// aic160:待遇开始享受日期 201201
// aae116:待遇发放状态     0-不可发放[暂停], 1-可发放[正常], 3-待遇终止[终止]
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
        var cs = new Cscbgrxxcx(session);
        
        for (var i = 2; i <= sheet.LastRowNum; i++)
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

            var sbzt = sheet.Cell(i, 14)?.StringCellValue;
            if (sbzt != "4") continue;

            var idcard = sheet.Cell(i, 4).StringCellValue;
            var list = cx.Search(idcard);

            var sbjg = sheet.Cell(i, 12)?.StringCellValue ?? "";
            Console.WriteLine($"{i}:{sbjg}");
            //if (!Cscbgrxxcx.IsInArea(sbjg)) continue;

            if (list.Count > 0)
            {
                var dict = list[0];
                name = dict["aac003"];
                id = dict["aac002"];
                cbzt = dict["aac031"];
                shbxzt = dict["aac008"];
                sbjgmc = dict["aab300"];
                sbjgbm = sbbm.GetBm(sbjgmc);

                Console.WriteLine(shbxzt);

                if (shbxzt == "4")
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
                    if (iltxs.Count == 0)
                    {
                        var rset = cs.Search(id, sbjgmc);
                        if (rset.Count > 0)
                        {
                            ltxrq = rset[0]["aic162"];
                            if (rset[0].SubItems.Count > 0)
                            {
                                dwmc = rset[0].SubItems[0]["aab004"];
                                dyffzt = rset[0].SubItems[0]["aae116"];
                                dykssj = rset[0].SubItems[0]["aic160"];
                            }
                            if (rset[0].SubItems.Count > 1)
                                memo = "CS有多条待遇记录";
                        }
                        if (rset.Count > 1)
                        {
                            var bz = "CS有多条参保记录";
                            memo = memo != "" ? bz + "|" + memo : bz;
                        }
                    }
                    else if (iltxs.Count > 1)
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

void searchCS(string pid)
{
    Session.Using(session =>
    {
        var cx = new Sncbrycx(session);
        var cs = new Cscbgrxxcx(session);

        var list = cx.Search(pid);
        if (list.Count > 0)
        {
            var name = list[0]["aac003"];
            var id = list[0]["aac002"];
            var sbjgmc = list[0]["aab300"];
            Console.WriteLine(list[0].ToDictString());
            
            var rset = cs.Search(id, sbjgmc);
            foreach (var dict in rset)
            {
                Console.WriteLine(dict.ToDictString());
                foreach (var d in dict.SubItems)
                {
                    Console.WriteLine(d.ToDictString());
                }
            }
        }
    });
}

// aac031:参保状态:     1-参保缴费, 2-暂停缴费, 3-终止缴费
// aac008:社会保险状态: 1-在职,     2-退休,     4-终止 
// sac007:缴费人员类别: 102-个体缴费, 101-单位在业人员
// aab300:社保机构名称
// aac003:姓名
// aac002:身份证号码

// 1. aac008 2
// 2. aab300 -> 获取地区编码
// 3. 按地区编码 身份证号码 -> 信息
// aic162:离退休日期       2011-12-01
// aic160:待遇开始享受日期 201201
// aae116:待遇发放状态     0-不可发放[暂停], 1-可发放[正常], 3-待遇终止[终止]
// aab004:单位名称
void updateSbzt2(string xls = @"D:\残疾特困\26873扶贫台账20181224.new.xls")
{
    IWorkbook workbook = ExcelExtension.LoadExcel(xls);
    var sheet = workbook.GetSheetAt(1);
    Session.Using(session =>
    {
        var cx = new Sncbrycx(session);
        var sbbm = new Sbjgbm(session);
        var ltx = new Ltxrycxtj(session);
        var cs = new Cscbgrxxcx(session);
        
        for (var i = 2; i <= sheet.LastRowNum; i++)
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

            var idcard = sheet.Cell(i, 2).StringCellValue;

            Console.WriteLine($"{i} {idcard}");

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

                Console.WriteLine(shbxzt);

                if (shbxzt == "4")
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
                    if (iltxs.Count == 0)
                    {
                        var rset = cs.Search(id, sbjgmc);
                        if (rset.Count > 0)
                        {
                            ltxrq = rset[0]["aic162"];
                            if (rset[0].SubItems.Count > 0)
                            {
                                dwmc = rset[0].SubItems[0]["aab004"];
                                dyffzt = rset[0].SubItems[0]["aae116"];
                                dykssj = rset[0].SubItems[0]["aic160"];
                            }
                            if (rset[0].SubItems.Count > 1)
                                memo = "CS有多条待遇记录";
                        }
                        if (rset.Count > 1)
                        {
                            var bz = "CS有多条参保记录";
                            memo = memo != "" ? bz + "|" + memo : bz;
                        }
                    }
                    else if (iltxs.Count > 1)
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

            sheet.Row(i).CreateCell(10).SetValue(cbzt);
            sheet.Row(i).CreateCell(11).SetValue(shbxzt);
        }
    });

    workbook.Save(Utils.FileNameAppend(xls, ".new"));
    workbook.Close();
}

//updateSbzt2();
//searchCS("430311195203261517");

// aac031:参保状态:     1-参保缴费, 2-暂停缴费, 3-终止缴费
// aac008:社会保险状态: 1-在职,     2-退休,     4-终止 
// sac007:缴费人员类别: 102-个体缴费, 101-单位在业人员
// aab300:社保机构名称
// aac003:姓名
// aac002:身份证号码

// 1. aac008 2
// 2. aab300 -> 获取地区编码
// 3. 按地区编码 身份证号码 -> 信息
// aic162:离退休日期       2011-12-01
// aic160:待遇开始享受日期 201201
// aae116:待遇发放状态     0-不可发放[暂停], 1-可发放[正常], 3-待遇终止[终止]
// aab004:单位名称
void updateSbzt3(string xls = @"D:\Downloads\银海培训第一期名册（育婴员）.xlsx")
{
    IWorkbook workbook = ExcelExtension.LoadExcel(xls);
    var sheet = workbook.GetSheetAt(0);
    Session.Using(session =>
    {
        var cx = new Sncbrycx(session);
        var sbbm = new Sbjgbm(session);
        var ltx = new Ltxrycxtj(session);
        var cs = new Cscbgrxxcx(session);
        
        for (var i = 3; i <= sheet.LastRowNum; i++)
        {
            var name = "";
            var id = "";
            var cbzt = "";
            var shbxzt = "";
            var jfrylb = "";
            var sbjgmc = "";
            var sbjgbm = "";

            var dwmc = "";
            var dyffzt = "";
            var ltxrq = "";
            var dykssj = "";

            var memo = "";

            var idcard = sheet.Cell(i, 3).StringCellValue;
            var list = cx.Search(idcard);

            if (list.Count > 0)
            {
                var dict = list[0];
                name = dict["aac003"];
                id = dict["aac002"];
                cbzt = dict["aac031"];
                shbxzt = dict["aac008"];
                jfrylb = dict["sac007"];
                sbjgmc = dict["aab300"];
                sbjgbm = sbbm.GetBm(sbjgmc);

                //Console.WriteLine(shbxzt);

                if (shbxzt == "4")
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
                    if (iltxs.Count == 0)
                    {
                        var rset = cs.Search(id, sbjgmc);
                        if (rset.Count > 0)
                        {
                            ltxrq = rset[0]["aic162"];
                            if (rset[0].SubItems.Count > 0)
                            {
                                dwmc = rset[0].SubItems[0]["aab004"];
                                dyffzt = rset[0].SubItems[0]["aae116"];
                                dykssj = rset[0].SubItems[0]["aic160"];
                            }
                            if (rset[0].SubItems.Count > 1)
                                memo = "CS有多条待遇记录";
                        }
                        if (rset.Count > 1)
                        {
                            var bz = "CS有多条参保记录";
                            memo = memo != "" ? bz + "|" + memo : bz;
                        }
                    }
                    else if (iltxs.Count > 1)
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
                msg += $"|{name}|{sbjgmc}|{cbzt}|{jfrylb}|{shbxzt}|{dwmc}|{ltxrq}|{dykssj}|{dyffzt}|{memo}";
            Console.WriteLine(msg);

            sheet.Row(i).CreateCell(12).SetValue(sbjgmc);
            sheet.Row(i).CreateCell(13).SetValue(cbzt);
            sheet.Row(i).CreateCell(14).SetValue(jfrylb);
            sheet.Row(i).CreateCell(15).SetValue(shbxzt);
        }
    });

    workbook.Save(Utils.FileNameAppend(xls, ".new"));
    workbook.Close();
}

void updateSbzt4(string xls = @"D:\Downloads\20190925雨湖区村（居）两委班子成员基本情况登记表.xlsx")
{
    IWorkbook workbook = ExcelExtension.LoadExcel(xls);
    var sheet = workbook.GetSheetAt(0);
    Session.Using(session =>
    {
        var cx = new Sncbrycx(session);
        var sbbm = new Sbjgbm(session);
        var ltx = new Ltxrycxtj(session);
        var cs = new Cscbgrxxcx(session);
        
        for (var i = 2; i <= sheet.LastRowNum; i++)
        {
            var name = "";
            var id = "";
            var cbzt = "";
            var shbxzt = "";
            var jfrylb = "";
            var sbjgmc = "";
            var sbjgbm = "";

            var dwmc = "";
            var dyffzt = "";
            var ltxrq = "";
            var dykssj = "";

            var memo = "";

            var idcard = sheet.Cell(i, 9).StringCellValue;
            var list = cx.Search(idcard);

            if (list.Count > 1)
            {
                var bz = "有多条参保记录";
                memo = bz;
            }
            var msg = $"{i}:{idcard}";
            if (list.Count == 0)
                msg += "|未参保";
            else
                msg += $"|{name}|{sbjgmc}|{cbzt}|{jfrylb}|{shbxzt}|{dwmc}|{ltxrq}|{dykssj}|{dyffzt}|{memo}";
            Console.WriteLine(msg);

            sheet.Row(i).CreateCell(18).SetValue(memo);
        }
    });

    workbook.Save(Utils.FileNameAppend(xls, ".new"));
    workbook.Close();
}

//updateSbzt4();

void fetchDyqk(string idcard)
{
    Session.Using(session =>
    {
        var cx = new Sncbrycx(session);
        var sbbm = new Sbjgbm(session);
        var ltx = new Ltxrycxtj(session);
        var cs = new Cscbgrxxcx(session);
        

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

            var txj = "";

            var memo = "";

            Console.WriteLine($"{idcard}");

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

                Console.WriteLine(dict);
                Console.WriteLine(shbxzt);

                if (shbxzt == "4" || shbxzt == "2")
                {
                    var iltxs = ltx.Search(id, sbjgbm);
                    if (iltxs.Count > 0)
                    {
                        var iltx = iltxs[0];
                        dwmc = iltx["aab004"];
                        dyffzt = iltx["aae116"];
                        ltxrq = iltx["aic162"];
                        dykssj = iltx["aic160"];
                        txj = iltx["txj"];
                    }
                    if (iltxs.Count == 0)
                    {
                        var rset = cs.Search(id, sbjgmc);
                        if (rset.Count > 0)
                        {
                            ltxrq = rset[0]["aic162"];
                            if (rset[0].SubItems.Count > 0)
                            {
                                dwmc = rset[0].SubItems[0]["aab004"];
                                dyffzt = rset[0].SubItems[0]["aae116"];
                                dykssj = rset[0].SubItems[0]["aic160"];
                                txj = rset[0].SubItems[0]["txj"];
                            }
                            if (rset[0].SubItems.Count > 1)
                                memo = "CS有多条待遇记录";
                        }
                        if (rset.Count > 1)
                        {
                            var bz = "CS有多条参保记录";
                            memo = memo != "" ? bz + "|" + memo : bz;
                        }
                    }
                    else if (iltxs.Count > 1)
                        memo = "有多条待遇记录";
                }
            }
            if (list.Count > 1)
            {
                var bz = "有多条参保记录";
                memo = memo != "" ? bz + "|" + memo : bz;
            }
            var msg = $"{idcard}";
            if (list.Count == 0)
                msg += "|未参保";
            else
                msg += $"|{name}|{sbjgmc}|{cbzt}|{shbxzt}|{dwmc}|{ltxrq}|{dykssj}|{dyffzt}|{txj}|{memo}";
            Console.WriteLine(msg);
    });
}

fetchDyqk("432501192608057027")