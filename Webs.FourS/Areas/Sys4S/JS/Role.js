using(['dialog']);
function viewModel(vdata) {
    var self = this;
    this.gridlist = $('#gridlist');
    this.grid = {
        size: { w: 4, h: 40 },
        url: '/api/sys4s/role/GetRoleList',
        queryParams: ko.observable(),
        loadFilter: function (d) {
            for (var i in d) {
                d[i].CollaborationID = vdata.collaborationID;
                d[i].UnionID = vdata.unionID;
                d[i].CorpLevel = vdata.corpLevel;
            }
            return { rows: d, total: d.length };
        }
    };

    this.gridEdit = new com.editGridViewModel(self.grid);
    this.grid.onDblClickRow = self.gridEdit.begin;
    this.grid.onClickRow = self.gridEdit.ended;
    //this.grid.OnAfterCreateEditor = function (editors, row) {
    //    if (row._isnew == undefined)
    //        com.readOnlyHandler('input')(editors.RoleCode.target, true);
    //};
    this.refreshClick = function () {
        window.location.reload();
    };

    this.addClick = function () {
        self.gridEdit.addnew({ CorpID: vdata.corpID, IsSystemRole: 0,RoleType:'自定义角色' });
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
        if (row.IsSystemRole) return  com.message('warning', '系统角色，不能删除。');

        com.message('confirm', '确定删除吗？', function (b) {
            if (b) {
                self.gridEdit.deleterow();
                self.saveClick();
            }
        });
    };

    this.saveClick = function () {
        self.gridEdit.ended();
        var post = {};
        post.list = self.gridEdit.getChanges();
        if (self.gridEdit.ended() && post.list._changed) {
            com.ajax({
                url: '/api/sys4s/role/edit',
                data: ko.toJSON(post),
                success: function (d) {
                    com.message('success', '保存成功！');
                    self.gridEdit.accept();
                    self.refreshClick();
                }
            });
        }
    };
}

