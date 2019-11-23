using(['dialog']);
var vms = vms || {};
vms.search = function (data) {
    var self = this;
    this.pkField = data.pkField || "ID";
    this.urls = data.urls;
    this.resx = data.resx;
    this.form = ko.mapping.fromJS(data.form);
    delete this.form.__ko_mapping__;

    this.grid = $.extend({
        size: { w: 4, h: 94 },
        url: data.urls.search,
        queryParams: ko.observable(),
        pagination: true,
        customLoad: false,
    }, (self.selfGrid || {}));

    this.grid.queryParams($.extend({ token: com.dmstoken, _xml: data._xml }, data.form));

    this.searchClick = function () {
        var param = ko.toJS(self.form);
        self.grid.queryParams($.extend({ token: com.dmstoken, _xml: data._xml }, param));
    };

    $(document).keydown(function (e) {
        var enterKey = "13", key = e.keyCode || e.which || e.charCode; //兼容IE(e.keyCode)和Firefox(e.which)
        if (key == enterKey) {
            if ($('#a_search')) {
                $('#a_search').focus();
            }
            self.searchClick();
            e.preventDefault();

            if ($('#txtCode')) {
                $('#txtCode').focus();
            }

            self.grid.datagrid('selectRow', 0);
        }
        return true;
    });

    this.clearClick = function () {
        //$.each(self.form, function () { this(''); });
        //$('#condition').find(':input').val('');
        //解决隐含的固定条件
        $.each($('#condition').find(':input'), function () {
            if (this.dataset && this.dataset.bind) {
                var bindTxt = this.dataset.bind.split('.');
                var field = bindTxt[bindTxt.length - 1];
                self.form[field]('');
            }
        });
        this.searchClick();
    };

    this.addClick = function () {
        com.openTab('编辑企业信息', self.urls.edit + '0');
    };

    //this.editClick = function () {
    //    if (!self.urls.edit) return true;

    //    var row = self.grid.datagrid('getSelected');
    //    if (!row) return com.message('warning', '请选择一条记录！');
    //    com.openTab('编辑企业信息', self.urls.edit + row[self.pkField]);
    //};

    this.editTreeClick = function () {
        if (!self.urls.edit) return true;

        var row = self.grid.treegrid('getSelected');
        if (!row) return com.message('warning', '请选择一条记录');
        if (self.urls.edit != undefined) {
            //parent.tabmanager.add({ name: '编辑企业信息', url: self.urls.edit + row[self.pkField] });
            com.openTab('编辑企业信息', self.urls.edit + row[self.pkField]);
        }
    };

    this.deleteClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', self.resx.noneSelect);
        com.message('confirm', self.resx.deleteConfirm, function (b) {
            if (b) {
                com.ajax({
                    type: 'POST',
                    url: self.urls.remove,
                    data: { id: row[self.pkField] },
                    success: function () {
                        com.message('success', self.resx.deleteSuccess);
                        self.searchClick();
                    }
                });
            }
        });
    };

    this.downloadClick = function (vm, event) {
        var dict = $(event.currentTarget).attr("dict");
        var title = $(document).attr("title");
        com.exporter(self.grid).download('xlsx', dict, title);
    };
};
