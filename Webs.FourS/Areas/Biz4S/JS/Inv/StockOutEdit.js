using(['linkbutton', 'panel', 'dialog']);
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
        size: { w: 6, h: 157 },
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
        if (self.form.OriCode) {
            self.grid.url = self.urls.detail;
            self.grid.queryParams({ id: self.keyVal });
        }
    };

    this.loadData = function () {
        com.ajax({
            type: 'GET',
            async: false,
            url: self.urls.head + self.keyVal,
            success: function (d) {
                ko.mapping.fromJS(d, {}, self.form);
                delete self.form.__ko_mapping__;
                self.loadDetailData();
            }
        });
    };

    $(function () {
        self.loadData();

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

        if (self.form.OriCode) {
            self.form.OriCode.subscribe(function (newValue) {
                if (newValue) {
                    self.keyVal = newValue;
                    self.loadData();
                }
            });
        }
    });

    this.saveClick = function () {
        if (!com.formValidate()) {
            com.message('warning', '请填写必填项！');
            return false;
        }

        if (self.form["BillState"]() == 1) {
            return com.message('warning', '单据已【审核】，不能修改！');
        }

        var post = {};
        post.head = self.form;
        post.rows = self.grid.datagrid('getRows');
        com.ajax({
            url: self.urls.save,
            data: ko.toJSON(post),
            async: false,
            success: function (d) {
                if (d == 'ok') {
                    com.message('success', '保存成功');
                    self.keyVal = self.form["StockOutCode"]();
                    self.loadData();
                } else {
                    com.message('error', '保存失败：'+d);
                    return false;
                }
            }
        });
        return true;
    };

    //审核、反审核
    this.auditClick = function (vm, event) {
        if (self.form["StockOutID"]() == 0) return com.message('warning', '请保存后，再【审核】！');

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
                if (d ==='ok') {
                    self.loadData();
                    com.message('success', tip + '成功！');
                } else {
                    com.message('error', tip + '失败：' + d);
                }
            }
        });
    }
};
