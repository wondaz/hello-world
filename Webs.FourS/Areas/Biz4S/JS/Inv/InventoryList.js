using(['calendar']);
var vms = vms || {};
vms.search = function (vdata) {
    var self = this;
    this.form = ko.mapping.fromJS(vdata.form);
    this.urls = vdata.urls;
    delete this.form.__ko_mapping__;

    this.grid = $.extend({
        size: { w: 4, h: 68 },
        url: self.urls.search,
        queryParams: ko.observable(),
        pagination: true,
        customLoad: false
    }, (self.selfGrid || {}));

    this.grid.queryParams($.extend({ _xml: vdata._xml }, vdata.form));

    //查询
    this.searchClick = function () {
        var param = ko.toJS(self.form);
        self.grid.queryParams($.extend({ _xml: vdata._xml }, param));
    };

    //清空查询条件
    this.clearClick = function () {
        $.each(self.form, function () { this(''); });
        self.searchClick();
    };

    //导出
    this.downloadClick = function (vm, event) {
        var dict = $(event.currentTarget).attr("dict");
        var title = $(document).attr("title");
        com.exporter(self.grid).download('xlsx', dict, title);
    };

    this.importClick = function () {
        var obj = $('#xlsxFile')[0];
        if (!obj.files.length > 0) {
            return;
        }
        var f = obj.files[0];
        var reader = new FileReader();
        var wb; //读取完成的数据
        reader.onload = function (e) {
            var edata = e.target.result;
            wb = XLSX.read(edata, {
                type: 'binary'
            });

            var jsonData = XLSX.utils.sheet_to_json(wb.Sheets[wb.SheetNames[0]]);
            var dict = { '备件类型': 'PartTypeName', '备件编码': 'SparePartCode', '备件名称': 'SparePartName', '规格型号': 'Spec', '计量单位': 'Unit', '仓库': 'StockName', '价格': 'Price', '库存量': 'Quantity' };
            var xlsxData = com.formatExcelData(jsonData, dict);
            com.ajax({
                url: self.urls.import,
                data: ko.toJSON(xlsxData),
                success: function (d) {
                    com.message('success', '导入成功！');
                    self.searchClick();
                }
            });
        };
        reader.readAsBinaryString(f);
    }
};

