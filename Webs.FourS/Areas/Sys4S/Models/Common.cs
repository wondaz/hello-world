using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web;
using Frame.Core;

namespace Web.FourS.Areas.Sys4S.Models
{
    [Module("DMS_4S")]
    public class CommonService : ServiceBase<ModelBase>
    {
        public string GetDictText(string type, string value)
        {
            var sql = string.Format(
                "SELECT Text FROM sys_code WHERE CodeType='{0}' AND Value='{1}'", type, value);
            var result = db.Sql(sql).QuerySingle<string>();
            return result;
        }

        public string GetDictDefaultVal(string codeType)
        {
            var sql = string.Format(
                "SELECT top (1) value FROM sys_code WHERE CodeType='{0}' AND IsDefault = 1", codeType);
            var result = db.Sql(sql).QuerySingle<string>();
            return result;
        }

        public dynamic UploadFiles(HttpRequest request, string folder)
        {
            try
            {
                if (request.Files.Count == 0)
                {
                    return new { result = "" };
                }

                var postFile = request.Files[0];
                var fileName = postFile.FileName;
                string fileExtension = Path.GetExtension(fileName).ToLower();
                if (fileExtension == ".exe" || fileExtension == ".bat")
                {
                    return new { result = "不允许上传此类型的文件" };
                }

                var fileSize = postFile.ContentLength / 1024.0 / 1024.0;
                // 检查文件大小, ContentLength获取的是字节，转成M的时候要除以2次1024
                var maxLength = Convert.ToInt32(ConfigurationManager.AppSettings["Upload_FileSize"]);
                if (fileSize > maxLength)
                {
                    return string.Format("只能上传小于{0}M的文件！", maxLength);
                }

                string configPath = ConfigurationManager.AppSettings["Upload_Path"];
                var dateDir = DateTime.Today.ToString("yyyy-MM-dd");
                var uploadPath = Path.Combine(configPath, folder, dateDir);
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var newFileName = FormsAuth.UserData.CorpID + "_" + fileName;
                string fileFullPath = Path.Combine(uploadPath, newFileName);
                var relativePath = string.Format("{0}/{1}/{2}", folder, dateDir, newFileName);
                postFile.SaveAs(fileFullPath);
                return new { fileFullPath, relativePath, fileName, fileSize, fileExtension, result = "ok" };
            }
            catch (Exception e)
            {
                return new { result = e.Message };
            }
        }
    }

}
