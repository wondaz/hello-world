using(['dialog']);
var viewModel = function (vdata) {
    var self = this;
    var rowState = 0;//0新增 1编辑
    this.grid = {
        size: { w: 189, h: 40 },
        url: "/api/sys4s/code/getlist",
        queryParams: ko.observable(),
        pagination: true,
        idField: 'ID',
        loadFilter: function (data) {
            data.rows = utils.toTreeData(data.rows, 'ID', 'ParentID', "children");
            return data;
        }
    };

    this.gridEdit = new com.editTreeGridViewModel(this.grid);
    this.grid.onClickRow = self.gridEdit.ended;
    this.grid.OnAfterCreateEditor = function (edt) {
        if (rowState === 1) {
            com.readOnlyHandler('input')(edt["Value"].target, true);
        }
    };

    this.tree = {
        method: 'GET',
        url: '/api/sys4s/code/getcodetype',
        queryParams: ko.observable(),
        loadFilter: function (d) {
            var filter = utils.filterProperties(d.rows || d, ['CodeType as id', 'CodeTypeName as text']);
            return [{ id: '', text: '所有类别', children: filter }];
        },
        onSelect: function (node) {
            self.CodeType(node.id);
            self.CodeTypeName(node.text);
        }
    };

    this.CodeType = ko.observable();
    this.CodeTypeName = ko.observable();
    this.CodeType.subscribe(function (value) {
        self.grid.queryParams({ CodeType: value });
    });

    this.refreshClick = function () {
        window.location.reload();
    };

    this.addClick = function () {
        if (!self.CodeType()) return com.message('warning', '请先在左边选择要添加的类别！');
        com.ajax({
            type: 'GET',
            url: '/api/sys4s/code/getnewcode',
            success: function (d) {
                var row = { CodeType: self.CodeType(), CodeTypeName: self.CodeTypeName(), ID: d, CorpID: vdata.corpid };
                rowState = 0;
                self.gridEdit.addnew(row);
            }
        });
    };

    this.editClick = function () {
        var row = self.grid.treegrid('getSelected');
        if (!row) return com.message('warning', '请选择一条数据');
        if (row.CorpID === 1) return com.message('warning', '系统数据，不能修改！');

        rowState = 1;
        self.gridEdit.begin(row);
    };

    this.grid.onDblClickRow = this.editClick;

    //this.deleteClick = self.gridEdit.deleterow;

    this.deleteClick = function () {
        var row = self.grid.treegrid('getSelected');
        if (!row) return com.message('warning', '请选择一条数据');
        if (row.CorpID === 1) return com.message('warning', '系统数据，不能删除！');

        com.message('confirm', '确定删除吗？', function (b) {
            if (b) {
                self.gridEdit.deleterow();
                self.saveClick();
                //com.ajax({
                //    type: 'DELETE',
                //    url: '/api/sys4s/code/deleteDict/' + row.ID,
                //    success: function (d) {
                //        if (d > 0) {
                //            self.gridEdit.deleterow();
                //            com.message('success', '删除成功！');
                //        }
                //    }
                //});
            }
        });
    };

    this.saveClick = function () {
        self.gridEdit.ended();
        var post = {};
        post.list = self.gridEdit.getChanges(['ID','CorpID', 'CodeType', 'CodeTypeName', 'Value', 'Text', 'IsEnable', 'IsDefault', 'Seq']);
        if (self.gridEdit.isChangedAndValid) {
            com.ajax({
                url: '/api/sys4s/code/edit',
                data: ko.toJSON(post),
                success: function (d) {
                    com.message('success', '保存成功！');
                    //self.grid.queryParams({ CodeType: self.CodeType() });
                    self.gridEdit.accept();
                }
            });
        }
    };
    this.typeClick = function () {
        if (vdata.corpid !== 1) return com.message('warning', '权限不足');;
        com.dialog({
            title: "&nbsp;字典类别",
            iconCls: 'icon-node_tree',
            width: 612,
            height: 412,
            html: "#type-template",
            viewModel: function (w) {
                var that = this;
                that.grid = {
                    width: 586,
                    height: 340,
                    pagination: true,
                    pageSize: 10,
                    rownumbers:true,
                    url: "/api/sys4s/code/getcodetype",
                    queryParams: ko.observable()
                };
                that.gridEdit = new com.editGridViewModel(that.grid);
                that.grid.OnAfterCreateEditor = function (editors, row) {
                    if (!row._isnew) com.readOnlyHandler('input')(editors["CodeType"].target, true);
                };
                that.grid.onClickRow = that.gridEdit.ended;
                that.grid.onDblClickRow = that.gridEdit.begin;
                that.grid.toolbar = [
                    { text: '新增', iconCls: 'icon-add1', handler: function () { that.gridEdit.addnew({ CorpID: vdata.corpid }); } }, '-',
                    { text: '编辑', iconCls: 'icon-edit', handler: that.gridEdit.begin }, '-',
                    { text: '删除', iconCls: 'icon-cross', handler: that.gridEdit.deleterow }
                ];
                this.confirmClick = function () {
                    if (that.gridEdit.isChangedAndValid()) {
                        var list = that.gridEdit.getChanges(['CorpID','_id', 'CodeType', 'CodeTypeName', 'Description', 'Seq']);
                        com.ajax({
                            url: '/api/sys4s/code/editcodetype/',
                            data: ko.toJSON({ list: list }),
                            success: function (d) {
                                that.cancelClick();
                                self.tree.$element().tree('reload');
                                com.message('success', '保存成功！');
                            }
                        });
                    }
                };
                this.cancelClick = function () {
                    w.window('close');
                };
            }
        });
    };
};