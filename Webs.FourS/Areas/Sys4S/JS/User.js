using(['dialog']);
var viewModel = function (vdata) {
    var self = this;
    this.form = ko.mapping.fromJS({ RoleCode: '' });
    delete this.form.__ko_mapping__;
    this.ds = {};

    this.ds.role = com.dataDict('/api/sys4s/role/GetRoleList/' + vdata.corpID);
    // vdata.deptList = com.dataDict('/api/sys4s/user/getdept');
    this.searchClick = function () {
        var param = ko.toJS(this.form);
        this.grid.queryParams(param);
    };

    this.clearClick = function () {
        $.each(self.form, function () { this(''); });
        this.searchClick();
    };

    this.addClick = function () {
        self.gridEdit.addnew({ CorpID: vdata.corpID });
    };

    this.editClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条数据');

        var index = self.grid.datagrid('getRowIndex', row);
        self.gridEdit.begin(index, row);
    };

    this.deleteClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条数据');
        if (row._isnew) {
            self.gridEdit.deleterow();
            return com.message('success', '已删除');
        }

        com.message('confirm', '确定删除吗？', function (b) {
            if (b) {
                self.gridEdit.deleterow();
                self.saveClick();
            }
        });
    };

    this.grid = {
        size: { w: 4, h: 68 },
        url: "/api/sys4s/user",
        queryParams: ko.observable(),
        pagination: true,
        customLoad: false
    };

    this.gridEdit = new com.editGridViewModel(this.grid);
    this.grid.onDblClickRow = self.gridEdit.begin;
    this.grid.onClickRow = self.gridEdit.ended;
    this.grid.OnAfterCreateEditor = function (editors, row) {
        if (row._isnew == undefined)
            com.readOnlyHandler('input')(editors.UserCode.target, true);
    };

    this.saveClick = function () {
        self.gridEdit.ended();       
        var post = {};
        post.list = self.gridEdit.getChanges();
        if (self.gridEdit.ended() && post.list._changed) {
            com.ajax({
                url: '/api/sys4s/user/edit',
                data: ko.toJSON(post),
                success: function (d) {
                    if (d == 'ok') {
                        self.gridEdit.accept();
                        com.message('success', '保存成功！');
                        self.searchClick();
                    } else {
                        com.message('error', '保存失败:' + d);
                    }

                }
            });
        }
    };

    this.passwordClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请先选择一个用户！');
        com.message('confirm', '确定需要重置密码？', function (b) {
            if (b) {
                com.ajax({
                    type: 'POST',
                    url: '/api/sys4s/user/postresetpassword/' + row.UserCode,
                    success: function () {
                        com.message('success', '密码已重置为<span style="color:red">111111</span>！');
                    }
                });
            }
        });
    };
};

var setRole = function (row) {
    if (row._isnew)
        return com.message('warning', '请先保存再设置角色！');

    com.dialog({
        title: "设置角色",
        width: 600,
        height: 450,
        html: "#setrole-template",
        viewModel: function (w) {
            var thisRole = this;
            this.UserName = ko.observable(row.UserName);
            com.ajax({
                type: 'GET',
                url: '/api/sys4s/user/getUserRole/' + row.UserCode,
                success: function (d) {
                    var ul = w.find(".listview");
                    for (var i in d)
                        ul.append(utils.formatString('<li role="{0}" class="{2}">{1}</li>', d[i].RoleCode, d[i].RoleName, d[i].Checked == 'true' ? 'selected' : ''));
                    ul.find("li").click(function () {
                        if ($(this).hasClass('selected'))
                            $(this).removeClass('selected');
                        else
                            $(this).addClass('selected');
                    });
                }
            });
            this.confirmClick = function () {
                var roles = [];
                w.find("li.selected").each(function () {
                    roles.push({ RoleCode: $(this).attr('role') });
                });
                com.ajax({
                    url: '/api/sys4s/user/edituserroles/' + row.UserCode,
                    data: ko.toJSON(roles),
                    success: function (d) {
                        thisRole.cancelClick();
                        com.message('success', '保存成功！');
                    }
                });
            };
            this.cancelClick = function () {
                w.dialog('close');
            };
        }
    });
};
