using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Frame.Core.Dict;
using Newtonsoft.Json;
using Frame.Utils;

namespace Frame.Core
{
    public class Exporter
    {
        const string DefaultExport = "xls";
        const string DefaultDatagetter = "api";

        private readonly Dictionary<string, Type> _dataGetter = new Dictionary<string, Type>() { 
            { "api", typeof(ApiData) } 
        };
        private readonly Dictionary<string, Type> _export = new Dictionary<string, Type>() { 
            { "xls", typeof(XlsExport) }, 
            { "xlsx", typeof(XlsxExport) } ,
            { "doc", typeof(HtmlDocExport) },
            { "pdf", typeof(PdfExport) }
        };

        //private Dictionary<string,IFormatter> _fieldFormatter = new Dictionary<string,IFormatter>();

        private object _data;
        private List<List<Column>> _columns;
        private Stream _fileStream;
        private string _fileName = string.Empty;
        private string _suffix = string.Empty;
        private List<string> _codeList = new List<string>();
        public static Exporter Instance()
        {
            var export = new Exporter();
            export.GetParams();
            export.GetData();
            export.Export(HttpContext.Current.Request.Form["fileType"]);

            return export;
        }

        private void GetParams()
        {
            var context = HttpContext.Current;
            if (context.Request.Form["columns"] != null)
            {
                _columns = JsonConvert.DeserializeObject<List<List<Column>>>(context.Request.Form["columns"]);
            }

            _fileName = string.Format("{0}{1}",context.Request.Form["title"],DateTime.Now.ToString("yyyyMMddHHmmss"));
            _codeList = context.Request.Form["codeDict"].Split(',', '，').ToList();
        }

        private void GetData()
        {
            var dataGetter = GetActor<IDataGetter>(_dataGetter, DefaultDatagetter, DefaultDatagetter);
            _data = dataGetter.GetData(HttpContext.Current);
        }

        private void Export(string fileTtype)
        {
            var export = GetActor<IExport>(_export, DefaultExport, fileTtype);

            if (_columns == null)
            {
                _columns = new List<List<Column>> { new List<Column>() };
                EachHelper.EachListHeader(_data, (i, field, type) => _columns[0].Add(new Column() { title = field, field = field, rowspan = 1, colspan = 1 }));
            }

            Dictionary<int, int> currentHeadRow = new Dictionary<int, int>();
            Dictionary<string, List<int>> fieldIndex = new Dictionary<string, List<int>>();
            Func<int, int> getCurrentHeadRow = cell => currentHeadRow.ContainsKey(cell) ? currentHeadRow[cell] : 0;
            var currentRow = 0;
            var currentCell = 0;

            export.Init(_data);

            //生成多行题头
            for (var i = 0; i < _columns.Count; i++)
            {
                currentCell = 0;

                for (var j = 0; j < _columns[i].Count; j++)
                {
                    var item = _columns[i][j];
                    if (item.hidden) continue;

                    while (currentRow < getCurrentHeadRow(currentCell))
                        currentCell++;

                    export.FillData(currentCell, currentRow, "title_" + item.field, item.title);

                    if (item.rowspan + item.colspan > 2)
                        export.MergeCell(currentCell, currentRow, currentCell + item.colspan - 1, currentRow + item.rowspan - 1);

                    if (!string.IsNullOrEmpty(item.field))
                    {
                        if (!fieldIndex.ContainsKey(item.field))
                            fieldIndex[item.field] = new List<int>();
                        fieldIndex[item.field].Add(currentCell);
                    }

                    for (var k = 0; k < item.colspan; k++)
                        currentHeadRow[currentCell] = getCurrentHeadRow(currentCell++) + item.rowspan;
                }
                currentRow++;
            }

            //设置题头样式
            export.SetHeadStyle(0, 0, currentCell - 1, currentRow - 1);

            //设置数据样式
            var dataCount = 0;
            EachHelper.EachListRow(_data, (i, r) => dataCount++);
            export.SetRowsStyle(0, currentRow, currentCell - 1, currentRow + dataCount - 1);

            EachHelper.EachListRow(_data, (rowIndex, rowData) =>
            {
                var currRow = currentRow;
                EachHelper.EachColumnProperty(rowData, (i, name, value) =>
                {
                    if (fieldIndex.ContainsKey(name))
                        foreach (int cellIndex in fieldIndex[name])
                        {
                            if (_codeList.Contains(name, StringComparer.CurrentCultureIgnoreCase))
                            {
                                value = DataDict.Format(name, value);
                            }

                            export.FillData(cellIndex, currRow, name, value);
                        }
                });
                currentRow++;
            });

            _fileStream = export.SaveAsStream();

            _suffix = export.suffix;

        }

        public void Download()
        {
            if (_fileStream != null && _fileStream.Length > 0)
                ZFiles.DownloadFile(HttpContext.Current, _fileStream, string.Format("{0}.{1}", _fileName, _suffix), 1024 * 1024 * 10);
        }

        private T GetActor<T>(Dictionary<string, Type> dict, string defaultKey, string key)
        {
            if (string.IsNullOrEmpty(key) || !dict.ContainsKey(key))
            {
                key = defaultKey;
            }

            return (T)Activator.CreateInstance(dict[key]);
        }
    }
}
