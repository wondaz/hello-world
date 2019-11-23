using(['calendar']);
var vms = vms || {};
vms.search = function (vdata) {
    var self = this;
    this.pkey = vdata.pkey;
    this.form = ko.mapping.fromJS(vdata.form);
    this.ds = vdata.dataSource;
    this.urls = vdata.urls;
    delete this.form.__ko_mapping__;

    this.grid = $.extend({
        size: { w: 4, h: 92 },
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

    this.clearClick = function () {
        $.each(self.form, function () { this(''); });
        self.searchClick();
    }
    this.addClick = function () {
        com.openTab('新增整车信息', self.urls.edit);
    }
    this.editClick = function () {
        if (!self.urls.edit) return false;

        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条记录！');
        com.openTab('修改整车退销信息', self.urls.edit + row["ID"]);
    }
    self.grid.onDblClickRow = this.editClick;
    this.deleteClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条记录！');
        var post = {};
        post.id = row[self.pkey];
        com.message('confirm', '确定删除吗？', function (b) {
            if (b) {
                com.ajax({
                    type: 'Post',
                    url: vdata.urls.delete,
                    data:ko.toJSON(post),
                    success: function (d) {
                        if (d === 'ok') {
                            self.searchClick();
                            com.message('success', '整车退销信息删除成功！');
                        } else {
                            com.message('error', '整车退销信息删除失败：' + d);
                        }
                    }
                });
            }
        });
    }
    this.downloadClick = function () {
        var dict = $(event.currentTarget).attr("dict");
        var title = $(document).attr("title");
        com.exporter(self.grid).download('xlsx', dict, title);
    }
}