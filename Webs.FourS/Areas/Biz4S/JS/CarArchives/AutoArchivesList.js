﻿using(['calendar']);
var vms = vms || {};
vms.search = function (vdata) {
    var self = this;
    this.pkey = vdata.pkey;    
    this.form = ko.mapping.fromJS(vdata.form);
    this.ds = vdata.dataSource;
    this.urls = vdata.urls;
    delete this.form.__ko_mapping__;

    this.grid = $.extend({
        size: { w: 4, h: 68 },
        url: self.urls.search,
        queryParams: ko.observable(),
        pagination: true,
        customLoad: false
    }, (self.selfGrid || {}));

    this.grid.queryParams($.extend({ _xml: vdata._xml }, vdata.form));

    //查询
    this.searchClick = function () {
        var param = ko.toJS(self.form);
        self.grid.queryParams($.extend({ _xml: vdata._xml }, param));
    };

    //清空查询条件
    this.clearClick = function () {
        $.each(self.form, function () { this(''); });
        self.searchClick();
    };

    //新增单据
    //this.addClick = function () {
       // com.openTab('新增工时费', self.urls.edit);
   // };
    //跳转到编辑页面
    this.editClick = function () {
        if (!self.urls.edit) return false;

        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条记录！');
        com.openTab('修改车辆档案', self.urls.edit + row[self.pkey]);
    };

    self.grid.onDblClickRow = this.editClick;

    //保存
    this.saveClick = function () {
        //self.gridEdit.ended();
        if (!com.formValidate()) {
            com.message('warning', '请输入必填项！');
            return;
        }

        com.ajax({
            url: self.urls.save,
            data: ko.toJSON(self.form),
            success: function (d) {
                if (d=='ok') {
                    com.message('success', '保存成功');
                } else {
                    com.message('error', '保存失败:' + d.msg);
                }
            }
        });
    };
    //删除
    //this.deleteClick = function () {
    //    var row = self.grid.datagrid('getSelected');
    //    if (!row) return com.message('warning', '请选择一条记录！');

    //    com.message('confirm', '确定删除吗？', function (b) {
    //        if (b) {
    //            com.ajax({
    //                type: 'DELETE',
    //                url: vdata.urls.delete + row[self.pkey],
    //                success: function (d) {
    //                    if (d === 'ok') {
    //                        self.searchClick();
    //                        com.message('success', '车辆删除成功！');
    //                    } else {
    //                        com.message('error', '车辆删除失败：' + d);
    //                    }
    //                }
    //            });
    //        }
    //    });
    //};

    //导出
    this.downloadClick = function (vm, event) {
        var dict = $(event.currentTarget).attr("dict");
        var title = $(document).attr("title");
        com.exporter(self.grid).download('xlsx', dict, title);
    };
};

