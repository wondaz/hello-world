using(['calendar', 'dialog']);
var vms = vms || {};
vms.search = function (vdata) {
    var self = this;
    this.pkey = vdata.pkey;
    //第一个grid查询条件
    this.form = ko.mapping.fromJS(vdata.form);
    //第二个grid查询条件
    this.form2 = ko.mapping.fromJS(vdata.form2);
    this.ds = vdata.dataSource;
    this.urls = vdata.urls;
    delete this.form.__ko_mapping__;
    delete this.form2.__ko_mapping__;

    this.grid1 = {
        size: { w: 4, h: 300 },
        url: self.urls.search,
        queryParams: ko.observable(),
        pagination: true,
        customLoad: false
    };

    this.grid2 = {
        size: { w: 4, h: 300 },
        url: ko.observable(),
        queryParams: ko.observable(),
        pagination: false,
        customLoad: false
    };

    this.grid1.queryParams($.extend({ _xml: vdata._xml1 }, vdata.form));
    //查询
    this.searchClick = function () {
        var param = ko.toJS(self.form);
        self.grid1.queryParams($.extend({ _xml: vdata._xml1 }, param));

        self.grid2.queryParams($.extend({ _xml: vdata._xml2 }, ko.toJS(self.form2)));
    };

    this.grid1.onClickRow = function () {
        var row = self.grid1.datagrid('getSelected');
        if (!row) return;

        self.form2.DispatchCode(row[self.pkey]);
        self.grid2.queryParams($.extend({ _xml: vdata._xml2 }, ko.toJS(self.form2)));
        self.grid2.url(self.urls.search);
    };

    //清空查询条件
    this.clearClick = function () {
        $.each(self.form, function () { this(''); });
        self.searchClick();
    };

    //新增单据
    this.addClick = function () {
        var row = self.grid1.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条付款单据！');;
        addPayItem(row, self.searchClick);
        return true;
    };
};

var addPayItem = function (row, callBack) {
    com.dialog({
        title: "新增收款单",
        width: 550,
        height: 300,
        html: "#addDispatchPayItem-template",
        viewModel: function (w) {
            var thisWin = this;
            this.wform = {};
            com.ajax({
                type: 'GET',
                async: false,
                url: '/api/biz4s/fin/GetPayDetail/0',
                success: function (d) {
                    d = $.extend( d, { SumPaid: 0 });
                    thisWin.wform = ko.mapping.fromJS(d);
                    thisWin.wform.Amount(row.EarningSum);//总金额
                    thisWin.wform.SumPaid(row.SumPaid);//已收款
                    thisWin.wform.PayMoney(row.Debt);//欠款
                    thisWin.wform.VIN(row.VIN);//VIN
                    thisWin.wform.CustomerID(row.CustomerID);//客户ID
                    thisWin.wform.PaymentType('维修付款');
                    thisWin.wform.OriBillCode(row.DispatchCode);
                    delete thisWin.wform.__ko_mapping__;
                }
            });

            this.confirmClick = function () {
                var debt = row.Debt == null ? 0 : row.Debt;
                if (thisWin.wform.PayMoney() > debt) {
                    return com.message('warning', '“本次收款”金额不能大于“应收金额”！');
                }

                if (com.formValidate()) {
                    com.ajax({
                        url: self.vdata.urls.edit,
                        data: ko.toJSON(thisWin.wform),
                        success: function (d) {
                            if (d) {
                                if (d == 'ok') {
                                    w.dialog('close');
                                    com.message('success', '保存成功！');
                                    if (typeof callBack === 'function') {                                        
                                        callBack();
                                    }
                                } else {
                                    com.message('error', '保存失败:' + d);
                                }
                            }
                        }
                    });
                }
            };

            this.cancelClick = function () {
                w.dialog('close');
                if (typeof callBack === 'function') {
                    callBack();
                }
            };
        }
    });
};


