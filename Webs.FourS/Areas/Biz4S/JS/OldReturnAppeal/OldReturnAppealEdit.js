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
        size: { w: 4, h: 176 },
        pageSize: 20,
        queryParams: ko.observable(),
        pagination: true,
        remoteSort: false
    };
    self.gridEdit1 = new com.editGridViewModel(self.grid1);
    self.gridEdit = new com.editGridViewModel(self.grid);

    this.saveClick = function () {
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
                if (d == "ok") {
                    self.grid.url = self.urls.search;
                    var param = ko.toJS(self.form);
                    self.grid.queryParams({ _xml: vdata._xml1, F_Mid: vdata.form.F_PKId });
                    com.message('success', '保存成功');
                } else {
                    com.message('error', '保存失败:' + d.msg);
                }
            }
        });
    }
    this.SelectOldParts = function () {
        $("#OldAppealPartsGrid").window({ title: '选择旧件', width: 1000, height: 400, iconcls: 'icon-add' }).window('open');
    }
    this.searchClick = function () {
        self.grid1.url = self.urls.search;
        var param = ko.toJS(self.form1);
        self.grid1.queryParams($.extend({ _xml: vdata._xml }, param));
    }
    
    this.deleteClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条备件记录！');
        var row = self.grid.datagrid('getSelected');
        if (row["F_PKId"] === '') {
            self.gridEdit.deleterow();
            return;
        }
        var post = {};
        post.id = row["F_PKId"];
        com.message('confirm', '确定删除吗？', function (b) {
            if (b) {
                com.ajax({
                    type: 'Post',
                    url: vdata.urls.delete,
                    data:ko.toJSON(post),
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
    }
    this.clearClick = function ()
    {
        $.each(self.form1, function () { this(''); });
        self.searchClick();
    }
    //列表双击事件
    this.DoubleClick = function () {
        //var row = self.grid.datagrid('getSelected');
        //if (!row) return com.message('warning', '请选择一条备件记录！');
        var row1 = self.grid1.datagrid('getSelected');
        self.gridEdit.addnew({
            F_PKId: '', F_ClaimCode: row1.F_ClaimCode, F_OldPartsCode: row1.F_OldPartsCode,
            F_ClaimTime: row1.F_ClaimTime, F_OldReturnTime: row1.F_OldReturnTime, P_PartCode: row1.P_PartCode, P_PartName: row1.P_PartName,
            F_ClaimPrice: row1.F_ClaimPrice, F_Number: row1.F_Number, F_ClaimOrNot: row1.F_ClaimOrNot,
            CheckedName: row1.CheckedName
        });
        //self.grid.onClickCell();
    }
    self.grid1.onDblClickRow = this.DoubleClick;

    this.loadDetailData = function () {
        self.grid.url = self.urls.search;
        //var param = ko.toJS(self.form);
        self.grid.queryParams({ _xml: vdata._xml1, F_Mid: vdata.form.F_PKId });
    }
    $(function () {
        self.loadDetailData();
    });
}