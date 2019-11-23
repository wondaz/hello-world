
using(['panel', 'dialog']);
var vms = vms || {};
vms.edit = function (vdata) {
    var self = this;
    this.pkey = vdata.pkey;
    this.form = ko.mapping.fromJS(vdata.form);
    this.form1 = ko.mapping.fromJS(vdata.form1);
    this.form2 = ko.mapping.fromJS(vdata.form2);
    this.form3 = ko.mapping.fromJS(vdata.form3);
    this.form5 = ko.mapping.fromJS(vdata.form5);
    this.ds = vdata.dataSource;
    this.urls = vdata.urls;
    delete this.form.__ko_mapping__;
    vdata.B_PartTypeName = com.bindCombobox('B_PartTypeName');
    this.grid = {
        size: { w: 4, h: 495 },
        pageSize: 20,
        queryParams: ko.observable(),
        pagination: true,
        customLoad: false,
        remoteSort: false
    };
    this.grid1 = {
        size: { w: 4, h: 92 },
        //url: self.urls.search,
        pageSize: 20,
        queryParams: ko.observable(),
        pagination: true,
        remoteSort: false
    };
    this.grid2 = {
        size: { w: 4, h: 92 },
        //url: self.urls.search,
        pageSize: 20,
        queryParams: ko.observable(),
        pagination: true,
        remoteSort: false
    };
    this.grid3 = {
        size: { w: 4, h: 92 },
        //url: self.urls.search,
        pageSize: 20,
        queryParams: ko.observable(),
        pagination: true,
        remoteSort: false
    };
    this.grid5 = {
        size: { w: 4, h: 80 },
        pageSize: 20,
        queryParams: ko.observable(),
        pagination: true,
        remoteSort:false
    };
    self.gridEdit5 = new com.editGridViewModel(self.grid5);
    self.gridEdit3 = new com.editGridViewModel(self.grid3);
    self.gridEdit2 = new com.editGridViewModel(self.grid2);
    self.gridEdit1 = new com.editGridViewModel(self.grid1);
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
    //查询辅料关联
    this.flglsearchClick = function () {
        self.grid3.url = self.urls.search;
        var param = ko.toJS(self.form3);
        self.grid3.queryParams($.extend({ _xml: vdata._xml3 }, param));
    };
    //查询工时关联
    this.gssearchClick = function () {
        self.grid2.url = self.urls.search;
        var param = ko.toJS(self.form2);
        self.grid2.queryParams($.extend({ _xml: vdata._xml2 }, param));
    };
    //查询
    this.searchClick = function () {
        
        self.grid1.url = self.urls.search;
        var param = ko.toJS(self.form1);
        self.grid1.queryParams($.extend({ _xml: vdata._xml1 }, param));
    };
    //清空
    this.clearClick = function () {
        $.each(self.form1, function () { this(''); });
        self.searchClick();
    };
    this.gsclearClick = function () {
        $.each(self.form2, function () { this(''); });
        self.searchClick();
    }
    this.flglclearClick = function () {
        $.each(self.form3, function () { this(''); });
        self.searchClick();
    }
    //弹出新增备件窗口
    this.addPartsClick = function () {
        addPartsWin(self.addNewRow);
    };
    this.addPartsJjClick = function () {
        addPartsJjWin(self.addNewRow);
    };
    //新行
    this.addNewRow = function (newRow) {
        //self.gridEdit.addnew({ PKID: 0, SparePartCode: newRow.SparePartCode, SparePartName: newRow.SparePartName, Spec: newRow.Spec, Unit: newRow.Unit, Amount: newRow.Price * newRow.Quantity, Price: newRow.Price, Quantity: newRow.Quantity, Stock: newRow.Stock });
        self.gridEdit.addnew({
            PKID: 0, F_FaultCode: '', F_FaultName: '', B_DealerName: '', P_PartCodeOld: newRow.SparePartCode,
            P_PartCode: newRow.SparePartCode, P_PartNameOld: newRow.SparePartName, P_PartName: newRow.SparePartName,
            F_ClaimPrice: newRow.Price, F_Number: newRow.Quantity, F_Total: Number(newRow.Price) * Number(newRow.Quantity),
            B_PartTypeName: '', F_IngredientCode: '', F_IngredientName: '', F_HourCode: '', F_HourName: ''
            //, ManHour: 0, DispatchManHour: 0,AddonsManHour:0,SumManHour:0
        });
        //self.calcTotalAmount();
    }
    //保存
    this.saveClick = function () {
        self.gridEdit.ended();
        if (!com.formValidate()) {
            com.message('warning', '请输入必填项！');
            return;
        }
        
        var post = {};
        post.head = self.form;
        post.rows = self.gridEdit.getChanges();
        com.ajax({
            url: self.urls.save,
            data: ko.toJSON(post),
            success: function (d) {
                if (d=="ok") {
                    com.message('success', '保存成功');
                } else {
                    com.message('error', '保存失败:' + d.msg);
                }
            }
        });
    };
    //提交索赔信息
    this.SubmitClick = function () {
        self.gridEdit.ended();
        if (!com.formValidate()) {
            com.message('warning', '请输入必填项！');
            return;
        }

        var post = {};
        post.head = self.form;
        post.rows = self.gridEdit.getChanges();
        com.ajax({
            url: self.urls.Submit,
            data: ko.toJSON(post),
            success: function (d) {
                if (d == "ok") {
                    com.message('success', '保存成功');
                    self.form.F_Status("1");
                } else {
                    com.message('error', '保存失败:' + d.msg);
                }
            }
        });
    };
    
    //删除
    this.deleteClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条记录！');
        //if (row.BillState == "已审核") return com.message('warning', '已审核通过，不能删除！');
       // if (row.IsOut == "1") return com.message('warning', '备件已出库，不能删除！');

        if (row["F_ClaimPartID"] === 0) {
            self.gridEdit.deleterow();
            return;
        }
        var post = {};
        post.id = row["F_ClaimPartID"];
        com.message('confirm', '确定删除吗？', function (b) {
            if (b) {
                com.ajax({
                    type: 'Post',
                    url: vdata.urls.delete,
                    data:ko.toJSON(post),
                    success: function (d) {
                        if (d === 'ok') {
                            self.gridEdit.deleterow();
                            //self.calcTotalAmount();
                            com.message('success', '删除成功！');
                        } else {
                            com.message('error', '删除失败：' + d);
                        }
                    }
                });
            }
        });
    };
    this.ClearGrid = function (o) {
        try {
            var row = self.grid.datagrid('getSelected');
            if (!row) return com.message('warning', '请选择一条记录！');
            switch (o) {
                case 1:
                    row.F_HourCode = '';
                    row.F_HourName = '';
                    self.grid.onClickCell();
                    com.message('success', '清空成功！');
                    break
                case 2:
                    row.F_IngredientCode = '';
                    row.F_IngredientName = '';
                    self.grid.onClickCell();
                    com.message('success', '清空成功！');
                    break;
                case 3:
                    row.P_PartCodeOld = '';
                    break;
            }
        }catch(ex){}
    };
    
    this.ClearGsGlClick = function (object) {
        alert(object);
    };
    self.form.B_ChassisCode.subscribe(function (newValue) {
        if (!newValue) return;
        com.ajax({
            type: 'GET',
            async: false,
            url: self.urls.archive + newValue,
            success: function (d) {
                if (d) {
                    self.form.ModelID(d['ModelID']);
                    self.form.EngineCode(d['EngineCode']);
                    self.form.CustomerName(d['CustomerName']);
                    self.form.MobileTel(d['MobileTel']);
                    self.form.SellerIdentityCard(d['SellerIdentityCard']);
                    self.form.Address(d['Address']);
                    self.form.SaleDate(d['SaleDate']); 
                    self.form.F_ClaimTime(d['F_ClaimTime']);
                    self.form.F_otherMoney(d['F_otherMoney']);
                    self.form.F_outMoney(d['F_outMoney']);
                    self.form.F_hourMoney(d['F_hourMoney']);
                    self.form.F_Total(d['F_Total']);
                    self.form.B_LicensePlate(d['B_LicensePlate']);
                }
            }
        });

    });
    var addPartsWin = function (callBack) {
        com.dialog({
            title: "新增备件",
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
    var addPartsJjWin = function (callBack) {
        com.dialog({
            title: "新增备件",
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
                    var row = self.grid.datagrid('getSelected');
                    if (!row) return com.message('warning', '请选择一条备件记录！');
                    var newRow = ko.toJS(thisWin.wform);
                    row.P_PartCodeOld = newRow.SparePartCode;
                    self.grid.onClickCell();
                    com.message('success', '新增成功！');
                    w.dialog('close');
                };
                thisWin.cancelClick = function () {
                    w.dialog('close');
                };
            }
        });
    };
    //列表双击事件
    this.DoubleClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条备件记录！');
        var row1 = self.grid1.datagrid('getSelected');
        row.F_FaultCode = row1.PB_FaultCode;
        row.F_FaultName = row1.PB_FaultName;
        $("#cesi").dialog('close');
        self.grid.onClickCell();
    }
    self.grid1.onDblClickRow = this.DoubleClick;
    //工时关联列表双击
    this.GsGlDouBleCliCK = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条备件记录！');
        var row2 = self.grid2.datagrid('getSelected');
        row.F_HourCode = row2.ManHourCode;
        row.F_HourName = row2.ManHourDescribe;
        $("#gsgl").dialog('close');
        self.grid.onClickCell();
    }
    self.grid2.onDblClickRow = this.GsGlDouBleCliCK;
    //关联辅料
    this.GlgLDoubClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条备件记录！');
        var row3 = self.grid3.datagrid('getSelected');
        row.F_IngredientCode = row3.F_IngredientCode;
        row.F_IngredientName = row3.F_IngredientName;
        $("#flgl").dialog('close');
        self.grid.onClickCell();
    }
    self.grid3.onDblClickRow = this.GlgLDoubClick;
    //故障关联
    this.addParGzDm = function () {
        $("#cesi").window({ title: '故障关联', width: 1000, height: 400, iconcls: 'icon-add' }).window('open');
    }
    //工时关联
    this.addParGsGl = function () {
        $("#gsgl").window({ title: '工时关联', width: 1000, height: 400, iconcls: 'icon-add' }).window('open');
    }
    //辅料关联
    this.addParFlgl = function () {
        $("#flgl").window({ title: '辅料关联', width: 1000, height: 400, iconcls: 'icon-add' }).window('open');
    }
    this.loadDetailData = function () {
        self.grid.url = self.urls.search;
        self.grid.queryParams({ _xml: vdata._xml4, F_ClaimCode: vdata.form.F_ClaimCode });
        self.grid5.url = self.urls.search;
        //self.grid5.queryParams({ _xml: vdata._xml5, B_ChassisCode: vdata.form.B_ChassisCode });
        var param = ko.toJS(self.form);
        self.grid5.queryParams($.extend({ _xml: vdata._xml5 }, param));
    };
    $(function () {
        self.loadDetailData();
    });
    /*面板选中
    $('#tt').tabs({
        border: true,
        onSelect: function (title) {
            alert(1);
            if (title == "历史维修记录") {
            }
        }
    });*/
};

