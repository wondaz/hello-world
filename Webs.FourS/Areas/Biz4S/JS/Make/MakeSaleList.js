using(['calendar']);
var vms = vms || {};
vms.search = function (vdata) {
    var self = this;
    this.pkey = vdata.pkey;
    this.form = ko.mapping.fromJS(vdata.form);
    this.ds = vdata.dataSource;
    this.urls = vdata.urls;
    delete this.form.__ko_mapping__;

    this.grid = $.extend({
        size: { w: 4, h: 92 },
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

    //新增单据
    this.addClick = function () {
        com.openTab('新增预约订单', self.urls.edit);
    };
    //跳转到编辑页面
    this.editClick = function () {
        if (!self.urls.edit) return false;

        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条记录！');
        com.openTab('修改预约订单', self.urls.edit + row[self.pkey]);
    };

    self.grid.onDblClickRow = this.editClick;

    //删除
    this.deleteClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条记录！');
        if (row.BillState == "已审核") return com.message('warning', '已审核通过，不能删除！');
        if (row.IsOut == "1") return com.message('warning', '车辆已出库，不能删除！');
        var post = {};
        post.id=row[self.pkey];
        com.message('confirm', '确定删除吗？', function (b) {
            if (b) {
                com.ajax({
                    type: 'Post',
                    url: vdata.urls.delete,
                    data:ko.toJSON(post),
                    success: function (d) {
                        if (d === 'ok') {
                            self.searchClick();
                            com.message('success', '车辆删除成功！');
                        } else {
                            com.message('error', '车辆删除失败：' + d);
                        }
                    }
                });
            }
        });
    };

    //出库    
    this.outStockClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条记录！');
        if (row.IsPay == "0") return com.message('warning', '车辆未付款，不能出库！');
        if (row.IsOut == "1") return com.message('warning', '车辆已出库，不能重复出库！');

        com.message('confirm', '确定出库吗？', function (b) {
            if (b) {
                com.ajax({
                    type: 'POST',
                    url: vdata.urls.outStock,
                    data: ko.toJSON({ id: row[self.pkey], vin: row["VIN"] }),
                    success: function (d) {
                        if (d === 'ok') {
                            self.searchClick();
                            com.message('success', '车辆出库成功！');
                        } else {
                            com.message('error', '车辆出库失败：' + d);
                        }
                    }
                });
            }
        });
    };
    //审核、反审核
    this.auditClick = function (vm, event) {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条记录！');
        if (row.BillState == "2") return com.message('warning', '车辆已出库，不能审核！');
        var btnStatus = $(event.currentTarget).attr("status");
        if (btnStatus == "1" && row.BillState == "1") return com.message('warning', '不能重复【审核】！');
        if (btnStatus == "0" && row.BillState == "0") return com.message('warning', '不能【反审核】！');
        var tip = btnStatus == "1" ? "审核" : "反审核";
        com.ajax({
            type: 'POST',
            url: vdata.urls.audit,
            data: ko.toJSON({ id: row[self.pkey], status: btnStatus }),
            success: function (d) {
                if (d > 0) {
                    self.searchClick();
                    com.message('success', tip + '成功！');
                } else {
                    com.message('error', tip + '失败：' + d);
                }
            }
        });
    }

    //导出
    this.downloadClick = function (vm, event) {
        var dict = $(event.currentTarget).attr("dict");
        var title = $(document).attr("title");
        com.exporter(self.grid).download('xlsx', dict, title);
    };
};

