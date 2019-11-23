using(['validatebox', 'combobox', 'datebox', 'numberbox', 'dialog']);
var viewModel = function (vdata) {
    var self = this;
    this.urls = vdata.urls;
    this.form = {};
    this.keyVal = vdata.keyVal;
    this.ds = vdata.dataSource;

    this.grid1 = {
        size: { w: 6, h: 220 },
        pagination: false,
        remoteSort: false,
        queryParams: ko.observable()
    };
    this.grid2 = {
        size: { w: 6, h: 220 },
        pagination: false,
        remoteSort: false,
        queryParams: ko.observable()
    };

    self.gridEdit1 = new com.editGridViewModel(self.grid1);

    self.gridEdit2 = new com.editGridViewModel(self.grid2);

    this.loadDetailData = function () {
        self.grid1.url = self.urls.grid1;
        self.grid1.queryParams({ id: self.keyVal });

        self.grid2.url = self.urls.grid2;
        self.grid2.queryParams({ id: self.keyVal });
    };

    this.loadHeadData = function () {
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
        self.loadHeadData();
    });

    var checkBillState = function () {
        var status = self.form.F_Status();
        if (status !== 0 && status !== 3) {
            com.message('warning', '单据已在处理中，不能修改。');
            return false;
        }
        return true;
    }

    //汇总金额
    var sumFee = function () {
        var money = 0;
        var rows1 = self.grid1.datagrid('getRows');
        for (var i = 0; i < rows1.length; i++) {
            money += parseFloat(rows1[i].F_Money);
        }
        self.form.F_SumMoney(money);
    }

    //退货单-增加新行
    this.addNewRow = function (rows) {
        var gridRows = self.grid1.datagrid('getRows');
        for (var i in rows) {
            if (rows.hasOwnProperty(i)) {
                var newRow = rows[i];
                var isExist = false;
                for (var k in gridRows) {
                    if (gridRows.hasOwnProperty(k)) {
                        if (gridRows[k].F_OldCode === newRow.F_OldPartsCode) {
                            isExist = true;
                            break;
                        }
                    }
                }

                if (isExist) continue;
                self.gridEdit1.addnew({
                    F_PKId: '',
                    F_OldId: newRow.F_PKId,
                    F_CarryName: newRow.F_CarryName,
                    F_CarryNumber: newRow.F_DeliveryNumber,
                    F_Money: newRow.F_Money,
                    F_OldCode: newRow.F_OldPartsCode,
                    F_OldTime: newRow.F_OldReturnTime
                });
            }
        }

        sumFee();
    }

    //附件增加新行
    this.addAttachsRow = function (d) {
        self.gridEdit2.addnew({
            F_PKId: '',
            F_AttaUrl: d.fileFullPath,
            F_AttaOriginName: d.fileName,
            F_AttaSize: parseFloat(d.fileSize).toFixed(2) + 'M',
            F_AttaType: d.fileExtension,
            F_AttaCusUrl: d.relativePath
        });
    }
    
    var deleteGridRow = function (idx) {
        if (idx === '1') {
            self.gridEdit1.deleterow();
        } else if (idx === '2') {
            self.gridEdit2.deleterow();
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
        }

        if (!row) return com.message('warning', '请选择一条记录！');

        //新增的数据行
        if (!row["F_PKId"]) {
            deleteGridRow(idx);
            sumFee();
            return true;
        }

        com.message('confirm', '确定删除吗？', function (b) {
            if (b) {
                com.ajax({
                    type: 'POST',
                    async: false,
                    url: deleteUrl,
                    data: ko.toJSON({ id: row["F_PKId"] }),
                    success: function (d) {
                        if (d === 'ok') {
                            deleteGridRow(idx);
                            sumFee();
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
    this.saveClick = function (vm, event) {
        if (!checkBillState()) return false;

        if (!com.formValidate()) {
            com.message('warning', '请填写必填项！');
            return false;
        }

        self.gridEdit1.ended();
        self.gridEdit2.ended();
        var post = {};
        delete self.form.__ko_mapping__;
        post.head = self.form;
        post.rows1 = self.grid1.datagrid('getRows');
        if (post.rows1.length === 0) {
            return com.message('warning', '请添加“旧件退回单”！');
        }

        post.rows2 = self.grid2.datagrid('getRows');
        var btnStatus = $(event.currentTarget).attr("status");
        self.form.F_Status(btnStatus);
        com.ajax({
            url: self.urls.save,
            data: ko.toJSON(post),
            success: function (d) {
                if (d.result == 'ok') {
                    com.message('success', btnStatus === '0' ? '保存成功' : '提交成功');

                    self.keyVal = d.id;
                    self.form.F_PKId(d.id);
                    self.loadHeadData();
                } else {
                    com.message('error', '操作失败:' + d.result);
                    return false;
                }
            }
        });

        return true;
    };

    this.uploadFiles = function () {
        if (!checkBillState()) return false;

        var file = document.getElementById("upload1").files[0];
        var formData = new FormData();
        formData.append("photo", file);
        com.ajax({
            url: self.urls.upload,
            type: 'POST',
            processData: false,
            contentType: false,
            data: formData,
            success: function (d) {
                if (d.result === '') {
                    return false;
                }
                if (d.result === 'ok') {
                    self.addAttachsRow(d);
                    com.message('success', '上传文件成功');
                } else {
                    com.message('error', '上传文件失败:' + d.result);
                    return false;
                }
            }
        });
    }

    //弹出新增"维修项目"窗口
    this.oldReturnClick = function () {
        com.dialog({
            title: "&nbsp;旧件退回申请单",
            iconCls: 'icon-node_tree',
            width: 645,
            height: 412,
            html: "#oldCode-template",
            viewModel: function (w) {
                var that = this;
                that.grid3 = {
                    width: 619,
                    height: 340,
                    singleSelect: false,
                    pagination: true,
                    pageSize: 20,
                    rownumbers: true,
                    url: vdata.urls.grid3,
                    queryParams: ko.observable()
                };

                this.confirmClick = function () {
                    var rows = that.grid3.datagrid('getChecked');
                    self.addNewRow(rows);
                    w.window('close');
                };
                this.cancelClick = function () {
                    w.window('close');
                };
            }
        });
    };
};
