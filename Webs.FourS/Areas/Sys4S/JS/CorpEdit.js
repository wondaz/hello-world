using(['linkbutton', 'panel', 'dialog']);
var vms = vms || {};
vms.edit = function (vdata) {
    var self = this;
    this.urls = vdata.urls;
    this.form = {};
    this.readonly = ko.observable(false);
    this.initForm = {};
    this.keyVal = vdata.keyVal;
    this.tipUserCode = ko.observable();
    com.ajax({
        type: 'GET',
        async: false,
        url: self.urls.master + self.keyVal,
        success: function (d) {
            //新增
            if (self.keyVal === 0 || self.keyVal === '0') {
                ko.mapping.fromJS($.extend(d, vdata.defaultForm), {}, self.form);
                self.keyVal = d[vdata.masterKey];
            }
            else {//修改
                self.initForm = d;
                ko.mapping.fromJS(d, {}, self.form);
            }
        }
    });

    if (self.urls.detail) {
        this.grid = $.extend({
            size: { w: 6, h: 177 },
            toolbar: "#gridtb",
            pagination: false,
            remoteSort: false,
            url: ko.observable(self.urls.detail + self.keyVal)
        }, (self.selfGrid || {}));


        this.gridEdit = new com.editGridViewModel(self.grid);

        this.grid.onDblClickRow = function () {
            if (self.readonly()) return;
            self.gridEdit.begin();
        };
    };


    this.refreshClick = function () {
        window.location.reload();
    };

    this.saveClick = function () {
        //if (self.readonly()) return;

        var post = { list: {} }; //传递到后台的数据
        post.form = com.formChanges(self.form, self.initForm, [vdata.masterKey]);

        if (self.urls.detail) {
            self.gridEdit.ended(); //结束grid编辑状态
            post.list = self.gridEdit.getChanges();
        }

        if (com.formValidate() && (post.form._changed || post.list._changed)) {
            com.ajax({
                url: self.urls.save,
                data: ko.toJSON(post),
                success: function (d) {
                    com.message('success', '保存成功');
                    if (d > 1) {
                        post.form[vdata.masterKey] = d;
                        self.keyVal = d;
                    }
                    ko.mapping.fromJS(post.form, {}, self.form); //更新旧值
                    self.initForm = ko.mapping.toJS(self.form);
                    if (self.urls.detail) {
                        self.gridEdit.accept();
                    };
                }
            });
        }
    };
    
};