var permissionTab = function (row) {
    com.dialog({
        title: "角色授限",
        width: 660,
        height: 455,
        html: "#permission-template",
        viewModel: function (win) {
            var self = this;
            this.role = ko.mapping.fromJS(row);
            this.tab = {
                onSelect: function (title, index) {
                    if (title == '按钮权限') {
                        //取得菜单权限中的选中行，并重新加开到按钮权限列表中
                        var temp = {}, data = [], panel = self.grid2.treegrid('getPanel');
                        utils.eachTreeRow(self.grid.treegrid('getData'), function (node) {
                            if (node.menuchecked) {
                                data.push(utils.filterProperties(node, ['children', 'Description'], true));
                                temp[node.MenuCode] = node;
                            }
                        });
                        self.grid2.treegrid('loadData', data);

                        //checkbox点击处理函数
                        var checkHandler = function (obj, value) {
                            if (!obj.length) return;
                            var map = { "0": "/Content/images/checknomark.gif", "1": "/Content/images/checkmark.gif" };
                            obj.attr("src", map[value]).attr("value", value);
                            temp[obj.attr("MenuCode")]["btn_" + obj.attr("ButtonCode")] = parseInt(obj.attr("value"));
                        };

                        //注册checkbox点击事件
                        panel.find("td[field]").unbind("click").click(function () {
                            var img = $(this).find("img"), value = img.attr("value") == "1" ? "0" : "1";
                            checkHandler(img, value);

                            if (img.attr("ButtonCode") == "_checkall")
                                panel.find("img[MenuCode=" + img.attr("MenuCode") + "]").each(function () {
                                    checkHandler($(this), value);
                                });
                        });

                        //注册全选checkbox的事件
                        panel.find(".datagrid-header .icon-chk_unchecked").unbind("click").click(function () {
                            var chk = $(this),
                                value = chk.hasClass("icon-chk_checked") ? "0" : "1",
                                iconcls = chk.hasClass("icon-chk_checked") ? "icon-chk_unchecked" : "icon-chk_checked";
                            chk.removeClass("icon-chk_unchecked").removeClass("icon-chk_checked").addClass(iconcls);

                            panel.find("img").each(function () {
                                checkHandler($(this), value);
                            });
                        });
                    }
                }
            };

            this.grid = {
                height: 310,
                width: 630,
                url: '/api/sys4s/menu/GetMenusAndButtons',
                idField: 'MenuCode',
                queryParams: ko.observable(),
                treeField: 'MenuName',
                singleSelect: false,
                onCheck: function (node) {
                    node.checked = true;
                    node.menuchecked = true;
                },
                onUncheck: function (node) {
                    node.checked = false;
                    node.menuchecked = false;
                },
                onCheckAll: function (rows) {
                    utils.eachTreeRow(rows, function (node) {
                        node.checked = true;
                        node.menuchecked = true;
                    });
                },
                onUncheckAll: function (rows) {
                    utils.eachTreeRow(rows, function (node) {
                        node.checked = false;
                        node.menuchecked = false;
                    });
                },
                loadFilter: function (d) {
                    var formatterChk = function (ButtonCode) {
                        return function (value, row) {
                            if (value >= 0)
                                return '<img MenuCode="' + row.MenuCode + '" ButtonCode="' + ButtonCode + '" value="' + value + '" src="/Content/images/' + (value ? "checkmark.gif" : "checknomark.gif") + '"/>';
                        };
                    }
                    var cols = [[]];
                    for (var i in d.buttons)
                        cols[0].push({ field: 'btn_' + d.buttons[i].ButtonCode, width: 50, align: 'center', title: utils.formatString('<span >{0}</span>', d.buttons[i].ButtonName), formatter: formatterChk(d.buttons[i].ButtonCode) });
                    self.grid2.columns(cols);

                    for (var k in d.menus) {
                        d.menus[k].checked = d.menus[k].menuchecked == 1 ? true : false;
                    }
                    return utils.toTreeData(d.menus, 'MenuCode', 'ParentCode', "children");
                }
            };

            this.grid.queryParams({ collaborationID: row.CollaborationID, roleCode: row.RoleCode, unionID: row.UnionID, corpLevel: row.CorpLevel, corpID: row.CorpID });

            this.grid2 = {
                height: 310,
                width: 630,
                idField: 'MenuCode',
                treeField: 'MenuName',
                frozenColumns: [[
                    { field: 'MenuName', width: 150, title: '菜单' },
                    {
                        field: 'btn__checkall',
                        width: 50,
                        align: 'center',
                        title: '<span class="icon icon-chk_unchecked">全选</span>',
                        formatter: function (v, r) {
                            for (var i in r) {
                                if (i.indexOf("btn_") > -1 && r[i] > -1) {
                                    return '<img MenuCode="' + r.MenuCode + '" ButtonCode="_checkall" src="/Content/images/' + (v ? "checkmark.gif" : "checknomark.gif") + '"/>';
                                }
                            }
                        }
                    }
                ]],
                columns: ko.observableArray(),
                loadFilter: function (d) {
                    return utils.toTreeData(d, 'MenuCode', 'ParentCode', "children");
                }
            };

            this.confirmClick = function () {
                var post = { menus: [], buttons: [] };
                utils.eachTreeRow(self.grid.treegrid('getData'), function (node) {
                    if (node.menuchecked && node.ParentCode >= 0) {
                        //1 取得菜单权限数据  
                        post.menus.push({ CorpID: row.CorpID, MenuCode: node.MenuCode, RoleCode: row.RoleCode });

                        //2 取得按钮权限数据
                        for (var btn in node)
                            if (btn.substr(0, 4) == 'btn_' && node[btn] == '1' && btn != 'btn__checkall')
                                post.buttons.push({ CorpID: row.CorpID, MenuCode: node.MenuCode, ButtonCode: btn.split('_')[1], RoleCode: row.RoleCode });
                    }
                });

                com.ajax({
                    url: '/api/sys4s/role/editpermission/' + row.RoleID,
                    data: ko.toJSON(post),
                    success: function (d) {
                        self.cancelClick();
                        com.message('success', '保存成功！');
                    }
                });

            };
            this.cancelClick = function () {
                win.dialog('close');
            };

        }
    });
};