using FluentData;
using Frame.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.FourS.Areas.Biz4S.Models
{
    [Module("DMS_4S")]
    public class SparePartService : ServiceBase<SparePart>
    {
        public List<dynamic> GetPartsSpell(string spellCode)
        {
            if (string.IsNullOrEmpty(spellCode)) return null;

            var data = db.Sql(string.Format("SELECT TOP 30 CONCAT(SparePartCode ,'/',SparePartName) FROM B_SparePart where [State] = 0 AND SpellAb LIKE '%{0}%' ", spellCode.Replace("'", ""))).QueryMany<dynamic>();
            return data;
        }

        public dynamic GetSpareParts(string partsCode)
        {
            if (string.IsNullOrEmpty(partsCode)) return null;

            var data = db.Sql(@"
SELECT B.SparePartCode, 
       B.SparePartName, 
       B.SpellAb, 
       B.Spec, 
       B.Unit, 
       P.Price,
	   ISNULL(S.Quantity,0) Stock
FROM B_SparePart B
     INNER JOIN B_PartPrice P ON B.SparePartCode = P.SparePartCode
	 LEFT JOIN P_AssemStorage S ON (B.SparePartCode = S.SparePartCode AND S.CorpID=9)
WHERE B.State = 0
      AND B.SparePartCode = @0", partsCode.Replace("'", "")).QuerySingle<dynamic>();
            return data;
        }

        public string ImportParts(JArray data)
        {
            string result = "";
            var user = FormsAuth.UserData;
            IStoredProcedureBuilder builder;
            var dbContext = db.UseTransaction(true);
            using (dbContext)//开启事务
            {
                Logger("编辑备件出库", () =>
                {
                    foreach (var jToken in data)
                    {
                        var item = (JObject)jToken;
                        if(item["SparePartCode"] == null || item["Quantity"] == null) continue;

                        builder = db.StoredProcedure("SP_AssemStorage_Import")
                            .Parameter("CorpID", user.CorpID)
                            .Parameter("UpdatePerson",user.UserName)
                            .Parameter("PartTypeName", item["PartTypeName"] == null ? "" : item["PartTypeName"].ToString())
                            .Parameter("SparePartCode", item["SparePartCode"] == null ? "" : item["SparePartCode"].ToString())
                            .Parameter("SparePartName", item["SparePartName"] == null ? "" : item["SparePartName"].ToString())
                            .Parameter("Spec", item["Spec"] == null ? "" : item["Spec"].ToString())
                            .Parameter("Unit", item["Unit"] == null ? "" : item["Unit"].ToString())
                            .Parameter("StockName", item["StockName"] == null ? "" : item["StockName"].ToString())
                            .Parameter("Price", Convert.ToDecimal(item["Price"]))
                            .Parameter("Quantity", Convert.ToDecimal(item["Quantity"]))
                            .ParameterOut("Result", DataTypes.String, 200);
                        builder.Execute();
                        result = builder.ParameterValue<string>("Result");
                        if (result != "ok") break;
                    }

                    if (result == "ok")
                    {
                        dbContext.Commit();
                    }

                }, ex =>
                {
                    dbContext.Rollback();
                    result = ex.Message;
                });
            }

            return result;
        }
    }

    public class SparePart : ModelBase
    {
    }
}