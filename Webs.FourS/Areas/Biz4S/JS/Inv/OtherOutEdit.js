using(['panel', 'dialog']);
var vms = vms || {};
vms.edit = function (vdata) {
    var self = this;
    this.urls = vdata.urls;
    this.form = {};
    //this.initForm = {};
    this.readonly = ko.observable(true);
    this.keyVal = vdata.keyVal;
    this.ds = vdata.dataSource;

    this.grid = $.extend({
        size: { w: 6, h: 177 },
        pagination: false,
        remoteSort: false,
        queryParams: ko.observable()
    }, (self.selfGrid || {}));

    self.gridEdit = new com.editGridViewModel(self.grid);
    //单击编辑
    self.grid.onClickCell = function (index, field) {
        self.grid.datagrid('selectRow', index).datagrid('beginEdit', index);;
        var ed = self.grid.datagrid('getEditor', { index: index, field: field });
        if (ed) {
            ($(ed.target).data('textbox') ? $(ed.target).textbox('textbox') : $(ed.target)).focus();
        }

        self.gridEdit.begin();
    };

    this.loadDetailData = function () {
        self.grid.url = self.urls.detail;
        self.grid.queryParams({ id: self.keyVal });
    };

    this.loadData = function () {
        com.ajax({
            type: 'GET',
            async: false,
            url: self.urls.head + self.keyVal,
            success: function (d) {
                if (!d.MobileTel) {//新增 
                    self.readonly(false);
                    ko.mapping.fromJS(d, {}, self.form);
                }
                else {//修改
                    self.readonly(true);
                    //self.initForm = d;
                    ko.mapping.fromJS(d, {}, self.form);
                }
                delete self.form.__ko_mapping__;
                self.loadDetailData();
            }
        });
    };

    $(function () {
        self.loadData();

        self.form.MobileTel.subscribe(function (newValue) {
            if (newValue) {
                com.ajax({
                    type: 'GET',
                    url: '/api/biz4s/parts/GetCustomerByPhone/' + newValue,
                    success: function (d) {
                        if (d != null) {
                            self.form.CustomerName(d.CustomerName);
                            self.form.Address(d.Address);
                        }
                    }
                });
            }
        });

        if (self.form.StockID) {
            self.form.StockID.subscribe(function (newValue) {
                if (newValue) {
                    com.ajax({
                        type: 'GET',
                        url: '/api/biz4s/inv/GetStockKeeper/' + newValue,
                        success: function (d) {
                            self.form.CorpManager(d);
                        }
                    });
                }
            });
        }
    });

    //弹出新增备件窗口
    this.addPartsClick = function () {
        addPartsWin(self.addNewRow);
    };

    //新行
    this.addNewRow = function (newRow) {
        self.gridEdit.addnew({ PKID: 0, SparePartCode: newRow.SparePartCode, SparePartName: newRow.SparePartName, Spec: newRow.Spec, Unit: newRow.Unit, Amount: newRow.Price * newRow.Quantity, Price: newRow.Price, Quantity: newRow.Quantity, Stock: newRow.Stock });
        self.calcTotalAmount();
    }

    //删除
    this.deleteClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条记录！');
        if (row.BillState == "已审核") return com.message('warning', '已审核通过，不能删除！');
        if (row.IsOut == "1") return com.message('warning', '备件已出库，不能删除！');

        if (row["PKID"] === 0) {
            self.gridEdit.deleterow();
            self.calcTotalAmount();
            return;
        }

        com.message('confirm', '确定删除吗？', function (b) {
            if (b) {
                com.ajax({
                    type: 'POST',
                    url: vdata.urls.delete,
                    data: ko.toJSON({ id: row["PKID"] }),
                    success: function (d) {
                        if (d === 'ok') {
                            self.gridEdit.deleterow();
                            self.calcTotalAmount();
                            com.message('success', '删除成功！');
                        } else {
                            com.message('error', '删除失败：' + d);
                        }
                    }
                });
            }
        });
    };

    this.calcTotalAmount = function () {
        var totalAmount = 0;
        $.each(self.grid.datagrid('getRows'), function (i, val) {
            totalAmount += parseFloat(val.Amount);
        });
        self.form.TotalAmount(totalAmount.toFixed(2));
    };

    this.saveClick = function () {
        if (!com.formValidate()) {
            return com.message('warning', '请填写必填项！');
        }

        if (self.form["BillState"]() == "1") {
            return com.message('warning', '单据已【审核】，不能修改！');
        }

        self.gridEdit.ended(); //结束grid编辑状态           
        var post = {};
        post.head = self.form;
        post.rows = self.grid.datagrid('getRows');
        if (post.rows.length === 0) return com.message('warning', '请添加备件！');

        com.ajax({
            url: self.urls.save,
            data: ko.toJSON(post),
            async: false,
            success: function (d) {
                if (d == 'ok') {
                    self.gridEdit.accept();
                    self.keyVal = self.form["StockOutCode"]();
                    com.message('success', '保存成功');
                } else {
                    com.message('error', '保存失败：' + d);
                    return false;
                }
            }
        });

        return true;
    };

    //审核、反审核
    this.auditClick = function (vm, event) {
        if (!self.keyVal || self.keyVal == 0) return com.message('warning', '请保存后，再【审核】！');

        var btnStatus = $(event.currentTarget).attr("status");
        var billState = self.form["BillState"]();
        if (btnStatus === "1" && billState === 1) return com.message('warning', '不能重复【审核】！');
        if (btnStatus === "0" && billState === 0) return com.message('warning', '不能【反审核】！');
        var tip = btnStatus === "1" ? "审核" : "反审核";
        //审核时，判断库存是否足够，反审核不需要判断
        if (btnStatus === "1") {
            var rows = self.grid.datagrid('getRows');
            for (var idx = 0; idx < rows.length; idx++) {
                if (parseInt(rows[idx]["Stock"]) < parseInt(rows[idx]["Quantity"])) {
                    return com.message('warning', '【' + rows[idx]["SparePartName"] + '】库存不足，不能【审核】！');
                }
            }
        }

        com.ajax({
            type: 'POST',
            url: self.urls.audit,
            data: ko.toJSON({ code: self.keyVal, state: btnStatus }),
            success: function (d) {
                if (d === 'ok') {
                    self.loadData();
                    com.message('success', tip + '成功！');
                } else {
                    com.message('error', tip + '失败：' + d);
                }
            }
        });
    }

    var addPartsWin = function (callBack) {
        com.dialog({
            title: "新增备件",
            width: 400,
            height: 360,
            html: "#addParts-template",
            viewModel: function (w) {
                var thisWin = this;
                this.wform = {};
                thisWin.wform.SparePartCode = ko.observable('');
                thisWin.wform.SparePartName = ko.observable('');
                thisWin.wform.Spec = ko.observable('');
                thisWin.wform.Unit = ko.observable('');
                thisWin.wform.Price = ko.observable('');
                thisWin.wform.SpellAb = ko.observable('');
                thisWin.wform.Quantity = ko.observable(1);
                thisWin.wform.Remark = ko.observable('');
                thisWin.wform.Stock = ko.observable(0);

                thisWin.wform.SpellAb.subscribe(function (newValue) {
                    if (newValue) {
                        var parts = newValue.split('/');
                        if (parts.length != 2) return;

                        com.ajax({
                            type: 'GET',
                            async: false,
                            url: '/api/biz4s/parts/GetSpareParts/' + parts[0],
                            success: function (d) {
                                if (d == null) {
                                    thisWin.wform.SparePartCode('');
                                    com.message('warning', '找不到此备件！');
                                    return;
                                }

                                thisWin.wform.SpellAb(d.SpellAb);
                                thisWin.wform.SparePartCode(d.SparePartCode);
                                thisWin.wform.SparePartName(d.SparePartName);
                                thisWin.wform.Spec(d.Spec);
                                thisWin.wform.Unit(d.Unit);
                                thisWin.wform.Price(d.Price);
                                thisWin.wform.Stock(d.Stock);
                                $("#txtQty").focus();
                            }
                        });
                    }
                });

                thisWin.confirmClick = function () {
                    if (!thisWin.wform.SparePartCode()) {
                        $("#txtSpellCode").focus();
                        com.message('warning', '备件信息错误，请重新输入！');
                        return;
                    }

                    var newRow = ko.toJS(thisWin.wform);
                    if (typeof callBack === 'function') {
                        callBack(newRow);
                    }

                    com.message('success', '新增成功！');
                    thisWin.wform.SpellAb('');
                    thisWin.wform.SparePartCode('');
                    thisWin.wform.SparePartName('');
                    thisWin.wform.Spec('');
                    thisWin.wform.Unit('');
                    thisWin.wform.Price('');
                    thisWin.wform.Quantity(1);
                    thisWin.wform.Remark('');
                    thisWin.wform.Stock(0);
                    $("#txtSpellCode").focus();
                };

                thisWin.cancelClick = function () {
                    w.dialog('close');
                };
            }
        });
    };

};
