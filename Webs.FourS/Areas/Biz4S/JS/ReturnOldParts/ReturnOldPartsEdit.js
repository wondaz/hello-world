using(['panel', 'dialog']);
var vms = vms || {};
vms.edit = function (vdata) {
    var self = this;
    this.urls = vdata.urls;
    //this.pkey = vdata.pkey;
    this.form1 = ko.mapping.fromJS(vdata.form1);
    this.form = ko.mapping.fromJS(vdata.form);
    this.grid = {
        size: { w: 4, h: 176 },
        pageSize: 20,
        queryParams: ko.observable(),
        pagination: true,
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
        size: { w: 6, h: 220 },
        pagination: false,
        remoteSort: false,
        queryParams: ko.observable()
    };
    self.gridEdit2 = new com.editGridViewModel(self.grid2);
    self.gridEdit1 = new com.editGridViewModel(self.grid1);
    self.gridEdit = new com.editGridViewModel(self.grid);
    this.SelectOldParts = function () {
        $("#OldPartsGrid").window({ title: '选择旧件', width: 1000, height: 400, iconcls: 'icon-add' }).window('open');
    }
    this.searchClick = function () {
        self.grid1.url = self.urls.search;
        var param = ko.toJS(self.form1);
        self.grid1.queryParams($.extend({ _xml: vdata._xml }, param));
    }
    this.clearClick = function () {
        $.each(self.form1, function () { this(''); });
        self.searchClick();
    }

    //列表双击事件
    this.DoubleClick = function () {
        //var row = self.grid.datagrid('getSelected');
        //if (!row) return com.message('warning', '请选择一条备件记录！');
        var row1 = self.grid1.datagrid('getSelected');
        self.gridEdit.addnew({
            ID:'',F_ClaimCode: row1.F_ClaimCode, F_ClaimTime: row1.F_ClaimTime,
            ModelID: row1.ModelID, ModelName: row1.ModelName, VIN: row1.VIN, CustomerName: row1.CustomerName,
            MobileTel: row1.MobileTel, P_PartCodeOld: row1.P_PartCodeOld, P_PartNameOld: row1.P_PartNameOld,
            F_ClaimPrice: row1.F_ClaimPrice, F_Number: row1.F_Number,F_StatusName:'',F_ClaimOrNotName:'',F_ClaimRemarks:''
        });
        //self.grid.onClickCell();
    }
    self.grid1.onDblClickRow = this.DoubleClick;

    this.saveClick = function () {
        if (!com.formValidate()) {
            com.message('warning', '请输入必填项！');
            return;
        }
        var post = {};
        post.head = self.form;
        post.rows = self.gridEdit.getChanges();
        post.rows2 = self.grid2.datagrid('getRows');
        com.ajax({
            url: self.urls.save,
            data: ko.toJSON(post),
            success: function (d) {
                if (d == "ok") {
                    self.grid.url = self.urls.search;
                    self.grid.queryParams({ _xml: vdata._xml0, F_Mid: vdata.form.F_PKId });
                    self.grid2.url = self.urls.grid2;
                    self.grid2.queryParams({ id: vdata.form.F_PKId });
                    com.message('success', '保存成功');
                    //self.grid.queryParams($.extend({ _xml: vdata._xml0 }, param));
                } else {
                    com.message('error', '保存失败:' + d.msg);
                }
            }
        });
    }
    this.deleteClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条记录！');
        if (row["ID"] === '') {
            self.gridEdit.deleterow();
            return;
        }
        var post = {};
        post.id = row["ID"];
        com.message('confirm', '确定删除吗？', function (b) {
            if (b) {
                com.ajax({
                    type: 'Post',
                    url: vdata.urls.delete,
                    data: ko.toJSON(post),
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
    }
    this.loadDetailData = function () {
        self.grid.url = self.urls.search;
        var param = ko.toJS(self.form);
        self.grid.queryParams({ _xml: vdata._xml0, F_Mid: vdata.form.F_PKId });
        //页面加载查询相关联的附件
        self.grid2.url = self.urls.grid2;
        self.grid2.queryParams({ id: vdata.form.F_PKId });
    }
    var checkBillState = function () {
        var status = self.form.F_Status();
        if (status !== 0 && status !== 3 && status !== null && status !=="") {
            com.message('warning', '单据已在处理中，不能修改。');
            return false;
        }
        return true;
    }
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
    this.deleteAttachClick = function () {
        if (!checkBillState()) return false;
        row = self.grid2.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条记录！');
        //新增的数据行
        if (!row["F_PKId"]) {
            self.gridEdit2.deleterow();
            return true;
        }
        var post = {};
        post.id = row["F_PKId"];
        com.ajax({
            url: self.urls.deleteAttach,
            data: ko.toJSON(post),
            success: function (d) {
                if (d == "ok") {
                    self.grid2.url = self.urls.grid2;
                    self.grid2.queryParams({ id: vdata.form.F_PKId });
                    com.message('success', '删除成功');
                    //self.grid.queryParams($.extend({ _xml: vdata._xml0 }, param));
                } else {
                    com.message('error', '删除失败:' + d.msg);
                }
            }
        });
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
    $(function () {
        self.loadDetailData();
    });
}