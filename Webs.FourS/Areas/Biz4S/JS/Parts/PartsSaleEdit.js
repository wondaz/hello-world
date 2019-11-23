using(['numberbox', 'linkbutton', 'panel', 'dialog']);
var vms = vms || {};
vms.edit = function (vdata) {
    var self = this;
    this.urls = vdata.urls;
    this.form = {};
    this.initForm = {};
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
        self.grid.queryParams({ _xml: vdata._xml, SellOrderCode: self.keyVal });
    };

    this.loadData = function () {
        com.ajax({
            type: 'GET',
            async: false,
            url: self.urls.head + self.keyVal,
            success: function (d) {
                if (!d.MobileTel) {//新增 
                    self.readonly(false);
                    self.keyVal = d.SellOrderCode;
                    ko.mapping.fromJS(d, {}, self.form);
                }
                else {//修改
                    self.readonly(true);
                    self.initForm = d;
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
                        if (d == null) {
                            return;
                        }

                        self.form.CustomerName(d.CustomerName);
                        self.form.Address(d.Address);
                    }
                });
            }
        });
    });

    //弹出新增备件窗口
    this.addPartsClick = function () {
        if (self.form["BillState"]() == "1") {
            return com.message('warning', '单据已【审核】，不能新增备件！');
        }

        if (self.form.BillState() == 2) {
            return com.message('warning', '车辆已【出库】，不能新增备件！');
        }

        addPartsWin(self.keyVal);
    };

    this.saveHead = function () {
        if (!com.formValidate()) {
            com.message('warning', '请填写必填项！');
            return false;
        }

        var formData = com.formChanges(self.form, self.initForm);
        if (formData._changed) {
            com.ajax({
                url: self.urls.saveHead,
                data: ko.toJSON(self.form),
                async: false,
                success: function (d) {
                    if (d == 'ok') {
                        com.message('success', '保存成功');
                        ko.mapping.fromJS(formData, {}, self.form); //更新旧值
                        self.initForm = ko.mapping.toJS(self.form);
                    } else {
                        com.message('error', d);
                        return false;
                    }
                }
            });
        }

        return true;
    };

    //删除
    this.deleteClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条记录！');
        if (self.form.BillState() > 0) return com.message('warning', '已审核通过，不能删除！');

        com.message('confirm', '确定删除吗？', function (b) {
            if (b) {
                com.ajax({
                    type: 'POST',
                    url: vdata.urls.delete,
                    data: ko.toJSON({ id: row[vdata.pkey] }),
                    success: function (d) {
                        if (d === 'ok') {
                            self.gridEdit.deleterow();
                            com.message('success', '删除成功！');
                            self.calcTotalAmount();
                        } else {
                            com.message('error', '删除失败：' + d);
                        }
                    }
                });
            }
        });
    };

    this.saveClick = function () {
        var billState = self.form.BillState();
        if (billState == 1) {
            return com.message('warning', '单据已【审核】，不能修改！');
        }

        if (billState == 2) {
            return com.message('warning', '车辆已【出库】，不能修改！');
        }

        self.gridEdit.ended(); //结束grid编辑状态           
        var rowCount = self.grid.datagrid('getRows').length;
        if (rowCount === 0) return com.message('warning', '请添加备件！');

        if (!self.saveHead()) return;
        var list = self.gridEdit.getChanges();
        if (list._changed) {
            com.ajax({
                url: self.urls.saveDetail,
                data: ko.toJSON(list.updated),
                async: false,
                success: function (d) {
                    if (d == 'ok') {
                        self.gridEdit.accept();
                        com.message('success', '保存成功');
                        self.loadData();
                    } else {
                        com.message('保存失败：', d);
                        return false;
                    }
                }
            });
        }

        return true;
    };

    //审核、反审核
    this.auditClick = function (vm, event) {
        var billState = self.form.BillState();
        if (billState == 2) return com.message('warning', '备件已出库，不能审核！');
        var btnStatus = $(event.currentTarget).attr("status");
        if (btnStatus == "1" && billState == 1) return com.message('warning', '不能重复【审核】！');
        if (btnStatus == "0" && billState == 0) return com.message('warning', '不能【反审核】！');
        var tip = btnStatus == "1" ? "审核" : "反审核";
        com.ajax({
            type: 'POST',
            url: vdata.urls.audit,
            data: ko.toJSON({ code: self.form.SellOrderCode(), status: btnStatus }),
            success: function (d) {
                if (d > 0) {
                    self.loadData();
                    com.message('success', tip + '成功！');
                } else {
                    com.message('error', tip + '失败：' + d);
                }
            }
        });
    }


    var addPartsWin = function (sellOrderCode) {
        com.dialog({
            title: "新增备件销售单",
            width: 400,
            height: 340,
            html: "#addParts-template",
            viewModel: function (w) {
                var thisWin = this;
                this.wform = {};
                thisWin.wform.SellOrderCode = ko.observable(sellOrderCode);
                thisWin.wform.SparePartCode = ko.observable('');
                thisWin.wform.SparePartName = ko.observable('');
                thisWin.wform.Spec = ko.observable('');
                thisWin.wform.Unit = ko.observable('');
                thisWin.wform.Price = ko.observable('');
                thisWin.wform.SpellAb = ko.observable('');
                thisWin.wform.Quantity = ko.observable(1);
                thisWin.wform.Remark = ko.observable('');

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
                            }
                        });
                    }
                });

                thisWin.confirmClick = function () {
                    if (!thisWin.wform.SparePartCode()) {
                        com.message('warning', '备件信息错误，请重新输入！');
                        return;
                    }

                    com.ajax({
                        url: vdata.urls.saveDetail,
                        data: ko.toJSON([thisWin.wform]),
                        success: function (d) {
                            if (d) {
                                if (d == 'ok') {
                                    com.message('success', '新增成功！');
                                    thisWin.wform.SpellAb('');
                                    thisWin.wform.SparePartCode('');
                                    thisWin.wform.SparePartName('');
                                    thisWin.wform.Spec('');
                                    thisWin.wform.Unit('');
                                    thisWin.wform.Price('');
                                    thisWin.wform.Quantity(1);
                                    thisWin.wform.Remark('');
                                    $("#txtSpellCode").focus();
                                    self.loadDetailData();

                                } else {
                                    com.message('error', '新增失败:' + d);
                                }
                            }
                        }
                    });
                };

                thisWin.cancelClick = function () {
                    w.dialog('close');
                    self.calcTotalAmount();
                };
            }
        });
    };

    this.calcTotalAmount = function () {
        if (!self.grid.datagrid) return;

        var totalAmount = 0;
        $.each(self.grid.datagrid('getRows'), function (i, val) {
            totalAmount += parseFloat(val.Amount);
        });
        self.form.TotalAmount(totalAmount.toFixed(2));
    };
};
