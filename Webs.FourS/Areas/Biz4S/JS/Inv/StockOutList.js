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

    //新增单据
    this.addClick = function () {
        com.openTab('新增备件销售订单', self.urls.edit1);
    };
    //跳转到编辑页面
    this.editClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条记录！');

        if (row[self.pkey].substring(0, 4) == 'QTCK') {
            com.openTab('其他出库单', self.urls.edit2 + row[self.pkey]);
        }
        else {
            com.openTab('销售出库单', self.urls.edit1 + row[self.pkey]);
        }
    };

    self.grid.onDblClickRow = this.editClick;

    //删除
    this.deleteClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条记录！');
        if (row.BillState === 1) return com.message('warning', '已审核通过，不能删除！');

        com.message('confirm', '确定删除吗？', function (b) {
            if (b) {
                com.ajax({
                    type: 'POST',
                    url: self.urls.delete,
                    data: ko.toJSON({ id: row['StockOutCode'] }),
                    success: function (d) {
                        if (d === 'ok') {
                            self.searchClick();
                            com.message('success', '删除成功！');
                        } else {
                            com.message('error', '删除失败：' + d);
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

        var btnStatus = $(event.currentTarget).attr("status");
        var billState = row.BillState;
        if (btnStatus === "1" && billState === 1) return com.message('warning', '不能重复【审核】！');
        if (btnStatus === "0" && billState === 0) return com.message('warning', '不能【反审核】！');
        var tip = btnStatus === "1" ? "审核" : "反审核";
        //审核时，判断库存是否足够，反审核不需要判断
        if (btnStatus === "1" && row.IsUnderStock > 0) {
            return com.message('warning', '备件库存不足，不能【审核】！');
        }

        com.ajax({
            type: 'POST',
            url: self.urls.audit,
            data: ko.toJSON({ code: row.StockOutCode, state: btnStatus }),
            success: function (d) {
                if (d === 'ok') {
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

