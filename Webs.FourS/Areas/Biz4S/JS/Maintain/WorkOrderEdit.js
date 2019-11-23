using(['validatebox', 'combobox', 'datebox', 'datetimespinner', 'numberbox', 'dialog']);
var viewModel = function (vdata) {
    var self = this;
    this.urls = vdata.urls;
    this.form = {};
    this.keyVal = vdata.keyVal;
    this.ds = vdata.dataSource;
    vdata.accountList = com.bindCombobox('Account');
    vdata.smallClassList = com.bindCombobox('RepairSmallClass');
    vdata.bigClassList = com.bindCombobox('RepairBigClass');
    vdata.workTeamList = com.bindCombobox('WorkTeam');
    vdata.workShopList = com.bindCombobox('WorkShop');
    this.grid1 = {
        size: { w: 6, h: 355 },
        pagination: false,
        remoteSort: false,
        queryParams: ko.observable()
    };
    this.grid2 = {
        size: { w: 6, h: 355 },
        pagination: false,
        remoteSort: false,
        queryParams: ko.observable()
    };
    this.grid3 = {
        size: { w: 6, h: 355 },
        pagination: false,
        remoteSort: false,
        queryParams: ko.observable()
    };
    this.grid4 = {
        size: { w: 6, h: 355 },
        pagination: false,
        remoteSort: false,
        queryParams: ko.observable()
    };

    self.gridEdit1 = new com.editGridViewModel(self.grid1);
    this.grid1.onDblClickRow = self.gridEdit1.begin;
    this.grid1.onClickRow = self.gridEdit1.ended;

    self.gridEdit2 = new com.editGridViewModel(self.grid2);
    this.grid2.onDblClickRow = self.gridEdit2.begin;
    this.grid2.onClickRow = self.gridEdit2.ended;

    self.gridEdit3 = new com.editGridViewModel(self.grid3);
    this.grid3.onDblClickRow = self.gridEdit3.begin;
    this.grid3.onClickRow = function () {
        self.gridEdit3.ended();
        sumFee(3);
    }

    self.gridEdit4 = new com.editGridViewModel(self.grid4);
    this.grid4.onDblClickRow = self.gridEdit4.begin;
    this.grid4.onClickRow = self.gridEdit4.ended;

    var getAutoArchive = function (signCode) {
        if (!signCode) return;
        com.ajax({
            type: 'GET',
            async: false,
            url: '/api/biz4s/maintain/GetAutoArchive/' + signCode,
            success: function (d) {
                if (d == null) {
                    self.form.CustomerName('');
                    self.form.RepairName('');
                    self.form.MobileTel('');
                    self.form.RepairTel('');
                    self.form.SignCode('');
                    com.message('error', '牌号不存在，请先在【车辆档案】中维护！');
                    return;
                }
                ko.mapping.fromJS(d, {}, self.form);
                self.form.RepairName(self.form.CustomerName());
                self.form.RepairTel(self.form.MobileTel());
            }
        });
    };

    this.loadDetailData = function () {
        self.grid1.url = self.urls.grid1;
        self.grid1.queryParams({ id: self.keyVal });

        self.grid2.url = self.urls.grid2;
        self.grid2.queryParams({ id: self.keyVal });

        self.grid3.url = self.urls.grid3;
        self.grid3.queryParams({ id: self.keyVal });

        self.grid4.url = self.urls.grid4;
        self.grid4.queryParams({ id: self.keyVal });
    };

    this.loadHeadData = function () {
        com.ajax({
            type: 'GET',
            async: false,
            url: self.urls.head + self.keyVal,
            success: function (d) {
                ko.mapping.fromJS(d, {}, self.form);
                delete self.form.__ko_mapping__;
                getAutoArchive(self.form.SignCode());
                self.loadDetailData();
            }
        });
    };

    $(function () {
        self.loadHeadData();
        //计算下次保养里程
        ko.computed(function () {
            if (Number(self.form.RunDistance()) === 0) return;
            var km = Number(self.form.RunDistance()) + 5000;
            self.form.NextMaintainDistance(km);
        });

        ko.computed(function () {
            var addonsFee = self.form.AddonsFee();
            addonsFee = isNaN(addonsFee) ? 0 : addonsFee;
            var amount = self.form.ManHourFee() + self.form.SparepartFee() + addonsFee;
            self.form.Amount(amount);
        });

        self.form.SignCode.subscribe(function (newValue) {
            getAutoArchive(newValue);
        });
    });

    var checkBillState = function () {
        if (self.form["BillState"]() > 0) {
            com.message('warning', '单据已审核，不能修改。');
            return false;
        }
        return true;
    }

    //汇总金额
    var sumFee = function (idx) {
        var fee = 0;
        if (idx == 1) {
            var rows1 = self.grid1.datagrid('getRows');
            for (var i = 0; i < rows1.length; i++) {
                fee += parseFloat(rows1[i].ManHourFee);
            }
            self.form.ManHourFee(fee);
        }

        if (idx == 2) {
            var rows2 = self.grid2.datagrid('getRows');
            for (var i = 0; i < rows2.length; i++) {
                fee += parseFloat(rows2[i].PartFee);
            }
            self.form.SparepartFee(fee);
        }

        if (idx == 3) {
            var rows3 = self.grid3.datagrid('getRows');
            for (var i = 0; i < rows3.length; i++) {
                fee += parseFloat(rows3[i].AppendFee);
            }
            self.form.AddonsFee(fee);
        }
    }

    //新行
    this.addNewRow = function (newRow, index) {
        if (index === 1) {
            self.gridEdit1.addnew(newRow);
            sumFee(1);
        }
        if (index === 2) {
            self.gridEdit2.addnew(newRow);
            sumFee(2);
        }
    }

    this.AppendItemClick = function () {
        self.gridEdit3.addnew();
    }

    this.addCheckClick = function () {
        self.gridEdit4.addnew();
    }

    var deleteGridRow = function (idx) {
        if (idx === '1') {
            self.gridEdit1.deleterow();
        } else if (idx === '2') {
            self.gridEdit2.deleterow();
        } else if (idx === '3') {
            self.gridEdit3.deleterow();
        } else if (idx === '4') {
            self.gridEdit4.deleterow();
        }
    }

    //删除grid条目
    this.deleteClick = function (vm, event) {
        if (!checkBillState()) return false;

        var deleteUrl = '';
        var idx = $(event.currentTarget).attr("gridIndex");
        var row = {};
        if (idx === '1') {
            row = self.grid1.datagrid('getSelected');
            deleteUrl = self.urls.delete1;
        } else if (idx === '2') {
            row = self.grid2.datagrid('getSelected');
            deleteUrl = self.urls.delete2;
        } else if (idx === '3') {
            row = self.grid3.datagrid('getSelected');
            deleteUrl = self.urls.delete3;
        } else if (idx === '4') {
            row = self.grid4.datagrid('getSelected');
            deleteUrl = self.urls.delete4;
        }

        if (!row) return com.message('warning', '请选择一条记录！');

        //新增的数据行
        if (!row["SerialID"] || self.keyVal === '0') {
            deleteGridRow(idx);
        } else {
            com.message('confirm', '确定删除吗？', function (b) {
                if (b) {
                    com.ajax({
                        type: 'POST',
                        url: deleteUrl,
                        data: ko.toJSON({ id: row["SerialID"] }),
                        success: function (d) {
                            if (d === 'ok') {
                                deleteGridRow(idx);
                                com.message('success', '删除成功！');
                            } else {
                                com.message('error', '删除失败：' + d);
                            }
                        }
                    });
                }
            });
        }
        sumFee(idx);
        return true;
    };
    //保存
    var saveData = function (showTip) {
        if (!checkBillState()) return false;

        if (!com.formValidate()) {
            com.message('warning', '请填写必填项！');
            return false;
        }

        self.gridEdit1.ended();
        self.gridEdit2.ended();
        self.gridEdit3.ended();
        self.gridEdit4.ended();
        var post = {};
        delete self.form.__ko_mapping__;
        post.head = self.form;
        post.rows1 = self.grid1.datagrid('getRows');
        post.rows2 = self.grid2.datagrid('getRows');
        post.rows3 = self.grid3.datagrid('getRows');
        post.rows4 = self.grid4.datagrid('getRows');
        com.ajax({
            url: self.urls.save,
            data: ko.toJSON(post),
            async: false,
            success: function (d) {
                if (d.result == 'ok') {
                    if (showTip) {
                        com.message('success', '保存成功');
                    }
                    self.keyVal = d.id;
                    self.form.DispatchID(d.id);
                    self.loadHeadData();
                } else {
                    com.message('error', '保存失败:' + d.result);
                    return false;
                }
            }
        });
        return true;
    }
    this.saveClick = function () {
        saveData(true);
    };

    //审核、反审核
    this.auditClick = function (vm, event) {

        var btnStatus = $(event.currentTarget).attr("status");
        var billState = self.form["BillState"]();
        if (btnStatus === "1" && billState === 1) return com.message('warning', '不能重复【审核】！');
        if (btnStatus === "0" && billState === 0) return com.message('warning', '不能【反审核】！');
        var tip = btnStatus === "1" ? "审核" : "反审核";
        if (self.grid4.datagrid('getRows').length === 0) return com.message('warning', '没有总检信息，不能【审核】！');

        if (btnStatus === "1") {//审核
            saveData(false);
        } else { //反审核
            if (self.form.BillState() == 2) {
                return com.message('warning', '已结算，不能【反审核】！');
            }
        }

        com.ajax({
            type: 'POST',
            url: self.urls.audit,
            data: ko.toJSON({ id: self.keyVal, state: btnStatus }),
            success: function (d) {
                if (d === 'ok') {
                    self.loadHeadData();
                    com.message('success', tip + '成功！');
                } else {
                    com.message('error', tip + '失败：' + d);
                }
            }
        });
    }

    //弹出新增"维修项目"窗口
    this.addRepairItemClick = function () {
        if (!checkBillState()) return false;
        com.dialog({
            title: "新增维修项目",
            width: 450,
            height: 410,
            html: "#addRepairItem-template",
            viewModel: function (w) {
                var thisWin = this;
                this.hform = {};
                thisWin.hform.ManhourCode = ko.observable();
                thisWin.hform.ManhourDescribe = ko.observable();
                thisWin.hform.BillTypeID = ko.observable();
                thisWin.hform.ItemTypeID = ko.observable();
                thisWin.hform.AccountType = ko.observable();
                thisWin.hform.DispatchManhour = ko.observable();
                thisWin.hform.ManHourFee = ko.observable();
                thisWin.hform.ClassID = ko.observable();
                thisWin.hform.WorkShopID = ko.observable();
                thisWin.hform.Remark = ko.observable();

                thisWin.hform.ManhourCode.subscribe(function (newValue) {
                    if (newValue) {
                        var infos = newValue.split('/');
                        if (infos.length !== 4) return;
                        thisWin.hform.ManhourCode(infos[0]);
                        thisWin.hform.ManhourDescribe(infos[1]);
                        thisWin.hform.ManHourFee(infos[2]);
                        thisWin.hform.DispatchManhour(infos[3]);
                    }
                });

                thisWin.confirmClick = function () {
                    if (!thisWin.hform.ManhourDescribe()) {
                        $("#txtManhourCode").focus();
                        com.message('warning', '工时信息错误，请重新输入！');
                        return;
                    }

                    if (!com.formValidate('#divItem')) {
                        com.message('warning', '请填写必填项！');
                        return false;
                    }

                    var newRow = ko.toJS(thisWin.hform);
                    self.addNewRow(newRow, 1);
                    thisWin.hform.ManhourCode('');
                    thisWin.hform.ManhourDescribe('');
                    thisWin.hform.DispatchManhour('');
                    thisWin.hform.ManHourFee('');
                    $("#txtManhourCode").focus();
                    com.message('success', '工时已加入列表');
                };

                thisWin.cancelClick = function () {
                    w.dialog('close');
                };
            }
        });
        return true;
    };
    //弹出新增“维修备件”窗口
    this.addPartsClick = function () {
        if (!checkBillState()) return false;
        com.dialog({
            title: "新增维修备件",
            width: 400,
            height: 330,
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
                thisWin.wform.PartFee = ko.observable();
                thisWin.wform.AccountType = ko.observable();
                thisWin.wform.SpellAb.subscribe(function (newValue) {
                    if (newValue) {
                        var parts = newValue.split('/');
                        if (parts.length !== 2) return;

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
                        $("#txtSpellCode").focus();
                        com.message('warning', '备件信息错误，请重新输入！');
                        return;
                    }

                    if (!com.formValidate('#divParts')) {
                        com.message('warning', '请填写必填项！');
                        return false;
                    }

                    thisWin.wform.PartFee(thisWin.wform.Price() * thisWin.wform.Quantity());
                    var newRow = ko.toJS(thisWin.wform);
                    self.addNewRow(newRow, 2);
                    thisWin.wform.SpellAb('');
                    thisWin.wform.SparePartCode('');
                    thisWin.wform.SparePartName('');
                    thisWin.wform.Spec('');
                    thisWin.wform.Unit('');
                    thisWin.wform.Price('');
                    thisWin.wform.Quantity(1);
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
