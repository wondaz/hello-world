using(['panel', 'dialog']);
var vms = vms || {};
vms.edit = function (vdata) {
    var self = this;
    this.urls = vdata.urls;
    this.form = {};
    //this.initForm = {};
    //this.readonly = ko.observable(true);
    this.keyVal = vdata.keyVal;
    this.ds = vdata.dataSource;

    this.grid = $.extend({
        size: { w: 6, h: 157 },
        pagination: false,
        remoteSort: false,
        queryParams: ko.observable()
    }, (self.selfGrid || {}));

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
                //self.readonly(true);
                ko.mapping.fromJS(d, {}, self.form);
                delete self.form.__ko_mapping__;
                self.loadDetailData();
            }
        });
    };

    $(function () {
        self.loadData();
    });

    var checkBillState = function () {
        if (self.form["BillState"]() > 0) {
            com.message('warning', '单据已审核，不能修改。');
            return false;
        }
        return true;
    }

    self.gridEdit = new com.editGridViewModel(self.grid);
    //单击编辑
    self.grid.onClickCell = function (index, field) {
        if (!checkBillState()) return false;
        self.grid.datagrid('selectRow', index).datagrid('beginEdit', index);;
        var ed = self.grid.datagrid('getEditor', { index: index, field: field });
        if (ed) {
            ($(ed.target).data('textbox') ? $(ed.target).textbox('textbox') : $(ed.target)).focus();
        }

        self.gridEdit.begin();
        return true;
    };

    //新行
    this.addNewRow = function (newRow) {
        self.gridEdit.addnew({ SerialID: 0, SparePartCode: newRow.SparePartCode, SparePartName: newRow.SparePartName, Spec: newRow.Spec, Unit: newRow.Unit, Price: newRow.Price, Quantity: newRow.Quantity, TradeQty: newRow.Quantity, Remark: newRow.Remark });
    }

    //删除
    this.deleteClick = function () {
        if (!checkBillState()) return false;

        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条记录！');
        if (row["SerialID"] === 0) {
            self.gridEdit.deleterow();
            return true;
        }

        com.message('confirm', '确定删除吗？', function (b) {
            if (b) {
                com.ajax({
                    type: 'POST',
                    url: self.urls.delete,
                    data: ko.toJSON({ id: row["SerialID"] }),
                    success: function (d) {
                        if (d === 'ok') {
                            self.gridEdit.deleterow();
                            com.message('success', '删除成功！');
                        } else {
                            com.message('error', '删除失败：' + d);
                        }
                    }
                });
            }
        });
        return true;
    };
    //保存
    this.saveClick = function () {
        if (!checkBillState()) return false;

        if (!com.formValidate()) {
            com.message('warning', '请填写必填项！');
            return false;
        }

        self.gridEdit.ended();
        var post = {};
        post.head = self.form;
        post.rows = self.grid.datagrid('getRows');
        com.ajax({
            url: self.urls.save,
            data: ko.toJSON(post),
            async: false,
            success: function (d) {
                if (d.result == 'ok') {
                    com.message('success', '保存成功');
                    self.keyVal = d.stockInID;
                    self.form["StockInID"](d.stockInID);
                } else {
                    com.message('error', '保存失败:' + d.result);
                    return false;
                }
            }
        });
        return true;
    };

    //审核、反审核
    this.auditClick = function (vm, event) {
        if (self.form["StockInID"]() === 0) return com.message('warning', '请保存后，再【审核】！');

        var btnStatus = $(event.currentTarget).attr("status");
        var billState = self.form["BillState"]();
        if (btnStatus === "1" && billState === 1) return com.message('warning', '不能重复【审核】！');
        if (btnStatus === "0" && billState === 0) return com.message('warning', '不能【反审核】！');
        var tip = btnStatus === "1" ? "审核" : "反审核";

        com.ajax({
            type: 'POST',
            url: self.urls.audit,
            data: ko.toJSON({ id: self.keyVal, state: btnStatus }),
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

    //弹出新增备件窗口
    this.addPartsClick = function () {
        if (!checkBillState()) return false;
        com.dialog({
            title: "新增备件",
            width: 400,
            height: 350,
            height: 340,
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
                    self.addNewRow(newRow);
                    thisWin.wform.SpellAb('');
                    thisWin.wform.SparePartCode('');
                    thisWin.wform.SparePartName('');
                    thisWin.wform.Spec('');
                    thisWin.wform.Unit('');
                    thisWin.wform.Price('');
                    thisWin.wform.Quantity(1);
                    thisWin.wform.Remark('');
                    $("#txtSpellCode").focus();
                    com.message('success', '已加入列表');
                };

                thisWin.cancelClick = function () {
                    w.dialog('close');
                };
            }
        });
        return true;
    };
};
