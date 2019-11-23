using(['calendar']);
var vms = vms || {};
vms.search = function (vdata) {
    var self = this;
    //this.pkey = vdata.pkey;
    this.form = ko.mapping.fromJS(vdata.form);
    this.dform = ko.mapping.fromJS(vdata.dform);
    this.ds = vdata.dataSource;
    this.urls = vdata.urls;
    delete this.form.__ko_mapping__;
    delete this.dform.__ko_mapping__;

    this.grid = $.extend({
        size: { w: 4, h: 190 },
        url: self.urls.search,
        queryParams: ko.observable(),
        pagination: false,
        customLoad: false
    }, (self.selfGrid || {}));

    this.grid.queryParams($.extend({ _xml: vdata._xml }, vdata.form));

    //查询
    this.searchClick = function () {
        var param = ko.toJS(self.form);
        self.grid.queryParams($.extend({ _xml: vdata._xml }, param));
    };

    this.saveClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选中入库车辆！');
        if (!com.formValidate()) {
            com.message('warning', '请输入必填项！');
            return;
        }

        self.dform.VIN(row["VIN"]);
        self.dform.CostPrice(row["CostPrice"]);
        com.message('confirm', '确定入库吗？', function (b) {
            com.ajax({
                url: self.urls.save,
                data: ko.toJSON(self.dform),
                success: function (d) {
                    if (d == 'ok') {
                        com.message('success', '车辆入库成功');
                        window.location.reload();
                    } else {
                        com.message('error', '保存失败:' + d.msg);
                    }
                }
            });
        });        
    };
};

