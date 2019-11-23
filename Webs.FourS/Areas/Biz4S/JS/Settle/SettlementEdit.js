using(['validatebox', 'combobox', 'numberbox']);
var vm = function (vdata) {
    var self = this;
    this.urls = vdata.urls;
    this.form = {};
    this.keyVal = vdata.keyVal;
    this.ds = vdata.dataSource;
    this.isNew = false;
    if (!isNaN(self.keyVal)) {
        self.isNew = true;
    }
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

    //单击编辑
    self.grid1.onClickCell = function (index, field) {
        self.grid1.datagrid('selectRow', index).datagrid('beginEdit', index);
        var ed = self.grid1.datagrid('getEditor', { index: index, field: field });
        if (ed) {
            ($(ed.target).data('textbox') ? $(ed.target).textbox('textbox') : $(ed.target)).focus();
        }

        self.gridEdit1.begin();
    };

    self.gridEdit2 = new com.editGridViewModel(self.grid2);
    //单击编辑
    self.grid2.onClickCell = function (index, field) {
        self.grid2.datagrid('selectRow', index).datagrid('beginEdit', index);
        var ed = self.grid2.datagrid('getEditor', { index: index, field: field });
        if (ed) {
            ($(ed.target).data('textbox') ? $(ed.target).textbox('textbox') : $(ed.target)).focus();
        }

        self.gridEdit2.begin();
    };

    self.gridEdit3 = new com.editGridViewModel(self.grid3);
    //单击编辑
    self.grid3.onClickCell = function (index, field) {
        self.grid3.datagrid('selectRow', index).datagrid('beginEdit', index);
        var ed = self.grid3.datagrid('getEditor', { index: index, field: field });
        if (ed) {
            ($(ed.target).data('textbox') ? $(ed.target).textbox('textbox') : $(ed.target)).focus();
        }

        self.gridEdit3.begin();
    };

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
                    return;
                }
                ko.mapping.fromJS(d, {}, self.form);
            }
        });
    };

    var getMoneySum = function () {
        if (!self.isNew) return;
        if (!self.keyVal || self.keyVal == '0') return;

        com.ajax({
            type: 'GET',
            async: false,
            url: '/api/biz4s/settle/GetMoneySum/' + self.keyVal,
            success: function (d) {
                if (d == null) {
                    self.form.BosomSum('');
                    self.form.CounterclaimSum('');
                    self.form.InsuranceSum('');
                    self.form.AccountReceivable('');
                    return;
                }

                ko.mapping.fromJS(d, {}, self.form);
            }
        });
    }
    this.loadDetailData = function () {
        var dispatchid = self.form.DispatchID();
        self.grid1.url = self.urls.grid1;
        self.grid1.queryParams({ id: dispatchid });

        self.grid2.url = self.urls.grid2;
        self.grid2.queryParams({ id: dispatchid });

        self.grid3.url = self.urls.grid3;
        self.grid3.queryParams({ id: dispatchid });

        self.grid4.url = self.urls.grid4;
        self.grid4.queryParams({ id: dispatchid });
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
                getMoneySum();
                self.loadDetailData();
            }
        });
    };

    $(function () {
        self.loadHeadData();

        if (self.isNew) {//新增
            $('#spanDispatch').hide();
            $('#divDispatch').show();

            self.form.DispatchID.subscribe(function (newValue) {
                if (newValue == null || newValue === '' || self.keyVal === newValue) return;
                self.keyVal = newValue;
                self.loadHeadData();
            });
        }
        else { // 修改
            $('#spanDispatch').show();
            $('#divDispatch').hide();
        }
    });

    var checkBillState = function () {
        if (self.form["BillState"]() > 0) {
            com.message('warning', '单据已审核，不能修改。');
            return false;
        }
        return true;
    }

    var saveData = function (showTip) {
        if (!checkBillState()) return false;

        if (!com.formValidate()) {
            return com.message('warning', '请填写必填项！');
        }

        if (parseFloat(self.form.EarningSum()) > parseFloat(self.form.AccountReceivable())) {
            return com.message('warning', '“实收金额”不能大于“现金金额”！');
        }
        if (parseFloat(self.form.InsuranceSum()) > 0 && !self.form.InsureCorp()) {
            $('#cbxInsureCorp').focus();
            return com.message('warning', '请选择保险公司');
        }

        self.gridEdit1.ended();
        self.gridEdit2.ended();
        self.gridEdit3.ended();
        var post = {};
        delete self.form.__ko_mapping__;

        post.head = {
            BalanceID: self.form.BalanceID(),
            BalanceCode: self.form.BalanceCode(),
            DispatchID: self.form.DispatchID(),
            EarningSum: self.form.EarningSum(),
            InsureCorp: self.form.InsureCorp()
        };

        post.rows1 = self.gridEdit1.getChanges(['SerialID', 'Agio', 'AccountType']).updated;
        post.rows2 = self.gridEdit2.getChanges(['SerialID', 'Agio', 'AccountType']).updated;
        post.rows3 = self.gridEdit3.getChanges(['SerialID', 'AccountType']).updated;
        com.ajax({
            url: self.urls.save,
            data: ko.toJSON(post),
            async: false,
            success: function (d) {
                if (d.result == 'ok') {
                    if (showTip) {
                        com.message('success', '保存成功');
                    }

                    self.form.BalanceID(d.id);
                    $('#divDispatch').hide();
                    $('#spanDispatch').show();
                    self.loadHeadData();

                } else {
                    com.message('error', '保存失败:' + d.result);
                    return false;
                }
            }
        });
        return true;
    };

    //保存
    this.saveClick = function () {
        saveData(true);
    };

    //审核、反审核
    this.auditClick = function (vm, event) {
        if (self.form.BillState() == 2) {
            return com.message('warning', '已结算，不能【审核、反审核】！');
        }

        if (self.form.IsPay()) {
            return com.message('warning', '已付款，不能【审核、反审核】！');
        }
        var btnStatus = $(event.currentTarget).attr("status");
        var billState = self.form["BillState"]();
        if (btnStatus === "1" && billState === 1) return com.message('warning', '不能重复【审核】！');
        if (btnStatus === "0" && billState === 0) return com.message('warning', '不能【反审核】！');
        var tip = btnStatus === "1" ? "审核" : "反审核";

        if (btnStatus === "1") {
            saveData(false);
        }

        com.ajax({
            type: 'POST',
            url: self.urls.audit,
            data: ko.toJSON({ id: self.form.BalanceID(), state: btnStatus }),
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

    this.sumMoney = function () {
        var cash = 0;//现金
        var insurance = 0; //保险
        var claim = 0;//索赔
        var loss = 0;//损耗

        //维修工时
        var manHourAgio = 0;
        var agioManHourFee = 0;
        $.each(self.grid1.datagrid('getData').rows, function () {
            manHourAgio += Number(this['Agio']);
            var agioFee = Number(this['AgioManHourFee']);
            agioManHourFee += agioFee;
            var accountType = this['AccountType'];
            if (accountType === 'cash') {
                cash += agioFee;
            } else if (accountType === 'insurance') {
                insurance += agioFee;
            }
            else if (accountType === 'claim') {
                claim += agioFee;
            }
            else if (accountType === 'loss') {
                loss += agioFee;
            }
        });

        self.form.ManHourAgio(manHourAgio);
        self.form.AgioManHourFee(agioManHourFee);

        //维修备件
        var partsAgio = 0;
        var agioPartFee = 0;
        $.each(self.grid2.datagrid('getData').rows, function () {
            partsAgio += Number(this['Agio']);
            var agioFee = Number(this['AgioPartFee']);
            agioPartFee += agioFee;
            var accountType = this['AccountType'];
            if (accountType === 'cash') {
                cash += agioFee;
            } else if (accountType === 'insurance') {
                insurance += agioFee;
            }
            else if (accountType === 'claim') {
                claim += agioFee;
            }
            else if (accountType === 'loss') {
                loss += agioFee;
            }
        });

        self.form.SparepartAgio(partsAgio);
        self.form.AgioSparepartFee(agioPartFee);
        //附加项目
        $.each(self.grid3.datagrid('getData').rows, function () {
            var appendFee = Number(this['AppendFee']);
            var accountType = this['AccountType'];
            if (accountType === 'cash') {
                cash += appendFee;
            } else if (accountType === 'insurance') {
                insurance += appendFee;
            }
            else if (accountType === 'claim') {
                claim += appendFee;
            }
            else if (accountType === 'loss') {
                loss += appendFee;
            }
        });

        //分类汇总
        self.form.AccountReceivable(cash);
        self.form.InsuranceSum(insurance);
        self.form.CounterclaimSum(claim);
        self.form.BosomSum(loss);
    }
};
